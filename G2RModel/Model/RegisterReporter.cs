using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using G2RModel.Model;
using SimpleGedcomLib;
using WpdInterfaceLib;

namespace Ged2Reg.Model
{
    public class RegisterReporter
    {
        public CitationCoordinator CitationCoordinator { get; internal set; }
        public ReportStats MyReportStats { get; set; }

        // former fields, retargeted to the new tree builder
        public ListOfReportEntry[] Generations => _tree?.Generations;
        private int _lastSlotWithPeople => _tree?.LastSlotWithPeople ?? 0;
        private List<GedcomIndividual> _nonContinued => _tree?.NonContinued;

        private ReportContext _c;
        private CitationsMap _cm;
        private Dictionary<StyleSlots, Formatting> _styleMap;
        private GenealogicalDateFormatter _dateFormatter;
        private GenealogicalPlaceFormatter _placeFormatter;

        private string[] _wordsForNumbers =
        {
            "",
            "first",
            "second",
            "third",
            "fourth",
            "fifth",
            // yeah, right. way to go, mon!
        };
        private char[] _splitSpace = { ' ' };
        private int _currentGeneration;
        private Regex _rexDivider1 = new Regex(@"^[\-_]+$");
        private Regex _rexDivider2 = new Regex(@"^[=]+$");

        private bool _factDesc;
        private bool _reduceChild;
        private bool _listBapt;
        private bool _listBuri;

        private bool _generationalReducePlaceNames;
        private bool _standardReducePlaceNames;
        private bool _standardBriefContdChild;

        private bool _ancestryReport;
        private bool _suppressGenNumbers;
        private bool _includeGenerationNumbers;
        private bool _allFamilies;
        private bool _allowMultiple;
        private bool _omitFocusSpouses;
        private int _minFromGen;
        private bool _omitBackRefs;
        private int _maxLivingGenerations;


        private ReportTreeBuilder _tree;
        private Formatting _childNameFormatting;
        public RegisterReportModel Model { get; set; }

        public RegisterReporter Init(GedcomIndividual root, TimeSpan prep)
        {
            MyReportStats = new ReportStats().Init(prep);
            _c = ReportContext.Instance;

            _factDesc = _c.Settings.IncludeFactDescriptions;
            _reduceChild = _c.Settings.ReduceContinuedChildren;
            _listBapt = _c.Settings.IncludeBaptism;
            _listBuri = _c.Settings.IncludeBurial;
            _ancestryReport = _c.Settings.AncestorsReport;
            _suppressGenNumbers = _c.Settings.SuppressGenNbrs;
            _includeGenerationNumbers = _c.Settings.GenerationPrefix && _ancestryReport;
            _allFamilies = !_c.Settings.AncestorsReport || _c.Settings.AllFamilies;
            _allowMultiple = _c.Settings.AllowMultipleAppearances;
            _omitFocusSpouses = _c.Settings.OmitFocusSpouses;
            _minFromGen = _c.Settings.MinimizeFromGeneration;
            _omitBackRefs = _c.Settings.OmitBackRefs && _ancestryReport;
            _maxLivingGenerations = _c.Settings.AssumedMaxLivingGenerations;

            
            _generationalReducePlaceNames = _c.Settings.FullPlaceOncePerGen;
            _standardReducePlaceNames = _c.Settings.ReducePlaceNames && !_c.Settings.FullPlaceOncePerGen;
            _standardBriefContdChild = _c.Settings.StandardBriefContinued;

            ReportEntryFactory.Init(!_allowMultiple);

            _cm = new CitationsMap(Model.GedcomFile);
            CitationCoordinator = new CitationCoordinator(Model.GedcomFile.CitationViews)
            {
                IncludeBurial = _listBuri,
                Priorities = _c.Settings.SourcePriorities,
                AnitPriorities = _c.Settings.SourceAnitPriorities,
            };
            int oldState = CitationCoordinator.Reset();

            Model.PostProgress("Building report tree");
            _tree = new ReportTreeBuilder()
            {
                AllFamilies = _allFamilies,
                AllowMultiple = _allowMultiple,
                Cm = _cm,
                Mode = _ancestryReport ? TreeMode.Ancestors : TreeMode.Descendants,
                Root = root
            };
            _tree.Init().Exec();

            if (_c.Settings.Focus && _ancestryReport)
            {
                Model.PostProgress($"Focus report tree on {_c.Settings.FocusName}");
                _tree.ApplyReduction(_c.Settings.FocusId, _c.Settings.ContinuePastFocus);
            }

            if (Model.CheckCancel()) throw new CanceledByUserException();
            Model.PostProgress("Analyzing source citations for main persons");

            foreach (ListOfReportEntry gen in Generations)
            {
                if ((gen?.Count??0) == 0)
                    continue;
                CitationCoordinator.PreProcess(gen.ToListOfIndividuals());
            }
            if (Model.CheckCancel()) throw new CanceledByUserException();
            Model.PostProgress("Analyzing source citations for non-continued persons");
            CitationCoordinator.PreProcess(_nonContinued);

            Model.PostProgress("Selecting source citations");
            CitationCoordinator.PreProcess(_nonContinued);
            CitationCoordinator.Process(_c.Settings.CitationStrategy, _c.Settings.FillinCitationStrategy);

            return this;
        }

        internal void ProcessLivingStatus()
        {
            if (!(GedcomIndividual.ConsiderLivingStatus = _c.Settings.ObscureLiving))
                return;
            
            Model.PostProgress($"obscure names of (possibly) living persons");

            List<ReportEntry> res = GatherReportEntries();
            int i = 0;
            foreach (ReportEntry re in res)
            {
                i++;
                re?.Individual?.EvalLivingStatus();
            }
            ApplyNotLivingOverride();
        }

