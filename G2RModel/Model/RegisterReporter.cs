using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
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
        public RegisterReportModel Model { get; set; }

        private ReportTreeBuilder _tree;
        private ReportContext _c;
        private CitationsMap _cm;
        private GenealogicalDateFormatter _dateFormatter;
        private GenealogicalPlaceFormatter _placeFormatter;
        private GenerationNumberMapper _gnMapper;

        // former fields, retargeted to the new tree builder
        public ListOfReportEntry[] Generations => _tree?.Generations;
        private int _lastSlotWithPeople => _tree?.LastSlotWithPeople ?? 0;
        private List<GedcomIndividual> _nonContinued => _tree?.NonContinued;

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

        #region Fields to internalize settings/policies
        private Dictionary<StyleSlots, Formatting> _styleMap;
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
        private bool _genNbrAllChildren;

        private bool _ancestryReport;
        private bool _registerReport => !_ancestryReport;
        private bool _suppressGenSuperscripts;
        private bool _allowMultiple;
        private bool _generationNumberPrefixes;
        private bool _placeholders;
        private string _unknownName;

        private bool _includeBackRefs;
        private bool _includeSibsOfBackrefs;

        private bool _allFamilies;
        private bool _omitFocusSpouses;
        private int _minFromGen;
        private bool _omitBackRefsLater;
        private int _maxLivingGenerations;

        private bool _indexMarriedNames;
        private bool _placeFirst;
        private bool _insertUncitedNotes;
        private bool _spaceCouples;

        private Formatting _childNameFormatting;
        private Formatting _generationNumberFormatting;
        private Formatting _introFormatting;
        private Formatting _lineageListNameFormatting;
        #endregion

        #region Initialization
        public RegisterReporter Init(GedcomIndividual root, TimeSpan prep)
        {
            MyReportStats = new ReportStats().Init(prep);
            _c = ReportContext.Instance;

            // configure policies that are held in statics
            CitationCoordinator.DeferConsecutiveRepeats = _c.Settings.DeferConsecutiveRepeats;
            FormattedEvent.IncludeFactDescription = _factDesc = _c.Settings.IncludeFactDescriptions;
            FormattedEvent.PlaceBeforeDate = _placeFirst = _c.Settings.PlaceFirst;

            _reduceChild = _c.Settings.ReduceContinuedChildren;
            _listBapt = _c.Settings.IncludeBaptism;
            _listBuri = _c.Settings.IncludeBurial;

            _ancestryReport = _c.Settings.AncestorsReport;
            _suppressGenSuperscripts = _c.Settings.SuppressGenNbrs && _ancestryReport;
            _generationNumberPrefixes = _c.Settings.GenerationPrefix && _ancestryReport;
            _genNbrAllChildren = _c.Settings.GenNbrAllChildren;
            _allowMultiple = _c.Settings.AllowMultipleAppearances;
            _placeholders = _ancestryReport && _c.Settings.Placeholders;
            _unknownName = _c.Settings.UnknownInReport;

            _includeBackRefs = _ancestryReport && _c.Settings.IncludeBackRefs;
            _includeSibsOfBackrefs = _includeBackRefs && _c.Settings.IncludeSiblings;

            _allFamilies = !_c.Settings.AncestorsReport; // || _c.Settings.AllFamilies;
            _omitFocusSpouses = _c.Settings.OmitFocusSpouses;
            _minFromGen = _c.Settings.MinimizeFromGeneration;
            _omitBackRefsLater = _c.Settings.OmitBackRefsLater && _ancestryReport;
            _maxLivingGenerations = _c.Settings.AssumedMaxLivingGenerations;

            _indexMarriedNames = _c.Settings.IndexMarriedNames;

            _insertUncitedNotes = _c.Settings.CitationPlaceholders;

            _generationalReducePlaceNames = _c.Settings.FullPlaceOncePerGen;
            _standardReducePlaceNames = _c.Settings.ReducePlaceNames && !_c.Settings.FullPlaceOncePerGen;
            _standardBriefContdChild = _c.Settings.StandardBriefContinued;

            _spaceCouples = _c.Settings.SpaceBetweenCouples;

            ReportEntryFactory.Init(!_allowMultiple);

            _cm = new CitationsMap(Model.GedcomFile);
            CitationCoordinator = new CitationCoordinator(Model.GedcomFile.CitationViews)
            {
                IncludeBurial = _listBuri,
                Priorities = _c.Settings.SourcePriorities,
                AnitPriorities = _c.Settings.SourceAnitPriorities,
                BaptismOption = _c.Settings.BaptismOption,
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

            _gnMapper = GenerationNumberMapper.Instance
                .Init(Generations.Length, _ancestryReport ? null : _c.Settings.FirstGenNbr);

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
        #endregion

        #region Execution
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
            _generationNumberFormatting = new Formatting() { CharacterStyleName = _styleMap[StyleSlots.GenerationNumber].CharacterStyleName };
            _introFormatting = new Formatting() { Bold = _c.Settings.IntroBold, Italic = _c.Settings.IntroItalic };
            _lineageListNameFormatting = new Formatting() { Italic = _c.Settings.ItalicsNamesInLineageList, CharacterStyleName = _styleMap[StyleSlots.MainPersonText].CharacterStyleName };

            _dateFormatter = GenealogicalDateFormatter.Instance;

            // as a side effect, we make the instance with option settings known here
            // available to others via the optional-singleton Instance
            GenealogicalPlaceFormatter.Instance = _placeFormatter = new GenealogicalPlaceFormatter()
            {
                DropUSA = _c.Settings.DropUsa, 
                InjectWordCounty = _c.Settings.InjectCounty,
                ReduceOnRepetition = _generationalReducePlaceNames || _standardReducePlaceNames,
            }.Init();

            _currentGeneration = 0;
            BigInteger genSize = new BigInteger(1);
            BigInteger expectedNext = 1;
            foreach (ListOfReportEntry generation in Generations)
            {
                if (Model.CheckCancel()) throw new CanceledByUserException();

                _currentGeneration++;
                Debug.WriteLine($"Generation {_currentGeneration} begins {DateTime.Now:G}");
                if (_generationalReducePlaceNames)
                    _placeFormatter.Reset();
                
                if ((generation?.Count??0)==0)
                    break;

                string describe;
                if (_ancestryReport)
                {
                    //double genPct = (double) (generation.Count / genSize);
                    describe =  _currentGeneration < 31 
                        ? $"{generation.Count} of {genSize} known ({generation.Count / (double)genSize:P1})"
                        : $"{generation.Count} of {genSize} known";
                }
                else
                {
                    describe = $"{generation.Count} known";
                }

                Model.PostProgress($"processing generation {_currentGeneration} ({describe})");

                if (_c.Settings.GenerationHeadings)
                    EmitDivider(doc, genDivider, genDivider3Plus);

                for (int i = 0; i < generation.Count; i++)
                {
                    ReportEntry individual = generation[i];

                    if (_standardReducePlaceNames)
                        _placeFormatter.Reset();

                    if (!_ancestryReport)
                    {
                        EmitMainPerson(doc, individual, _currentGeneration);
                        continue;
                    }

                    // Below this point: Ancestors Table format ONLY

                    if (_placeholders && (_minFromGen == 0 || _currentGeneration < _minFromGen))
                    {
                        if (individual.AssignedMainNumber != expectedNext)
                        {
                            if (_spaceCouples && expectedNext % 2 == 0)
                                doc.InsertParagraph();
                            EmitAncestorsPlaceholder(doc, expectedNext, individual.AssignedMainNumber - 1);
                        }
                    }

                    if (_spaceCouples)
                    {
                        bool spaceNeeded = _currentGeneration > 1 && i == 0;
                        spaceNeeded |= individual.AssignedMainNumber % 2 == 0;
                        spaceNeeded |=
                            i > 0 && individual.AssignedMainNumber - 1 != generation[i - 1].AssignedMainNumber;
                        if (spaceNeeded)
                            doc.InsertParagraph();
                    }

                    if (_allowMultiple && individual.IsRepeat)
                        EmitRepeatedAncestor(doc, individual, _currentGeneration);
                    else
                        EmitMainPerson(doc, individual, _currentGeneration);
                    
                    expectedNext = individual.AssignedMainNumber + 1;
                }

                if (_ancestryReport)  
                    genSize *= 2; 
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

        public string GetStatsSummary()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Summary of report processing");
            sb.AppendLine($"\tMain/continued persons.......{MyReportStats.MainPerson,8:N0}");
            sb.AppendLine($"\tNon-continued children.......{MyReportStats.NonContinuedPerson,8:N0}");
            sb.AppendLine($"\tMain spouses.................{MyReportStats.MainSpouse,8:N0}");
            sb.AppendLine($"\tSpouses of non-continued.....{MyReportStats.NonContinuedSpouses,8:N0}");
            sb.AppendLine($"\tParents of spouses...........{MyReportStats.SpouseParents,8:N0}");
            sb.AppendLine($"\tPersons possibly living......{MyReportStats.MaybeLiving,8:N0}");
            sb.AppendLine($"\tCitations....................{MyReportStats.Citations,8:N0}");
            sb.AppendLine($"\tDistinct citations...........{MyReportStats.DistinctCitations,8:N0}");
            sb.AppendLine($"\tDeferred/combined citations..{CitationCoordinator.Deferred,8:N0}");
            sb.AppendLine($"\tFacts with no citations......{MyReportStats.UncitedEvents,8:N0}");
            //sb.AppendLine($"\tPrep time...................{MyReportStats.PrepTime:h\\:mm\\:ss\\.fff}");
            //sb.AppendLine($"\tRun time....................{MyReportStats.ReportTime:h\\:mm\\:ss\\.fff}");
            sb.AppendLine($"\tReport preparation time.......{MyReportStats.ReportTime.Add(MyReportStats.PrepTime):h\\:mm\\:ss\\.fff}");
            sb.Append($"\tDate/time completed...........{MyReportStats.EndTime}");
            return sb.ToString();
        }
        #endregion

        #region Specialized emitters - indexes, dividers, notes, ...
 
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
        /// <summary>
        /// Convenience / accommodation for points in the code that
        /// don't have the ReportEntry in hand... that is a poor
        /// condition, but, we can get around it for the purpose of
        /// emitting the index entries...
        /// </summary>
        /// <param name="p"></param>
        /// <param name="indi"></param>
        /// <returns></returns>
        internal bool EmitNameIndexEntries(IWpdParagraph p, GedcomIndividual indi)
        {
            ReportEntry re = ReportEntryFactory.Instance.GetReportEntry(indi);
            return re != null && EmitNameIndexEntries(p, re);
        }

        internal bool EmitNameIndexEntries(IWpdParagraph p, ReportEntry re)
        {
            GedcomIndividual indi = re?.Individual;
            if (!_c.Settings.NameIndexSettings.Enabled || indi == null) return false;
            IWpdDocument doc = p.Document;
            WpdFieldBase ex;
            string ixn = _c.Settings.NameIndexSettings.IndexName;

            ex = doc.BuildIndexEntryField(ixn, $"{indi.IndexableSurname}:{indi.SafeGivenName}").Build();
            p.AppendField(ex);

            if (!_indexMarriedNames || indi.Gender != "F" || indi.HasNoSurname)
                return true;

            // heh.  I guess we can call this "lazy".
            if (re.FamilyEntries == null && !re.DidInit)
                re.Init();

            if (re.FamilyEntries == null)
                return true;

            foreach (ReportFamilyEntry fre in re.SafeFamilyEntries)
            {
                string altName;
                if ((altName = fre?.IndexableExtendedWifeForeName) == null)
                    continue;
                ex = doc.BuildIndexEntryField(ixn, $"{fre.Husband.Individual.IndexableSurname}:{altName}").Build();
                p.AppendField(ex);
            }

            return true;
        }

        internal bool ConditionallyEmitPlaceIndexEntry(IWpdDocument doc, IWpdParagraph p, FormattedEvent fe)
        {
            if (!_c.Settings.PlaceIndexSettings.Enabled || string.IsNullOrEmpty(fe?.PlaceIndexEntry)) return false;
            string ixn = _c.Settings.PlaceIndexSettings.IndexName;
            if (string.IsNullOrEmpty(ixn))
                ixn = null;
            var ex = doc.BuildIndexEntryField(ixn, fe.PlaceIndexEntry).Build();
            p.AppendField(ex); 
            return true;
        }

        private void EmitNotes(IWpdDocument doc, IWpdParagraph p, List<GedcomIndividual> noteworthy)
        {
            bool lastLineWasDivider = false;
            bool dividersApplied = false;
            foreach (GedcomIndividual indiNotes in noteworthy)
            {
                if (indiNotes == null) continue;
                string s = indiNotes.PersonNotes;
                if (string.IsNullOrEmpty(s)) continue;
                p = doc.InsertParagraph();
                p = doc.InsertParagraph();
                p.StyleName =
                    _styleMap[StyleSlots.BodyTextIndent]
                        .CharacterStyleName; // trick/quirk: line may change the style, this must be done first
                if (!lastLineWasDivider && _c.Settings.NotesDividers)
                {
                    p.InsertHorizontalLine(lineType: "single", position: "top");
                    dividersApplied = true;
                }

                string intro = string.Format(_c.Settings.NoteIntro, indiNotes.SafeNameForward);
                p.Append(intro, false, _introFormatting);
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

        #endregion

        #region Specialized emitters - Ancestry report

        private void EmitAncestorsPlaceholder(IWpdDocument doc, BigInteger frum, BigInteger thru, int? g = null)
        {
            int gen = g ?? _currentGeneration;
            IWpdParagraph p = doc.InsertParagraph();
            p.StyleName = _styleMap[StyleSlots.MainPersonText].CharacterStyleName;
            string slot = FormatAncestorNumber(frum, gen, _generationNumberPrefixes);
            p.Append($"{slot}");
            if (thru > frum)
                p.Append($" - {FormatAncestorNumber(thru, gen, _generationNumberPrefixes)}");
            p.Append($". {_unknownName} {_unknownName}.", false, _styleMap[StyleSlots.MainPerson]);
        }

        private void EmitRepeatedAncestor(IWpdDocument doc, ReportEntry individual, int gen)
        {
            IWpdParagraph p = doc.InsertParagraph();
            EmitMainPersonName(doc, p, individual, gen);
            p.Append($". See {FormatAncestorNumber(individual.FirstAppearance, gen, _generationNumberPrefixes)}.");

            // this allows for back references... which are distinct, even when the indi repeats
            ConditionallyEmitFamily(doc, individual, gen, -1);
        }
        private string FormatAncestorNumber(BigInteger bi, int gen, bool prefix)
        {
            return prefix
                ? $"{gen:00}-{bi}"
                : $"{bi}";
        }
        private static string EmitChildOfClause(IWpdParagraph p, ReportEntry re)
        {
            p.Append(", ").Append(re.Individual.NounAsChild.ToLower()).Append(" of ");
            string conjunction = " ";
            if (re.ChildhoodFamily.Husband != null)
            {
                p.Append(re.ChildhoodFamily.Husband.Individual.SafeNameForward);
                conjunction = " and ";
            }

            if (re.ChildhoodFamily.Wife != null)
            {
                p.Append(conjunction);
                p.Append(re.ChildhoodFamily.Wife.Individual.SafeNameForward);
            }

            return ",";
        }

        #endregion

        #region Main person output
        private void EmitMainPerson(IWpdDocument doc, ReportEntry re, int gen)
        {
            if (_omitFocusSpouses && re.OutOfFocus) 
                return;

            bool timeToMinimize = _ancestryReport && _minFromGen > 0 && _minFromGen <= gen;
            int genNbrIncr = _ancestryReport ? -1 : 1;

            // begin main person content
            IWpdParagraph p = doc.InsertParagraph();
            EmitMainPersonName(doc, p, re, gen);

            if (!_ancestryReport)
                re.Ancestry?.Emit(p, _lineageListNameFormatting, _styleMap[StyleSlots.GenerationNumber]);

            List<GedcomIndividual> noteworthy = AppendPersonDetails(doc, p, re); 

            string tinue = re.GetContinuation(_generationNumberPrefixes);
            if (tinue != null)
                p.Append(" ").Append(tinue);

            if (timeToMinimize)
            {
                if (_omitBackRefsLater || !_includeBackRefs)
                    return;
                foreach (ReportFamilyEntry family in re.FindMainNumberedFamilies())
                {
                    List<ReportEntry> numbered = family.FindMainNumberedChildren();
                    EmitFamilyIntroLine(doc, numbered.Count, family);
                    foreach (ReportEntry child in numbered)
                    {
                        EmitChildEntry(doc, gen + genNbrIncr, child);
                    }
                }
                return;
            }

            EmitNotes(doc, p, noteworthy);

            ConditionallyEmitFamily(doc, re, gen, genNbrIncr);
        }
        private void EmitMainPersonName(IWpdDocument doc, IWpdParagraph p, ReportEntry indi, int gen)
        {
            p.StyleName = _styleMap[StyleSlots.MainPersonText].CharacterStyleName;
            p.Append($"{indi.GetNumber(_generationNumberPrefixes)}. ");
            p.Append(indi.Individual.SafeGivenName, false, _styleMap[StyleSlots.MainPerson]);
            if (!_suppressGenSuperscripts)
                p.Append($"{_gnMapper.GenerationNumberFor(gen)}", false, _generationNumberFormatting);
            if (!string.IsNullOrEmpty(indi.Individual.SafeSurname))
                p.Append($" {indi.Individual.SafeSurname}", false, _styleMap[StyleSlots.MainPerson]);

            EmitNameIndexEntries(p, indi);

            if (_c.Settings.DebuggingOutput)
            {
                p.Append($" [{indi.NaturalId}]");
            }

            MyReportStats.MainPerson++;
            if (!indi.Individual.PresumedDeceased)
                MyReportStats.MaybeLiving++;
        }


        #endregion

        #region Family Output
        private void ConditionallyEmitFamily(IWpdDocument doc, ReportEntry indi, int gen, int genNbrIncr)
        {
            if (_ancestryReport && (!indi.EmitChildrenAfter || !_includeBackRefs))
                return;

            // list the children
            foreach (ReportFamilyEntry family in indi.SafeFamilyEntries) // todo:FamiliesToReport not populated?
            {
                if ((family.Children?.Count ?? 0) == 0)
                    continue;

                if (!_allFamilies && !family.IsIncluded)
                    continue;
                int numberToList = family.Children.Count;
                EmitFamilyIntroLine(doc, numberToList, family);
                foreach (ReportEntry child in family.Children)
                {
                    if (child.ChildEntryEmitted)
                        continue;
                    if (_ancestryReport && !_includeSibsOfBackrefs && !Follows(child, indi))
                        continue;
                    child.ChildEntryEmitted = true;
                    EmitChildEntry(doc, gen + genNbrIncr, child);
                }
            }
        }

        private bool Follows(ReportEntry child, ReportEntry parent)
        {
            var fatherAtThisPoint = child.AssignedMainNumber * 2;
            return parent.AssignedMainNumber == fatherAtThisPoint
                   || parent.AssignedMainNumber == fatherAtThisPoint + 1;
        }

        private void EmitFamilyIntroLine(IWpdDocument doc, int numberToList, ReportFamilyEntry family)
        {
            if (numberToList < 1) return; // guard
            IWpdParagraph p = doc.InsertParagraph(); // NB insert empty "" para with a styleid DOES NOT WORK
            p.StyleName = _styleMap[StyleSlots.KidsIntro].CharacterStyleName;
            p.Append(((numberToList > 1) ? "Children" : "Child"));
            p.Append($" of {(family.Husband?.Individual?.SafeGivenName) ?? "unknown"}");
            if (!_suppressGenSuperscripts)
                p.Append($"{_gnMapper.GenerationNumberFor(_currentGeneration)}", false, _generationNumberFormatting);
            string s = family.ExtendedWifeName();
            if (!string.IsNullOrEmpty(s))
                p.Append($" and {s}:");
            else
                p.Append($" {(family.Husband?.Individual?.SafeSurname) ?? "unknown"}");
        }

        private void EmitChildEntry(IWpdDocument doc, int g, ReportEntry child)
        {
            IWpdParagraph p;
            p = doc.InsertParagraph();
            p.StyleName = _styleMap[_ancestryReport ? StyleSlots.KidsAlt : StyleSlots.Kids].CharacterStyleName;
            char sp = child.AssignedMainNumber > 9999999 ? ' ' : '\t';
            string kidNbr = child.AssignedMainNumber > 0
                ? $"{child.GetNumber(_generationNumberPrefixes)}.\t{child.ChildNumberRoman}.{sp}"
                : $"\t{child.ChildNumberRoman}.\t";
            p.Append(kidNbr);

            string childNameStyle = _styleMap[StyleSlots.ChildName].CharacterStyleName;
            // conditions to turn off the generation number:
            bool dropNbr = _suppressGenSuperscripts
                           || g < 1
                           || (_ancestryReport && child.AssignedMainNumber < 1);
            // but those do not apply for a register report with 'on all children' set:
            dropNbr &= !(_registerReport && _genNbrAllChildren);
            if (dropNbr || (!_genNbrAllChildren && child.AssignedChildNumber > 1))
            {
                // just the name please: convention applies
                p.Append(child.Individual.SafeNameForward, false, _childNameFormatting);
            }
            else
            {
                // include the generation number
                p.Append(child.Individual.SafeGivenName, false, _childNameFormatting);
                p.Append($"{_gnMapper.GenerationNumberFor(g)}", false, _generationNumberFormatting);
                if (!string.IsNullOrEmpty(child.Individual.SafeSurname))
                    p.Append($" {child.Individual.SafeSurname}", false, _childNameFormatting);
            }
            if (child.AssignedMainNumber == 0)
            {
                MyReportStats.NonContinuedPerson++;
                if (!child.Individual.PresumedDeceased)
                    MyReportStats.MaybeLiving++;
            }

            EmitNameIndexEntries(p, child);
            if (_c.Settings.DebuggingOutput)
            {
                p.Append($" [{child.NaturalId}]");
            }

            AppendPersonDetails(doc, p, child, true);
        }
        #endregion

        #region Person content emitters 
        
        private List<GedcomIndividual> AppendPersonDetails(IWpdDocument doc, IWpdParagraph p, ReportEntry re, bool isChild = false)
        {
            // short-stop: optional standard / brief child line if so configured and the child is continued
            if (isChild && _standardBriefContdChild && re.AssignedMainNumber > 0)
            {
                return EmitStandardBriefChildLine(p, re);
            }

            List<GedcomIndividual> toDoNotes = new List<GedcomIndividual>();

            // optional minimized or reduced output for child listing if there are descendants, except for the last reported generation
            bool minimized = isChild && _c.Settings.MinimizeContinuedChildren && re.HasDescendants && _currentGeneration < _c.Settings.Generations;
            if (minimized)
            {
                p.Append(re.Individual.ReportableSpan);
                p.Append(".");
                return toDoNotes;
            }

            // proceed with "not minimized" output
            // we can decide if we are doing citations
            bool reduced = isChild && _reduceChild && re.HasDescendants && _currentGeneration < _c.Settings.Generations;
            bool doNotCite = isChild && _c.Settings.OmitCitesOnContinued && re.HasDescendants && _currentGeneration < _c.Settings.Generations;
            
            // override these decisions in the case of ancestry report; a bit of "jiggle the handle" approach, eh?
            reduced &= !_ancestryReport || re.AssignedMainNumber > 0;
            doNotCite &= !_ancestryReport || re.AssignedMainNumber > 0;

            bool doCite = _c.Settings.Citations 
                          && !doNotCite
                          && (!_c.Settings.ObscureLiving || !_c.Settings.OmitLivingCitations || re.Individual.PresumedDeceased);

            LocalCitationCoordinator localCitations = BuildLocalCitationCoordinator(re, doCite);

            string comma = null;
            if (!isChild && _ancestryReport && re.HasParents)
            {
                comma = EmitChildOfClause(p, re);
            }

            EmitVitalEvents(p, re, reduced, localCitations, comma);

            if (reduced) 
                return toDoNotes;

            if (_c.Settings.MainPersonNotes)
                toDoNotes.Add(re.Individual);
            
            if (!re.SuppressSpouseInfo)
                AppendMarriagesSentences(p, re, isChild, toDoNotes, reduced, doCite, localCitations);

            return toDoNotes;
        }

        private void EmitVitalEvents(IWpdParagraph p, ReportEntry re, bool reduced, LocalCitationCoordinator lCite, string conn)
        {
            // to position footnote superscripts in the running text correctly (especially,
            // in relation to punctuation), we need to know IN ADVANCE all of the pieces that 
            // will be emitted.  So, figure all that out FIRST and then start outputting it
            FormattedEvent p1_birt = new FormattedEvent() {EventTagCode = TagCode.BIRT, Owner = re}
                .Init("was born", re.Individual.Born, re.Individual.PlaceBorn, reduced ? null : re.Individual.BirthDescription);

            // list the baptism for non-reduced output OR if there is no birth and option set to substitute it
            // note: CHR and BAPM are treated as equivalent in layer(s) below 
            FormattedEvent p2_bapt = (_listBapt && !reduced) || (_c.Settings.BaptIfNoBirt && p1_birt == null)
                ? new FormattedEvent() {EventTagCode = re.Individual.BaptismTagCode, Owner = re }
                    .Init("baptized", re.Individual.Baptized, re.Individual.PlaceBaptized,
                        reduced ? null : re.Individual.BaptizedDescription)
                : null;

            FormattedEvent p3_deat = new FormattedEvent() {EventTagCode = TagCode.DEAT, Owner = re }
                .Init("died", re.Individual.Died, re.Individual.PlaceDied, reduced ? null : re.Individual.DeathDescription);

            FormattedEvent p4_buri = (!reduced && _listBuri)
                ? new FormattedEvent() {EventTagCode = TagCode.BURI, Owner = re }
                    .Init("was buried", re.Individual.Buried, re.Individual.PlaceBuried, re.Individual.BurialDescription,
                        _c.Settings.OmitBurialDate)
                : null;

            if (conn != null && (p1_birt ?? p2_bapt ?? p3_deat ?? p4_buri) != null)
            {
                p.Append(conn);
            }

            // close the name if no BBD to follow
            if ((p3_deat ?? p2_bapt ?? p1_birt) == null)
                p.Append(".");

            // BIRT event
            string clauseOpener = null;
            string clauseEnder = (!reduced && (p2_bapt ?? p3_deat) != null) ? "," : ".";
            EmitEvent(p, p1_birt, lCite, clauseEnder, clauseOpener);

            // BAPM event;
            // note that deferring the 'was' to this point allows the variation
            // "...was born xxx, baptized xxx, and died xxx."
            clauseOpener = (p1_birt == null)
                ? " was"
                : (p3_deat == null)
                    ? " and was"
                    : null;
            clauseEnder = (p3_deat != null) ? "," : ".";
            EmitEvent(p, p2_bapt, lCite, clauseEnder, clauseOpener);

            // DEAT event
            clauseOpener = ((p1_birt ?? p2_bapt) != null)
                ? $" and {re.Individual.Pronoun.ToLower()}"
                : null;
            clauseEnder = ".";
            EmitEvent(p, p3_deat, lCite, clauseEnder, clauseOpener);

            // BURI event
            // style choice: always saying burial as a separate sentence
            clauseOpener = $" {re.Individual.Pronoun}";
            clauseEnder = ".";
            EmitEvent(p, p4_buri, lCite, clauseEnder, clauseOpener);
        }

        private void EmitEvent(IWpdParagraph p, FormattedEvent ev,
            LocalCitationCoordinator lcc, string clauseEnder = null, string clauseOpener = null)
        {
            if (ev == null || string.IsNullOrEmpty(ev.EventString)) return;

            p.Append($"{clauseOpener}{ev.EventString}{clauseEnder}");
            if (lcc.DoCite)
            {
                CitationProposal cp = lcc[ev.EventTagCode.ToString()];
                EventCitations ec = cp?.Citation;
                if (ec?.SelectedItem != null)
                { // the lcc detects multiplicity and here we will push the extra sentence to the EmitNote
                    MyReportStats.Citations++;
                    if (!ec.SelectedItem.IsEmitted)
                        MyReportStats.DistinctCitations++;
                    ec.EmitNote(p.Document, p, cp.AppliesTo());
                } 
                else if (lcc.IsUncited(ev.Owner.InternalId, ev.EventTagCode))
                {
                    MyReportStats.UncitedEvents++;
                    if (_insertUncitedNotes)
                        EventCitations.InsertUnciteNote(p, $"{ev.EventTagCode.Map()}");
                }
            }
            ConditionallyEmitPlaceIndexEntry(p.Document, p, ev);
        }
        private List<GedcomIndividual> EmitStandardBriefChildLine(IWpdParagraph p, ReportEntry re)
        {
            List<GedcomIndividual> toDoNotes = new List<GedcomIndividual>();
            string conn = ", m.";
            FormattedEvent p0_bbp = new FormattedEvent() { EventTagCode = TagCode.BIRT }.Init(", b.", re.Individual.Born, re.Individual.PlaceBorn);
            if (string.IsNullOrEmpty(p0_bbp?.EventString))
            {
                // note: CHR and BAPM are treated as equivalent in layer(s) below 
                p0_bbp = new FormattedEvent() { EventTagCode = re.Individual.BaptismTagCode }.Init(", bp.", re.Individual.Baptized, re.Individual.PlaceBaptized);
            }

            if (!string.IsNullOrEmpty(p0_bbp?.EventString))
            {
                p.Append(p0_bbp.EventString.TrimStart()
                    .Replace(" on ", " ")); // ugly little tweaks; this needs to all be smarter
                ConditionallyEmitPlaceIndexEntry(_c.Model.Doc, p, p0_bbp);
                conn = "; m.";
            }

            string mnbr = re.SafeFamilyEntries.Count > 1 ? $" {_wordsForNumbers[1]}" : "";
            for (int i = 0; i < re.SafeFamilyEntries.Count; i++)
            {
                GedcomIndividual spouze = re.SafeFamilyEntries[i].Family.SpouseOf(re.Individual);
                string s = $"{conn}{mnbr} {spouze?.SafeNameForward ?? GedcomIndividual.UnknownName}";
                p.Append(s);
                EmitNameIndexEntries(p, spouze);
                conn = ", ";
                mnbr = $"{_wordsForNumbers[i + 2]}";
            }

            p.Append(".");
            return toDoNotes;
        }

        private void AppendMarriagesSentences(IWpdParagraph p, ReportEntry re, bool isChild, List<GedcomIndividual> toDoNotes,
            bool reduced, bool doCite, LocalCitationCoordinator lcc)
        {
            bool storyAppended = false;
            for (int mnbr = 0; mnbr < re.SafeFamilyEntries.Count; mnbr++)
            {
                string marriageNumberWord = (re.SafeFamilyEntries.Count > 1)
                    ? (mnbr + 1 < _wordsForNumbers.Length) ? $" {_wordsForNumbers[mnbr + 1]}" : (mnbr + 1).ToString()
                    : null;
                if (marriageNumberWord != null && !re.FamiliesAreSorted)
                    marriageNumberWord += "[?]";
                ReportFamilyEntry family = re.SafeFamilyEntries[mnbr];
                ReportEntry spouse = (family.Husband?.Individual == re.Individual) ? family.Wife : family.Husband;
                ReportEntry main = spouse == family.Wife ? family.Husband : family.Wife;
                p.Append($" {(storyAppended ? re.Individual.SafeNameForward : re.Individual.Pronoun)} married{marriageNumberWord} ");
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

                    EmitNameIndexEntries(p, spouse);
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

                FormattedEvent p5_marr = new FormattedEvent() { EventTagCode = TagCode.MARR, Owner = family}.Init("", family.Family.DateMarried, family.Family.PlaceMarried,
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
                    CitationProposal cp = lcc[TagCode.MARR.ToString() + family.Family.FamilyView.Id];
                    EventCitations ec = cp?.Citation;
                    if (ec?.SelectedItem != null)
                    {
                        MyReportStats.Citations++;
                        if (!ec.SelectedItem.IsEmitted)
                            MyReportStats.DistinctCitations++;
                        ec.EmitNote(_c.Model.Doc, p, cp.AppliesTo());
                    } 
                    else if (p5_marr!=null && lcc.IsUncited(family.InternalId, p5_marr.EventTagCode))
                    {
                        MyReportStats.UncitedEvents++;
                        if (_insertUncitedNotes)
                            EventCitations.InsertUnciteNote(p, $"{TagCode.MARR.Map()}");
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
            FormattedEvent p1BirtBapm = new FormattedEvent() { EventTagCode = TagCode.BIRT, Owner = spouse}.Init("born", spouse.Individual.Born, spouse.Individual.PlaceBorn, spouse.Individual.BirthDescription);
            p1BirtBapm ??= new FormattedEvent() {EventTagCode = spouse.Individual.BaptismTagCode}.Init("baptized",
                spouse.Individual.Baptized, spouse.Individual.PlaceBaptized, spouse.Individual.BaptizedDescription);

            // here we will list the burial iff there is no death
            FormattedEvent p3DeatBuri = new FormattedEvent() { EventTagCode = TagCode.DEAT, Owner = spouse }.Init("died", spouse.Individual.Died, spouse.Individual.PlaceDied, spouse.Individual.DeathDescription);
            p3DeatBuri ??= new FormattedEvent() { EventTagCode = TagCode.BURI }.Init("buried", spouse.Individual.Buried, spouse.Individual.PlaceBuried, spouse.Individual.BurialDescription, _c.Settings.OmitBurialDate);
            
            List<TagCode> eventsToCiteFor = new List<TagCode>();
            if (p1BirtBapm != null) eventsToCiteFor.Add(p1BirtBapm.EventTagCode);
            if (p3DeatBuri != null) eventsToCiteFor.Add(p3DeatBuri.EventTagCode);
            LocalCitationCoordinator localCitations 
                = BuildLocalCitationCoordinator(spouse, _c.Settings.Citations, eventsToCiteFor, false);

            bool hasBb = p1BirtBapm != null;
            bool hasDb = p3DeatBuri != null;
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
                // todo: we NEED a mechanism to do this stuff in a generalized way
                p.Append($" was the {spouse.Individual.NounAsChild.ToLower()} of");
                string conj = null;
                if (hasPere)
                {
                    if (hasMere)
                    {
                        p.Append($" {spousesChildhoodFamily.Husband.GivenOrFullName}"); // uses full name in the pre-binomial era
                    }
                    else
                    {
                        p.Append($" {spousesChildhoodFamily.Husband.SafeNameForward}");
                    }
                    MyReportStats.SpouseParents++;
                    if (!spousesChildhoodFamily.Husband.PresumedDeceased)
                        MyReportStats.MaybeLiving++;
                    EmitNameIndexEntries(p, spousesChildhoodFamily.Husband);
                    if (_c.Settings.DebuggingOutput)
                    {
                        p.Append($" [{spousesChildhoodFamily.Husband.IndividualView.Id}]");
                    }
                    conj = "and ";
                }
                if (hasMere)
                {
                    p.Append($" {conj}{spousesChildhoodFamily.ExtendedWifeName()}"); // extended to recognize no surn
                    MyReportStats.SpouseParents++;
                    if (!spousesChildhoodFamily.Wife.PresumedDeceased)
                        MyReportStats.MaybeLiving++;
                    EmitNameIndexEntries(p, spousesChildhoodFamily.Wife);
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

                EmitEvent(p, p1BirtBapm, localCitations, hasDb?",":closer, connector);
                if (!hasDb)
                    closer = null;
                connector = $" and {spouse.Individual.Pronoun.ToLower()}";
            }
            else
            {
                connector = $" and {spouse.Individual.Pronoun.ToLower()}";
            }

            if (hasDb)
            {
                EmitEvent(p, p3DeatBuri, localCitations, closer, connector);
                closer = null;
            }
            if (closer!=null)
                p.Append(closer);
            return true;
        }
        #endregion

        #region Citation coordinator
        internal LocalCitationCoordinator BuildLocalCitationCoordinator(ReportEntry re, bool doCite, List<TagCode> eventsToCiteFor = null, bool doFamily = true)
        {
            LocalCitationCoordinator lcc = new LocalCitationCoordinator() {DoCite = doCite};
            if (!doCite)
                return lcc;

            // we can detect and optimize out consecutive repeats of the same citation
            // NB this list is ORDERED by the appearance of the cited facts
            eventsToCiteFor ??= new List<TagCode>()
            {
                TagCode.BIRT,
                re.Individual.BaptismTagCode,
                TagCode.DEAT,
                TagCode.BURI
            };
            foreach (TagCode tc in eventsToCiteFor)
            {
                if (!lcc.AddNonNull(re.Individual.CitableEvents?.Find(re.Individual.EventTag(tc))))
                    lcc.NoteUncited(re.InternalId, tc);
            }

            if (doFamily) 
            {
                foreach (ReportFamilyEntry family in re.SafeFamilyEntries)
                {
                    EventCitations ec = family.Family.CitableEvents?.Find(family.Family.EventTag(TagCode.MARR));
                    lcc.AddNonNull(ec, family.Family.FamilyView.Id);
                }
            }

            LocalCitationCoordinator localCitations = CitationCoordinator.Optimize(lcc);
            return localCitations;
        }
        #endregion

   }
}