        internal List<ReportEntry> GatherReportEntries()
        {
            List<ReportEntry> rvl = new List<ReportEntry>();

            foreach (ListOfReportEntry generation in Generations)
            {
                if (generation == null)
                    break;
                foreach (ReportEntry reportEntry in generation)
                {
                    rvl.AddRange(reportEntry.GatherVisiblePersons());
                }
            }

            return rvl;
        }

        private void ApplyNotLivingOverride()
        {

            int ix0 = !_ancestryReport ? 0 : _maxLivingGenerations;
            int ixn = !_ancestryReport 
                ? _lastSlotWithPeople - _maxLivingGenerations
                : _lastSlotWithPeople;

            for (int i = ix0; i <= ixn; i++)
            {
                foreach (ReportEntry re in Generations[i])
                {
                    re.OverrideAsNotLiving();
                }
            }
        }

        public void Exec(IWpdDocument doc)
        {
            _styleMap = new Dictionary<StyleSlots, Formatting>();
            foreach (StyleAssignment s in _c.Settings.StyleMap)
            {
                _styleMap.Add(s.Style, new Formatting(){CharacterStyleName = s.StyleName});
            }

            _styleMap.TryGetValue(StyleSlots.GenerationDivider, out Formatting genDivider);
            _styleMap.TryGetValue(StyleSlots.GenerationDivider3Plus, out Formatting genDivider3Plus);
            _styleMap.TryGetValue(StyleSlots.ChildName, out _childNameFormatting);

            _dateFormatter = new GenealogicalDateFormatter();
            _placeFormatter = new GenealogicalPlaceFormatter()
            {
                DropUSA = _c.Settings.DropUsa, 
                InjectWordCounty = _c.Settings.InjectCounty,
                ReduceOnRepetition = _generationalReducePlaceNames || _standardReducePlaceNames,
            }.Init();

            _currentGeneration = 0;
            foreach (ListOfReportEntry generation in Generations)
            {
                _currentGeneration++;
                if (Model.CheckCancel()) throw new CanceledByUserException();
                Debug.WriteLine($"Generation {_currentGeneration} begins {DateTime.Now:G}");
                if (_generationalReducePlaceNames)
                    _placeFormatter.Reset();
                if ((generation?.Count??0)==0)
                    continue;
                Model.PostProgress($"processing generation {_currentGeneration}");

                if (_c.Settings.GenerationHeadings)
                    EmitDivider(doc, genDivider, genDivider3Plus);

                foreach (ReportEntry individual in generation)
                {
                    if (_standardReducePlaceNames)
                        _placeFormatter.Reset();
                    EmitMainPerson(doc, individual, _currentGeneration);
                }
            }

            var didIx = ConditionallyEmitIndexField(doc, _c.Settings.NameIndexSettings);
            if (!(didIx?.SingleIndexOnly??false)) didIx = ConditionallyEmitIndexField(doc, _c.Settings.PlaceIndexSettings) ?? didIx;
            if (_c.Settings.ReportSummary || (didIx != null && _c.Settings.AsEndnotes))
                doc.InsertPageBreak();

            // this has to be done now to accurate reporting
            // side effect: the ui does not update correctly, it 
            // appears that the binding events are ignored (wrong thread?)
            _c.Settings.LastRun = MyReportStats.EndTime = DateTime.Now;
            _c.Settings.LastRunTimeSpan = MyReportStats.PrepTime.Add(MyReportStats.ReportTime);
            _c.Settings.LastFileCreated = _c.Settings.OutFile;

            if (!_c.Settings.ReportSummary) return;

            var p = doc.InsertParagraph();
            p.StyleName = "ReportInfo";
            p.Append(_c.Settings.ReportKeySettings());
            p = doc.InsertParagraph();
            p.StyleName = "ReportInfo";
            p.Append(GetStatsSummary().Replace("\t", ""));
        }

        private void EmitDivider(IWpdDocument doc, Formatting genDivider, Formatting genDivider3Plus)
        {
            IWpdParagraph para;
            switch (_currentGeneration)
            {
                case 0:
                case 1:
                    break;
                case 2:
                    para = doc.InsertParagraph($"Generation {_currentGeneration}");
                    para.StyleName = genDivider?.CharacterStyleName;
                    break;
                default:
                    para = doc.InsertParagraph($"Generation {_currentGeneration}");
                    para.StyleName = genDivider3Plus?.CharacterStyleName;
                    break;
            }
        }

        public string GetStatsSummary()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Summary of report processing");
            sb.AppendLine($"\tMain/continued persons.....{MyReportStats.MainPerson,8:N0}");
            sb.AppendLine($"\tNon-continued children.....{MyReportStats.NonContinuedPerson,8:N0}");
            sb.AppendLine($"\tMain spouses...............{MyReportStats.MainSpouse,8:N0}");
            sb.AppendLine($"\tSpouses of non-continued...{MyReportStats.NonContinuedSpouses,8:N0}");
            sb.AppendLine($"\tParents of spouses.........{MyReportStats.SpouseParents,8:N0}");
            sb.AppendLine($"\tPersons possibly living....{MyReportStats.MaybeLiving,8:N0}");
            sb.AppendLine($"\tCitations..................{MyReportStats.Citations,8:N0}");
            sb.AppendLine($"\tDistinct citations.........{MyReportStats.DistinctCitations,8:N0}");
            //sb.AppendLine($"\tPrep time...................{MyReportStats.PrepTime:h\\:mm\\:ss\\.fff}");
            //sb.AppendLine($"\tRun time....................{MyReportStats.ReportTime:h\\:mm\\:ss\\.fff}");
            sb.AppendLine($"\tTotal execution time........{MyReportStats.ReportTime.Add(MyReportStats.PrepTime):h\\:mm\\:ss\\.fff}");
            sb.Append($"\tDate/time completed.........{MyReportStats.EndTime}");
            return sb.ToString();
        }

        private WpdIndexField ConditionallyEmitIndexField(IWpdDocument doc, IndexSettings ixs)
        {
            if (!ixs.Enabled) return null;
            doc.BreakForIndex();
            IWpdParagraph p = doc.InsertParagraph(); //ixs.IndexHeading);
            WpdIndexField ixf = doc.BuildIndexField();
            ixf.IndexName = ixs.IndexName;
            ixf.Columns = ixs.Columns;
            ixf.EntryPageSeparator = ixs.Separator;
            ixf.Heading = ixs.IndexHeading;
            p.AppendField(ixf.Build());
            return ixf;
        }

        private void EmitMainPerson(IWpdDocument doc, ReportEntry re, int gen)
        {
            if (_omitFocusSpouses && re.OutOfFocus) 
                return;

            bool timeToMinimize = _ancestryReport && _minFromGen > 0 && _minFromGen <= gen;

            Formatting generationNumberFormatting = new Formatting() { CharacterStyleName = _styleMap[StyleSlots.GenerationNumber].CharacterStyleName }; //  switched this to the style
            int genNbrIncr = _ancestryReport ? -1 : 1;
            Formatting introFormatting = new Formatting(){Bold = _c.Settings.IntroBold, Italic = _c.Settings.IntroItalic};
            Formatting lineageListNameFormatting = new Formatting() { Italic = _c.Settings.ItalicsNamesInLineageList, CharacterStyleName = _styleMap[StyleSlots.MainPersonText].CharacterStyleName };

            // begin main person content
            IWpdParagraph p = doc.InsertParagraph();
            p.StyleName = _styleMap[StyleSlots.MainPersonText].CharacterStyleName;
            p.Append($"{re.GetNumber(_includeGenerationNumbers)}. ");
            p.Append(re.Individual.SafeGivenName, false, _styleMap[StyleSlots.MainPerson]);
            if (!_suppressGenNumbers)
                p.Append($"{gen}", false, generationNumberFormatting);
            if (!string.IsNullOrEmpty(re.Individual.SafeSurname))
                p.Append($" {re.Individual.SafeSurname}", false, _styleMap[StyleSlots.MainPerson]);
            ConditionallyEmitNameIndexEntry(doc, p, re.Individual);
            if (_c.Settings.DebuggingOutput)
            {
                p.Append($" [{re.NaturalId}]");
            }

            MyReportStats.MainPerson++;
            if (!re.Individual.PresumedDeceased)
                MyReportStats.MaybeLiving++;
            
            if (!_ancestryReport)
                re.Ancestry?.Emit(p, lineageListNameFormatting, _styleMap[StyleSlots.GenerationNumber]);

            List<GedcomIndividual> noteworthy = AppendPersonDetails(p, re); //todo: ??

            string tinue = re.GetContinuation(_includeGenerationNumbers);
            if (tinue != null)
                p.Append(" ").Append(tinue);

            if (timeToMinimize)
            {
                if (_omitBackRefs)
                    return;
                foreach (ReportFamilyEntry family in re.FindMainNumberedFamilies())
                {
                    List<ReportEntry> numbered = family.FindMainNumberedChildren();
                    EmitFamilyIntroLine(doc, numbered.Count, family);
                    foreach (ReportEntry child in numbered)
                    {
                        EmitChildEntry(doc, gen + genNbrIncr, child, generationNumberFormatting);
                    }
                }
                return;
            }

            bool lastLineWasDivider = false;
            bool dividersApplied = false;
            foreach (GedcomIndividual indiNotes in noteworthy)
            {
                if (indiNotes == null) continue;
                string s = indiNotes.PersonNotes;
                if (string.IsNullOrEmpty(s)) continue;
                p = doc.InsertParagraph();
                p = doc.InsertParagraph();
                p.StyleName = _styleMap[StyleSlots.BodyTextIndent].CharacterStyleName; // trick/quirk: line may change the style, this must be done first
                if (!lastLineWasDivider && _c.Settings.NotesDividers)
                {
                    p.InsertHorizontalLine(lineType:"single", position:"top");
                    dividersApplied = true;
                }
                string intro = string.Format(_c.Settings.NoteIntro, indiNotes.NameForward);
                p.Append(intro, false, introFormatting);
                string[] paras = s.Split('\n');
                foreach (string para in paras)
                {
                    // dividers seem to bloat the output with empty space
                    // the best look seems to be, position at bottom and swallow the divider's newline
                    if (_c.Settings.ConvertDividers && (lastLineWasDivider = AppliesAsDivider(p, para)))
                        continue;
                    lastLineWasDivider = false;
                    p = doc.InsertParagraph();
                    p.StyleName = _styleMap[StyleSlots.BodyTextNotes].CharacterStyleName;
                    p.Append(para);
                }
            }

            if (dividersApplied && !lastLineWasDivider)
            {
                p.InsertHorizontalLine(lineType: "single");
                lastLineWasDivider = true;
            }

            if (_ancestryReport && !re.EmitChildrenAfter)
                return;

            // list the children
            foreach (ReportFamilyEntry family in re.FamilyEntries)  // todo:FamiliesToReport not populated?
            {
                if ((family.Children?.Count??0) == 0)
                    continue;

                if (!_allFamilies && !family.IsIncluded)
                    continue;
                int numberToList = family.Children.Count;
                EmitFamilyIntroLine(doc, numberToList, family);
                foreach (ReportEntry child in family.Children)
                {
                    if (child.ChildEntryEmitted) 
                        continue;
                    child.ChildEntryEmitted = true;
                    EmitChildEntry(doc, gen + genNbrIncr, child, generationNumberFormatting);
                }
            }
        }

        private void EmitFamilyIntroLine(IWpdDocument doc, int numberToList, ReportFamilyEntry family)
        {
            if (numberToList < 1) return; // guard
            IWpdParagraph p = doc.InsertParagraph(); // NB insert empty "" para with a styleid DOES NOT WORK
            p.StyleName = _styleMap[StyleSlots.KidsIntro].CharacterStyleName;
            p.Append(((numberToList > 1) ? "Children" : "Child"));
            p.Append($" of {(family.Husband?.Individual?.SafeNameForward) ?? "unknown"}");
            p.Append($" and {(family.Wife?.Individual?.SafeNameForward) ?? "unknown"}:");
        }

        private void EmitChildEntry(IWpdDocument doc, int g, ReportEntry child, Formatting generationNumberFormatting)
        {
            IWpdParagraph p;
            p = doc.InsertParagraph();
            p.StyleName = _styleMap[_ancestryReport ? StyleSlots.KidsAlt : StyleSlots.Kids].CharacterStyleName;
            char sp = child.AssignedMainNumber > 9999999 ? ' ' : '\t';
            string kidNbr = child.AssignedMainNumber > 0
                ? $"{child.GetNumber(_includeGenerationNumbers)}.\t{child.ChildNumberRoman}.{sp}"
                : $"\t{child.ChildNumberRoman}.\t";
            p.Append(kidNbr);
            string childNameStyle = _styleMap[StyleSlots.ChildName].CharacterStyleName;
            if (child.AssignedChildNumber > 1)
            {
                // just the name please: standard applies
                p.Append(child.Individual.SafeNameForward, false, _childNameFormatting);
            }
            else
            {
                // include the generation number
                p.Append(child.Individual.SafeGivenName, false, _childNameFormatting);
                bool dropNbr = _suppressGenNumbers || g < 1 || (_ancestryReport && child.AssignedMainNumber < 1);
                if (!dropNbr)
                    p.Append($"{g}", false, generationNumberFormatting);
                if (!string.IsNullOrEmpty(child.Individual.SafeSurname))
                    p.Append($" {child.Individual.SafeSurname}", false, _childNameFormatting);
            }
            if (child.AssignedMainNumber == 0)
            {
                MyReportStats.NonContinuedPerson++;
                if (!child.Individual.PresumedDeceased)
                    MyReportStats.MaybeLiving++;
            }

            ConditionallyEmitNameIndexEntry(doc, p, child.Individual);
            if (_c.Settings.DebuggingOutput)
            {
                p.Append($" [{child.NaturalId}]");
            }

            AppendPersonDetails(p, child, true);
        }

        private bool AppliesAsDivider(IWpdParagraph p, string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            if (_rexDivider1.IsMatch(s))
            {
                p.InsertHorizontalLine(lineType: "single");
                return true;
            }

            if (!_rexDivider2.IsMatch(s))
                return false;

            p.InsertHorizontalLine(lineType: "double");
            return true;
        }

        internal bool ConditionallyEmitNameIndexEntry(IWpdDocument doc, IWpdParagraph p, GedcomIndividual indi)
        {
            if (!_c.Settings.NameIndexSettings.Enabled || indi == null) return false;
            string ixn = _c.Settings.NameIndexSettings.IndexName;
            if (string.IsNullOrEmpty(ixn))
                ixn = null;
            var ex = doc.BuildIndexEntryField(ixn, $"{indi.IndexableSurname}:{indi.SafeGivenName}").Build();
            p.AppendField(ex);
            return true;
        }
        internal bool ConditionallyEmitPlaceIndexEntry(IWpdDocument doc, IWpdParagraph p, FormattedEvent fe)
        {
            if (!_c.Settings.PlaceIndexSettings.Enabled || string.IsNullOrEmpty(fe?.PlaceIndexEntry)) return false;
            string ixn = _c.Settings.PlaceIndexSettings.IndexName;
            if (string.IsNullOrEmpty(ixn))
                ixn = null;
            var ex = doc.BuildIndexEntryField(ixn, fe.PlaceIndexEntry).Build();
            p.AppendField(ex); // todo: consider INSERTING it if there is more text after the place
            return true;
        }

        private List<GedcomIndividual> AppendPersonDetails(IWpdParagraph p, ReportEntry re, bool isChild = false)
        {
            List<GedcomIndividual> toDoNotes = new List<GedcomIndividual>();

            // short-stop: optional standard / brief child line if so configured and the child is continued
            if (isChild && _standardBriefContdChild && re.AssignedMainNumber > 0)
            {
                string conn = ", m.";
                FormattedEvent p0_bbp = ConditionalEvent(", b.", re.Individual.Born, re.Individual.PlaceBorn);
                if (string.IsNullOrEmpty(p0_bbp?.EventString))
                {
                    p0_bbp = ConditionalEvent(", bp.", re.Individual.Baptized, re.Individual.PlaceBaptized);
                }
                if (!string.IsNullOrEmpty(p0_bbp?.EventString))
                {
                    p.Append(p0_bbp.EventString.TrimStart().Replace(" on ", " "));  // ugly little tweaks; this needs to all be smarter
                    ConditionallyEmitPlaceIndexEntry(_c.Model.Doc, p, p0_bbp);
                    conn = "; m.";
                }

                string mnbr = re.SafeFamilies.Count > 1 ? $" {_wordsForNumbers[1]}" : "";
                for (int i = 0; i < re.SafeFamilies.Count; i++)
                {
                    GedcomIndividual spouze = re.SafeFamilies[i].Family.SpouseOf(re.Individual);
                    string s = $"{conn}{mnbr} {spouze?.NameForward}";
                    p.Append(s);
                    ConditionallyEmitNameIndexEntry(_c.Model.Doc, p, spouze);
                    conn = ", ";
                    mnbr = $" {_wordsForNumbers[i + 2]}";
                }

                p.Append(".");
                return toDoNotes;
            }


            // optional minimized or reduced output for child listing if there are descendants, except for the last reported generation
            bool minimized = isChild && _c.Settings.MinimizeContinuedChildren && re.HasDescendants && _currentGeneration < _c.Settings.Generations;
            if (minimized)
            {
                p.Append(re.Individual.ReportableSpan);
                p.Append(".");
                return toDoNotes;
            }

            // we can decide if we are doing citations
            bool reduced = isChild && _reduceChild && re.HasDescendants && _currentGeneration < _c.Settings.Generations;
            bool doNotCite = isChild && _c.Settings.OmitCitesOnContinued && re.HasDescendants && _currentGeneration < _c.Settings.Generations;
            
            // override these decisions in the case of ancestry report; a bit of "jiggle the handle" approach, eh?
            reduced &= !_ancestryReport || re.AssignedMainNumber > 0;
            doNotCite &= !_ancestryReport || re.AssignedMainNumber > 0;

            bool doCite = _c.Settings.Citations && !doNotCite
                                                && (!_c.Settings.ObscureLiving || !_c.Settings.OmitLivingCitations || re.Individual.PresumedDeceased);

            // and we can detect and optimize out consecutive repeats of the same citation
            // NB this list is ORDERED by the appearance of the cited facts
            CitationProposals cp = new CitationProposals();
            if (doCite)
            {
                cp.AddNonNull(re.Individual.CitableEvents?.Find(re.Individual.EventTag(TagCode.BIRT)));
                cp.AddNonNull(re.Individual.CitableEvents?.Find(re.Individual.EventTag(TagCode.BAPM)));
                cp.AddNonNull(re.Individual.CitableEvents?.Find(re.Individual.EventTag(TagCode.DEAT)));
                cp.AddNonNull(re.Individual.CitableEvents?.Find(re.Individual.EventTag(TagCode.BURI)));
                for (int mnbr = 0; mnbr < re.SafeFamilies.Count; mnbr++)
                {
                    ReportFamilyEntry family = re.SafeFamilies[mnbr];
                    EventCitations ec = family.Family.CitableEvents?.Find(family.Family.EventTag(TagCode.MARR));
                    cp.AddNonNull(ec, family.Family.FamilyView.Id);
                }
            }
            CitationProposals chosenCitations = CitationCoordinator.Optimize(cp);

            string comma = null;
            if (!isChild && _ancestryReport && re.HasParents)
            {
                p.Append(", ").Append(re.Individual.NounAsChild.ToLower()).Append(" of ");
                string conjunction = " ";
                if (re.ChildhoodFamily.Husband != null)
                {
                    p.Append(re.ChildhoodFamily.Husband.Individual.NameForward);
                    conjunction = " and ";
                }

                if (re.ChildhoodFamily.Wife != null)
                {
                    p.Append(conjunction);
                    p.Append(re.ChildhoodFamily.Wife.Individual.NameForward);
                }

                //p.Append(",");
                comma = ",";
            }

            // to position footnote superscripts in the running text correctly (especially,
            // in relation to punctuation), we need to know IN ADVANCE all of the pieces that 
            // will be emitted.  So, figure all that out FIRST and then start outputting it
            FormattedEvent p1_birt = ConditionalEvent("was born", re.Individual.Born, 
                re.Individual.PlaceBorn, reduced ? null : re.Individual.BirthDescription);
            // list the baptism for non-reduced output OR if there is no birth and option set to substitute it
            FormattedEvent p2_bapt = (_listBapt && !reduced) || (_c.Settings.BaptIfNoBirt && p1_birt == null)
                ? ConditionalEvent("baptized", re.Individual.Baptized, re.Individual.PlaceBaptized, 
                    reduced ? null : re.Individual.BaptizedDescription)
                : null;
            FormattedEvent p3_deat = ConditionalEvent("died", re.Individual.Died, re.Individual.PlaceDied, 
                reduced ? null : re.Individual.DeathDescription);
            FormattedEvent p4_buri = (!reduced && _listBuri) 
                ? ConditionalEvent("was buried", re.Individual.Buried, re.Individual.PlaceBuried, re.Individual.BurialDescription, _c.Settings.OmitBurialDate)
                : null;

            if (comma != null && !(p1_birt is null && p2_bapt is null && p3_deat is null && p4_buri is null))
            {
                p.Append(comma);
                comma = null; // in case we need this again
            }
            // knowing all the pieces, we can add the conjunctions, [NOT commas], pronouns needed to the text
            if (p2_bapt != null)
            {
                if (p1_birt == null)
                {
                    if (null == p3_deat && null == p4_buri)
                        p2_bapt.EventString = $" and was{p2_bapt.EventString}";
                    else
                        p2_bapt.EventString = $" was{p2_bapt.EventString}";
                } 
                else if (null == p3_deat && null == p4_buri)
                    p2_bapt.EventString = $" and was{p2_bapt.EventString}";
            }

            if (p3_deat!=null && (p1_birt != null || p2_bapt !=null))
            {
                p3_deat.EventString = $" and {re.Individual.Pronoun.ToLower()}{p3_deat.EventString}"; // remove pendingPunctuation
            }

            if (p3_deat != null)
                p3_deat.EventString = $"{p3_deat.EventString}.";
            else if (p2_bapt != null)
                p2_bapt.EventString = $"{p2_bapt.EventString}.";
            else if (p1_birt != null)
                p1_birt.EventString = $"{p1_birt.EventString}.";
            else
                p.Append(".");


            if (p4_buri != null)
            {
                // style choice: always saying burial as a separate sentence
                p4_buri.EventString = $" {re.Individual.Pronoun}{p4_buri.EventString}.";
            }
            
            if (!string.IsNullOrEmpty(p1_birt?.EventString))
            {
                p.Append(p1_birt.EventString);
                if (!reduced && p2_bapt != null && p3_deat != null)
                    p.Append(",");
                if (doCite)
                {
                    EventCitations ec = chosenCitations[TagCode.BIRT.ToString()];
                    if (ec?.SelectedItem != null)
                    {
                        MyReportStats.Citations++;
                        if (!ec.SelectedItem.IsEmitted)
                            MyReportStats.DistinctCitations++;
                        ec.EmitNote(_c.Model.Doc, p);
                    }
                }
                ConditionallyEmitPlaceIndexEntry(_c.Model.Doc, p, p1_birt);
            }

            if (p2_bapt != null)
            {
                p.Append(p2_bapt.EventString);
                if (doCite)
                {
                    EventCitations ec = chosenCitations[TagCode.BAPM.ToString()];
                    if (ec?.SelectedItem != null)
                    {
                        MyReportStats.Citations++;
                        if (!ec.SelectedItem.IsEmitted)
                            MyReportStats.DistinctCitations++;
                        ec.EmitNote(_c.Model.Doc, p);
                    }
                }
                ConditionallyEmitPlaceIndexEntry(_c.Model.Doc, p, p2_bapt);
            }

            if (!string.IsNullOrEmpty(p3_deat?.EventString))
            {
                p.Append(p3_deat.EventString);
                if (doCite)
                {
                    EventCitations ec = chosenCitations[TagCode.DEAT.ToString()];
                    if (ec?.SelectedItem != null)
                    {
                        MyReportStats.Citations++;
                        if (!ec.SelectedItem.IsEmitted)
                            MyReportStats.DistinctCitations++;
                        ec.EmitNote(_c.Model.Doc, p);
                    }
                }
                ConditionallyEmitPlaceIndexEntry(_c.Model.Doc, p, p3_deat);
            }

            if (p4_buri != null)
            {
                p.Append(p4_buri.EventString);
                if (doCite)
                {
                    EventCitations ec = chosenCitations[TagCode.BURI.ToString()];
                    if (ec?.SelectedItem != null)
                    {
                        MyReportStats.Citations++;
                        if (!ec.SelectedItem.IsEmitted)
                            MyReportStats.DistinctCitations++;
                        ec.EmitNote(_c.Model.Doc, p);
                    }
                }
                ConditionallyEmitPlaceIndexEntry(_c.Model.Doc, p, p4_buri);
            }

            if (reduced) return toDoNotes;

            if (_c.Settings.MainPersonNotes)
                toDoNotes.Add(re.Individual);
            
            if (!re.SuppressSpouseInfo)
                AppendMarriagesSentences(p, re, isChild, toDoNotes, reduced, doCite, chosenCitations);

            return toDoNotes;
        }

        private void AppendMarriagesSentences(IWpdParagraph p, ReportEntry re, bool isChild, List<GedcomIndividual> toDoNotes,
            bool reduced, bool doCite, CitationProposals chosenCitations)
        {
            bool storyAppended = false;
            for (int mnbr = 0; mnbr < re.SafeFamilies.Count; mnbr++)
            {
                string mid = (re.SafeFamilies.Count > 1)
                    ? (mnbr + 1 < _wordsForNumbers.Length) ? $" {_wordsForNumbers[mnbr + 1]}" : (mnbr + 1).ToString()
                    : null;
                if (mid != null && !re.FamiliesAreSorted)
                    mid += "[?]";
                ReportFamilyEntry family = re.SafeFamilies[mnbr];
                ReportEntry spouse = (family.Husband?.Individual == re.Individual) ? family.Wife : family.Husband;
                p.Append($" {(storyAppended ? re.Individual.SafeNameForward : re.Individual.Pronoun)} married{mid} ");
                if (_c.Settings.DebuggingOutput)
                {
                    p.Append($"[{family.Family.FamilyView.Id}] ");
                }

                string pending = "";
                if (spouse != null)
                {
                    if (_c.Settings.SpousesNotes && !_ancestryReport)  // prevent duplication when both are handled as mains
                        toDoNotes.Add(spouse.Individual);


                    p.Append(spouse.Individual.SafeNameForward);
                    if (isChild && re.AssignedMainNumber == 0)
                    {
                        MyReportStats.NonContinuedSpouses++;
                        if (!re.Individual.PresumedDeceased)
                            MyReportStats.MaybeLiving++;
                    }
                    else if (!isChild)
                    {
                        MyReportStats.MainSpouse++;
                        if (!re.Individual.PresumedDeceased)
                            MyReportStats.MaybeLiving++;
                    }

                    ConditionallyEmitNameIndexEntry(_c.Model.Doc, p, spouse.Individual);
                    if (_c.Settings.DebuggingOutput)
                    {
                        p.Append($" [{spouse.NaturalId}]");
                    }
                    // if appropriate, here say ", as his second wife, ", or similar
                    if ((spouse.Families?.Count ?? 0) > 1)
                    {
                        string asNth = GenerateNthSpousePhrase(re, spouse);
                        if (!string.IsNullOrEmpty(asNth))
                        {
                            p.Append($", {asNth}");
                            pending = ",";
                        }
                    }
                }
                else
                {
                    p.Append(" (unknown)");
                }

                FormattedEvent p5_marr = ConditionalEvent("", family.Family.DateMarried, family.Family.PlaceMarried,
                    reduced ? null : family.Family.MarriageDescription);
                if (!string.IsNullOrEmpty(p5_marr?.EventString))
                {
                    p.Append($"{pending}{p5_marr.EventString}");
                    ConditionallyEmitPlaceIndexEntry(_c.Model.Doc, p, p5_marr);
                }

                // todo: divorces (complicates the punctuation and citation placement)
                p.Append(".");
                if (doCite)
                {
                    EventCitations ec = chosenCitations[TagCode.MARR.ToString() + family.Family.FamilyView.Id];
                    if (ec?.SelectedItem != null)
                    {
                        MyReportStats.Citations++;
                        if (!ec.SelectedItem.IsEmitted)
                            MyReportStats.DistinctCitations++;
                        ec.EmitNote(_c.Model.Doc, p);
                    }
                }

                if (spouse?.Id == re.SpouseToMinimize?.Id) 
                    continue;

                storyAppended = !re.SuppressSpouseInfo && AppendSpouseSentence2(p, spouse);
                //...
            }
        }

        private string GenerateNthSpousePhrase(ReportEntry re, ReportEntry spouse)
        {
            int i = spouse.MarriageNumberWith(re);
            if (i < 1)
                return null;
            return $"as {spouse.Individual.PronounPossessive.ToLower()} {_wordsForNumbers[i]} {re.Individual.NounAsSpouse.ToLower()}";
        }

        private bool AppendSpouseSentence2(IWpdParagraph p, ReportEntry spouse)
        {
            // emit details of spouse:
            //      if we know them (descendants report)
            //      iff the person is not assigned a main number (ancestry report)
            if (spouse == null || (_ancestryReport && spouse.AssignedMainNumber > 0))
                return false;

            GedcomFamily spousesChildhoodFamily = ReportContext.Instance.Model.FindAsChildInFamily(spouse.Individual);
            if (spousesChildhoodFamily == null) return false;

            // here we will list the baptism iff there is no birth 
            FormattedEvent p1_birt = ConditionalEvent("born", spouse.Individual.Born, spouse.Individual.PlaceBorn, spouse.Individual.BirthDescription);
            FormattedEvent p2_bapt = p1_birt == null
                ? ConditionalEvent("baptized", spouse.Individual.Baptized, spouse.Individual.PlaceBaptized, spouse.Individual.BaptizedDescription)
                : null;

            // here we will list the burial iff there is no death
            FormattedEvent p3_deat = ConditionalEvent("died", spouse.Individual.Died, spouse.Individual.PlaceDied, spouse.Individual.DeathDescription);
            FormattedEvent p4_buri = p3_deat == null
                ? ConditionalEvent("buried", spouse.Individual.Buried, spouse.Individual.PlaceBuried, spouse.Individual.BurialDescription, _c.Settings.OmitBurialDate)
                : null;

            // to reduce the number of cites and get the minimal case... b&d same source... down to one
            // note that birt/bapm and deat/buri are natural synonyms, and 
            // we are listing only one from each pair here, so, we have at most two citations
            EventCitations ec_bb = spouse.Individual.CitableEvents?.Find(spouse.Individual.EventTag(p1_birt == null ? TagCode.BAPM : TagCode.BIRT));
            EventCitations ec_di = spouse.Individual.CitableEvents?.Find(spouse.Individual.EventTag(p3_deat == null ? TagCode.BURI : TagCode.DEAT));

            // and if they are the same, drop the earlier one
            if (ec_di?.SelectedItem == ec_bb?.SelectedItem)
                ec_bb = null;


            bool hasBb = p1_birt != null || p2_bapt != null;
            bool hasDb = p3_deat != null || p4_buri != null;
            bool hasContent = hasBb || hasDb;
            bool hasPere = !string.IsNullOrEmpty(spousesChildhoodFamily.Husband?.SafeNameForward);
            bool hasMere = !string.IsNullOrEmpty(spousesChildhoodFamily.Wife?.SafeNameForward);
            bool hasRents = hasMere || hasPere;

            hasContent |= hasRents;

            if (!hasContent) return false;

            // {He|She} was [the {son|daughter} of [father] [and] [mother], [born ...][, and {he|she} died ...].

            p.Append($" {spouse.Individual.Pronoun}");
            bool isOpen = false;
            bool doCite = _c.Settings.Citations && (!_c.Settings.ObscureLiving || !_c.Settings.OmitLivingCitations || spouse.Individual.PresumedDeceased);
            string connector = null;
            string closer = ".";

            if (hasRents)
            {
                p.Append($" was the {spouse.Individual.NounAsChild.ToLower()} of");
                string conj = null;
                if (hasPere)
                {
                    p.Append($" {spousesChildhoodFamily.Husband.SafeNameForward}");
                    MyReportStats.SpouseParents++;
                    if (!spousesChildhoodFamily.Husband.PresumedDeceased)
                        MyReportStats.MaybeLiving++;
                    ConditionallyEmitNameIndexEntry(_c.Model.Doc, p, spousesChildhoodFamily.Husband);
                    if (_c.Settings.DebuggingOutput)
                    {
                        p.Append($" [{spousesChildhoodFamily.Husband.IndividualView.Id}]");
                    }
                    conj = "and ";
                }
                if (hasMere)
                {
                    p.Append($" {conj}{spousesChildhoodFamily.Wife.SafeNameForward}");
                    MyReportStats.SpouseParents++;
                    if (!spousesChildhoodFamily.Wife.PresumedDeceased)
                        MyReportStats.MaybeLiving++;
                    ConditionallyEmitNameIndexEntry(_c.Model.Doc, p, spousesChildhoodFamily.Wife);
                    if (_c.Settings.DebuggingOutput)
                    {
                        p.Append($" [{spousesChildhoodFamily.Wife.IndividualView.Id}]");
                    }
                }

                isOpen = true;
            }

            if (hasBb)
            {
                if (isOpen)
                {
                    if (!hasDb)
                        connector = $" and {spouse.Individual.Pronoun.ToLower()} was";
                    else
                        connector = $",";
                }
                else
                {
                    connector = " was";
                }

                p.Append($"{connector}{p1_birt?.EventString??p2_bapt?.EventString}{(hasDb?",":null)}");
                ConditionallyEmitPlaceIndexEntry(_c.Model.Doc, p, p1_birt??p2_bapt);
                if (hasDb)
                {
                    connector = $" and {spouse.Individual.Pronoun.ToLower()}";
                    isOpen = true;
                }
                else
                {
                    p.Append(closer);
                    closer = null;
                }
                if (doCite)
                {
                    if (ec_bb?.SelectedItem != null)
                    {
                        MyReportStats.Citations++;
                        if (!ec_bb.SelectedItem.IsEmitted)
                            MyReportStats.DistinctCitations++;
                        ec_bb.EmitNote(_c.Model.Doc, p);
                    }
                }
            }

            if (!hasBb && isOpen)
                connector = $" and {spouse.Individual.Pronoun.ToLower()}";

            if (hasDb)
            {
                p.Append($"{connector}{p3_deat?.EventString ?? p4_buri?.EventString}");
                ConditionallyEmitPlaceIndexEntry(_c.Model.Doc, p, p3_deat??p4_buri);
                p.Append(closer);
                closer = null;
                if (doCite)
                {
                    if (ec_di?.SelectedItem != null)
                    {
                        MyReportStats.Citations++;
                        if (!ec_di.SelectedItem.IsEmitted)
                            MyReportStats.DistinctCitations++;
                        ec_di.EmitNote(_c.Model.Doc, p);
                    }
                }
            }
            if (closer!=null)
                p.Append(closer);
            return true;
        }

        private FormattedEvent ConditionalEvent(string ev, string dayt, string place, string detail = null, bool omitDate = false)
        {
            if (string.IsNullOrEmpty(dayt) && string.IsNullOrEmpty(place))
                return null;
            FormattedEvent fe = new FormattedEvent();
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(ev))
                sb.Append(" ").Append(ev);

            if (!omitDate && !string.IsNullOrEmpty(dayt))
                sb.Append(' ').Append(_dateFormatter.Reformat(dayt));
            if (!string.IsNullOrEmpty(place))
            {
                FormattedPlaceName fpn = _placeFormatter.Reformat(place);
                sb.Append(" in ").Append(fpn.PreferredName);
                fe.PlaceIndexIndex = sb.Length;
                fe.PlaceIndexEntry = fpn.IndexEntry;
            }

            if (_factDesc && !string.IsNullOrEmpty(detail))
            {
                string det = OptimizeEventDetail(detail);
                if (!string.IsNullOrEmpty(det))
                    sb.Append(" ").Append(det);
            }

            string rv = sb.ToString().Trim();
            if (rv.Equals(ev?.Trim() ?? "")) return null;
            fe.EventString = sb.ToString();

            return fe;
        }

        internal class FormattedEvent
        {
            public string EventString { get; set; }
            public string PlaceIndexEntry { get; set; }
            public int PlaceIndexIndex { get; set; }
        }

        private string OptimizeEventDetail(string detail)
        {
            detail = detail?.Trim();
            if (string.IsNullOrEmpty(detail) || detail.Length < 2)
                return null;
            
            // don't end with a period
            if (detail.EndsWith("."))
                detail = detail.Substring(0, detail.Length - 1);

            string[] ss = detail.Split(_splitSpace, StringSplitOptions.RemoveEmptyEntries);

            // don't start with a capital letter
            // unless it is apparently part of a name (next word also caps)
            // or an abbreviated name (one letter)
            if (EvalForInitialLowercase(ss[0], ss.Length > 1 ? ss[1] : null))
                ss[0] = ss[0].Substring(0, 1).ToLower() + ss[0].Substring(1);


            // don't end sentences in the body of this text, make them ; clauses
            // single letter with a . do not change.  The word after any . -> ; change
            // is also a candidate to lowercase
            for (int i = 1; i < ss.Length; i++)
            {
                if (!ss[i].EndsWith(".")) continue;
                if (ss[i].Length < 3) continue;
                // remove the '.'; todo: what about abbreviations like "St."
                ss[i] = ss[i].Substring(0, ss[i].Length - 1) + ";";
                if (i+2 >= ss.Length) continue; // need two more words to evaluate for lc
                if (!EvalForInitialLowercase(ss[i+1], ss[i+2])) continue;
                ss[i+1] = ss[i+1].Substring(0, 1).ToLower() + ss[i+1].Substring(1);
            }

            // reassemble, wrapped in ()s
            StringBuilder sb = new StringBuilder(detail.Length + 2);
            sb.Append('(');
            for (int i = 0; i < ss.Length-1; i++)
            {
                sb.Append(ss[i]).Append(' ');
            }

            sb.Append(ss[ss.Length - 1]).Append(')');
            return sb.ToString();
        }

        public bool EvalForInitialLowercase(string x1, string x2)
        {
            if (NameConstants.CommonGivenNames.Contains(x1.ToUpper())) return false;
            if (NameConstants.CommonSurnames.Contains(x1.ToUpper())) return false;

            if (LanguageElementConstants.CommonPrepositions.Contains(x1.ToLower())) return true;
            if (LanguageElementConstants.Determiners.Contains(x1.ToLower())) return true;
            if (LanguageElementConstants.Pronouns.Contains(x1.ToLower())) return true;

            bool initialLowercase = x1.Length > 1 && char.IsUpper(x1.ToCharArray()[0]);
            initialLowercase = initialLowercase && (x1.Length > 2 || (x1.Length == 2 && x1.ToCharArray()[1] != '.'));
            initialLowercase = initialLowercase && (x2 == null || !char.IsUpper(x2.ToCharArray()[0]));
            return initialLowercase;
        }
    }
}
