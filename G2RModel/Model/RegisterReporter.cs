using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using G2RModel.Model;
using DocxAdapterLib;
using SimpleGedcomLib;
//using Novacode;
using WpdInterfaceLib;

//using Novacode.Src;


namespace Ged2Reg.Model
{
    public class RegisterReporter
    {
        public CitationCoordinator CitationCoordinator { get; internal set; }
        public ReportStats MyReportStats { get; set; }

        private GedcomIndividual _root;
        private ReportContext _c;

        //public ListOfGedcomIndividuals MainIndividuals { get; set; }

        public ListOfGedcomIndividuals[] Generations;
        private List<GedcomIndividual> _nonContinued;
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

        private bool _factDesc;
        private bool _reduceChild;
        private bool _listBapt;
        private bool _listBuri;

        public RegisterReportModel Model { get; set; }

        private HashSet<string> _treedPersons;

        public RegisterReporter Init(GedcomIndividual root, TimeSpan prep)
        {
            MyReportStats = new ReportStats().Init(prep);
            _c = ReportContext.Instance;

            _factDesc = _c.Settings.IncludeFactDescriptions;
            _reduceChild = _c.Settings.ReduceContinuedChildren;
            _listBapt = _c.Settings.IncludeBaptism;
            _listBuri = _c.Settings.IncludeBurial;

            _root = root;
            _root.AssignedMainNumber = 1;

            _cm = new CitationsMap(Model.GedcomFile);

            CitationCoordinator = new CitationCoordinator(Model.GedcomFile.CitationViews)
            {
                IncludeBurial = _listBuri,
                Priorities = _c.Settings.SourcePriorities,
                AnitPriorities = _c.Settings.SourceAnitPriorities,
            };

            int oldState = CitationCoordinator.Reset();

            _c.Settings.BracketArray = _c.Settings.Brackets ? new[] {"[", "]"} : null;

            _treedPersons = new HashSet<string>();

            // the root is generation 0
            // the array holds ordered lists of main persons by generation
            // the array index is the generation number
            // set up Generation 0 to initialize for recursion
            Generations = new ListOfGedcomIndividuals[_c.Settings.Generations];
            Generations[0] = new ListOfGedcomIndividuals();

            AddPersonToGeneration(_root, 0);

            //_root.LocateCitations(_cm);

            _nonContinued = new List<GedcomIndividual>();

            Process(0);

            if (Model.CheckCancel()) throw new CanceledByUserException();
            Model.PostProgress("Analyzing source citations for main persons");

            foreach (ListOfGedcomIndividuals gen in Generations)
            {
                if ((gen?.Count??0) == 0)
                    continue;
                CitationCoordinator.PreProcess(gen.ToList());
            }
            if (Model.CheckCancel()) throw new CanceledByUserException();
            Model.PostProgress("Analyzing source citations for non-continued persons");
            CitationCoordinator.PreProcess(_nonContinued);

            Model.PostProgress("Selecting source citations");
            CitationCoordinator.PreProcess(_nonContinued);
            CitationCoordinator.Process(_c.Settings.CitationStrategy, _c.Settings.FillinCitationStrategy);

            return this;
        }

        private void AddPersonToGeneration(GedcomIndividual p, int ix)
        {
            if (_treedPersons.Contains(p.IndividualView.Id))
            {
                Debug.WriteLine($"Duplicate person, not added: {p.IndividualView.Id}");
            }
            else
            {
                _treedPersons.Add(p.IndividualView.Id);
                Generations[ix].Add(p);
            }
        }

        private void Process(int generation)
        {
            ListOfGedcomIndividuals ip = Generations[generation];
            if (ip.Count == 0)
                return;
            if (generation >= Generations.Length - 1)
            {
                // well, this is awkward....
                // we need to put i. 's on the children of the last gen in the report
                foreach (GedcomIndividual mainIndividual in ip)
                {
                    // being in the list entails: indi has some child[ren]
                    int greatestChildSeq = 0;
                    foreach (GedcomFamily family in mainIndividual.SafeFamilies)
                    {
                        foreach (GedcomIndividual child in family.Children)
                        {
                            if (child.AssignedChildNumber != 0) continue;
                            child.AssignedChildNumber = ++greatestChildSeq;
                            _nonContinued.Add(child);
                        }
                    }
                }
                return;
            }

            // advance to the next generation, starting with an empty list of 'main' persons
            int nextGen = generation + 1;
            ListOfGedcomIndividuals op = Generations[nextGen] = new ListOfGedcomIndividuals();

            //bool hasAnotherGeneration = nextGen < Generations.Length - 1;

            int greatestId = ip[ip.Count - 1].AssignedMainNumber;

            foreach (GedcomIndividual mainIndividual in ip)
            {
                AncestryNameList anl = mainIndividual.Ancestry?.Descend(mainIndividual) ??
                                       new AncestryNameList(mainIndividual);
                // being in the list entails: indi has some child[ren]
                int greatestChildSeq = 0;
                foreach (GedcomFamily family in mainIndividual.SafeFamilies)
                {
                    foreach (GedcomIndividual child in family.Children)
                    {
                        child.Ancestry = anl;
                        child.LocateCitations(_cm);
                        if (child.AssignedChildNumber == 0)
                            child.AssignedChildNumber = ++greatestChildSeq;
                        if (child.NumberOfChildren == 0)
                        {
                            _nonContinued.Add(child);
                            continue;
                        }
                        if (child.AssignedMainNumber == 0)
                            child.AssignedMainNumber = ++greatestId;
                        //op.Add(child);
                        AddPersonToGeneration(child, nextGen);
                    }
                }
            }
            Process(nextGen);
        }

        public void Exec(IWpdDocument doc)
        {
            _styleMap = new Dictionary<StyleSlots, Formatting>();
            foreach (StyleAssignment s in _c.Settings.StyleMap)
            {
                _styleMap.Add(s.Style, new Formatting(){CharacterStyleName = s.StyleName});
            }

            _dateFormatter = new GenealogicalDateFormatter();
            _placeFormatter = new GenealogicalPlaceFormatter()
            {
                DropUSA = _c.Settings.DropUsa, 
                InjectWordCounty = _c.Settings.InjectCounty,
                ReduceOnRepetition = _c.Settings.ReducePlaceNames,
            }.Init();

            _currentGeneration = 0;
            foreach (ListOfGedcomIndividuals generation in Generations)
            {
                _currentGeneration++;
                if (Model.CheckCancel()) throw new CanceledByUserException();
                Debug.WriteLine($"Generation {_currentGeneration} begins {DateTime.Now:G}");
                if (_c.Settings.FullPlaceOncePerGen)
                    _placeFormatter.Reset();
                if ((generation?.Count??0)==0)
                    continue;
                Model.PostProgress($"processing generation {_currentGeneration}");
                foreach (GedcomIndividual individual in generation)
                {
                    Emit(doc, individual, _currentGeneration);
                }
            }

            bool didIx = ConditionallyEmitIndexField(doc, _c.Settings.NameIndexSettings);
            didIx |= ConditionallyEmitIndexField(doc, _c.Settings.PlaceIndexSettings);
            if (didIx && _c.Settings.AsEndnotes)
                doc.InsertPageBreak();

            MyReportStats.EndTime = DateTime.Now;
        }

        public string GetStatsSummary()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Summary of report processing");
            sb.AppendLine($"\tPrep time..................{MyReportStats.PrepTime:h\\:mm\\:ss\\.fff}");
            sb.AppendLine($"\tRun time...................{MyReportStats.ReportTime:h\\:mm\\:ss\\.fff}");
            sb.AppendLine($"\tMain/continued persons.....{MyReportStats.MainPerson,8:N0}");
            sb.AppendLine($"\tNon-continued children.....{MyReportStats.NonContinuedPerson,8:N0}");
            sb.AppendLine($"\tMain spouses...............{MyReportStats.MainSpouse,8:N0}");
            sb.AppendLine($"\tSpouses of non-continued...{MyReportStats.NonContinuedSpouses,8:N0}");
            sb.AppendLine($"\tParents of spouses.........{MyReportStats.SpouseParents,8:N0}");
            sb.AppendLine($"\tPersons possibly living....{MyReportStats.MaybeLiving,8:N0}");
            sb.AppendLine($"\tCitations..................{MyReportStats.Citations,8:N0}");
            sb.Append($"\tDistinct citations.........{MyReportStats.DistinctCitations,8:N0}");
            return sb.ToString();
        }

        private static bool ConditionallyEmitIndexField(IWpdDocument doc, IndexSettings ixs)
        {
            if (!ixs.Enabled) return false;
            doc.InsertPageBreak();
            IWpdParagraph p = doc.InsertParagraph(ixs.IndexHeading);
            p = doc.InsertParagraph();
            var ixf = doc.BuildIndexField();
            ixf.IndexName = ixs.IndexName;
            ixf.Columns = ixs.Columns;
            ixf.EntryPageSeparator = ixs.Separator;
            //var ixf = new OoxIndexField(doc)
            //{
            //    IndexName = ixs.IndexName,
            //    Columns = ixs.Columns,
            //    EntryPageSeparator = ixs.Separator
            //}; //.Build();
            p.AppendField(ixf.Build());
            return true;
        }

        private void Emit(IWpdDocument doc, GedcomIndividual individual, int gen)
        {
            //Formatting superFormatting = new Formatting() { Script = Script.superscript };
            Formatting generationNumberFormatting = new Formatting() { CharacterStyleName = _styleMap[StyleSlots.GenerationNumber].CharacterStyleName }; //  switched this to the style
            Formatting introFormatting = new Formatting(){Bold = _c.Settings.IntroBold, Italic = _c.Settings.IntroItalic};
            // begin main person content
            IWpdParagraph p = doc.InsertParagraph();
            p.StyleName = _styleMap[StyleSlots.MainPersonText].CharacterStyleName;
            p.Append($"{individual.AssignedMainNumber}. ");
            p.Append(individual.SafeGivenName, false, _styleMap[StyleSlots.MainPerson]);
            //p.Append($"{gen}", _styleMap[StyleSlots.GenerationNumber]);
            //p.Append($"{gen}", generationNumberFormatting);
            p.Append($"{gen}", false, generationNumberFormatting);
            p.Append($" {individual.SafeSurname}", false, _styleMap[StyleSlots.MainPerson]);
            ConditionallyEmitNameIndex(doc, p, individual);
            if (_c.Settings.DebuggingOutput)
            {
                p.Append($" [{individual.IndividualView.Id}]");
            }

            MyReportStats.MainPerson++;
            if (!individual.NotLiving)
                MyReportStats.MaybeLiving++;
            
            individual.Ancestry?.Emit(p, _styleMap[StyleSlots.MainPersonText], _styleMap[StyleSlots.GenerationNumber]);

            List<GedcomIndividual> noteworthy = AppendPersonDetails(p, individual);

            bool lastLineWasDivider = false;
            bool dividersApplied = false;
            foreach (GedcomIndividual indiNotes in noteworthy)
            {
                if (indiNotes == null) continue;
                string s = indiNotes.PersonNotes;
                if (string.IsNullOrEmpty(s)) continue;
                p = doc.InsertParagraph();
                p = doc.InsertParagraph();
                if (!lastLineWasDivider && _c.Settings.NotesDividers)
                {
                    //p.InsertHorizontalLine(HorizontalBorderPosition.top, BorderStyle.Tcbs_single);
                    p.InsertHorizontalLine(lineType:"single", position:"top");
                    dividersApplied = true;
                }
                p.StyleName = _styleMap[StyleSlots.BodyTextIndent].CharacterStyleName;
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
                    p.StyleName = _styleMap[StyleSlots.BodyTextIndent].CharacterStyleName;
                    p.Append(para);
                }
            }

            if (dividersApplied && !lastLineWasDivider)
            {
                //p.InsertHorizontalLine(HorizontalBorderPosition.bottom, BorderStyle.Tcbs_single);
                p.InsertHorizontalLine(lineType: "single");
                lastLineWasDivider = true;
            }

            // "something happens" that can disorder the families
            // so... here at the LPM we will try to get them sorted
            individual.SortFamilies();

            // list the children
            foreach (GedcomFamily family in individual.SafeFamilies)
            {
                if ((family.Children?.Count??0) == 0)
                    continue;

                p = doc.InsertParagraph(); // NB insert empty "" para with a styleid DOES NOT WORK
                p.StyleName = _styleMap[StyleSlots.KidsIntro].CharacterStyleName;
                p.Append(((family.Children.Count > 1) ? "Children" : "Child"));
                p.Append($" of {(family.Husband?.SafeNameForward) ?? "unknown"}");
                p.Append($" and {(family.Wife?.SafeNameForward) ?? "unknown"}:");
                foreach (GedcomIndividual child in family.Children)
                {
                    if (child.ChildEntryEmitted) 
                        continue;
                    child.ChildEntryEmitted = true;
                    p = doc.InsertParagraph();
                    p.StyleName = _styleMap[StyleSlots.Kids].CharacterStyleName;
                    string kidNbr = child.AssignedMainNumber > 0
                        ? $"{child.AssignedMainNumber}.\t{child.ChildNumberRoman}.\t"
                        : $"\t{child.ChildNumberRoman}.\t";
                    p.Append(kidNbr);
                    p.Append(child.SafeGivenName);
                    p.Append($"{gen+1}", false, generationNumberFormatting);
                    p.Append($" {child.SafeSurname}");
                    if (child.AssignedMainNumber == 0)
                    {
                        MyReportStats.NonContinuedPerson++;
                        if (!child.NotLiving)
                            MyReportStats.MaybeLiving++;
                    }
                    ConditionallyEmitNameIndex(doc, p, child);
                    if (_c.Settings.DebuggingOutput)
                    {
                        p.Append($" [{child.IndividualView.Id}]");
                    }
                    AppendPersonDetails(p, child, true);
                }
            }
        }

        private Regex _rexDivider1 = new Regex(@"^[\-_]+$");
        private Regex _rexDivider2 = new Regex(@"^[=]+$");

        private bool AppliesAsDivider(IWpdParagraph p, string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            if (_rexDivider1.IsMatch(s))
            {
                //p.InsertHorizontalLine(HorizontalBorderPosition.bottom, BorderStyle.Tcbs_single);
                p.InsertHorizontalLine(lineType: "single");
                return true;
            }

            if (!_rexDivider2.IsMatch(s))
                return false;

            //p.InsertHorizontalLine(HorizontalBorderPosition.bottom, BorderStyle.Tcbs_double);
            p.InsertHorizontalLine(lineType: "double");
            return true;
        }

        internal bool ConditionallyEmitNameIndex(IWpdDocument doc, IWpdParagraph p, GedcomIndividual indi)
        {
            if (!_c.Settings.NameIndexSettings.Enabled) return false;
            string ixn = _c.Settings.NameIndexSettings.IndexName;
            if (string.IsNullOrEmpty(ixn))
                ixn = null;
            var ex = doc.BuildIndexEntryField(ixn, $"{indi.SafeSurname}:{indi.SafeGivenName}").Build();
            p.AppendField(ex);
            return true;
        }
        internal bool ConditionallyEmitPlaceIndex(IWpdDocument doc, IWpdParagraph p, FormattedEvent fe)
        {
            if (!_c.Settings.PlaceIndexSettings.Enabled || string.IsNullOrEmpty(fe?.PlaceIndexEntry)) return false;
            string ixn = _c.Settings.PlaceIndexSettings.IndexName;
            if (string.IsNullOrEmpty(ixn))
                ixn = null;
            var ex = doc.BuildIndexEntryField(ixn, fe.PlaceIndexEntry).Build();
            p.AppendField(ex); // todo: consider INSERTING it if there is more text after the place
            return true;
        }

        private List<GedcomIndividual> AppendPersonDetails(IWpdParagraph p, GedcomIndividual indi, bool isChild = false)
        {
            List<GedcomIndividual> toDoNotes = new List<GedcomIndividual>();
            // optional minimized or reduced output for child listing if there are descendants, except for the last reported generation
            bool minimized = isChild && _c.Settings.MinimizeContinuedChildren && indi.HasDescendants && _currentGeneration < _c.Settings.Generations;
            if (minimized)
            {
                p.Append(indi.ReportableSpan);
                p.Append(".");
                return toDoNotes;
            }

            bool reduced = isChild && _reduceChild && indi.HasDescendants && _currentGeneration < _c.Settings.Generations;
            bool doNotCite = isChild && _c.Settings.OmitCitesOnContinued && indi.HasDescendants && _currentGeneration < _c.Settings.Generations;

            // to position footnote superscripts in the running text correctly (especially,
            // in relation to punctuation), we need to know IN ADVANCE all of the pieces that 
            // will be emitted.  So, figure all that out FIRST and then start outputting it
            FormattedEvent p1_birt = ConditionalEvent("was born", indi.Born, indi.PlaceBorn, reduced ? null : indi.BirthDescription);
            // list the baptism for non-reduced output OR if there is no birth and option set to substitute it
            FormattedEvent p2_bapt = (_listBapt && !reduced) || (_c.Settings.BaptIfNoBirt && p1_birt == null)
                ? ConditionalEvent("baptized", indi.Baptized, indi.PlaceBaptized, reduced ? null : indi.BaptizedDescription)
                : null;
            FormattedEvent p3_deat = ConditionalEvent("died", indi.Died, indi.PlaceDied, reduced ? null : indi.DeathDescription);
            FormattedEvent p4_buri = (!reduced && _listBuri) 
                ? ConditionalEvent("was buried", indi.Buried, indi.PlaceBuried, indi.BurialDescription, _c.Settings.OmitBurialDate)
                : null;

            // knowing all the pieces, we can add the conjunctions, [NOT commas], pronouns needed to the text
            //string pendingPunctuation = "";
            if (p2_bapt != null)
            {
                if (p1_birt == null)
                {
                    //    p2_bapt = $" {p2_bapt}";
                    //    //pendingPunctuation = ",";
                    //}
                    //else
                    //{
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
                p3_deat.EventString = $" and {indi.Pronoun.ToLower()}{p3_deat.EventString}"; // remove pendingPunctuation
            }

            if (p3_deat != null)
                p3_deat.EventString = $"{p3_deat.EventString}.";
            else if (p2_bapt != null)
                p2_bapt.EventString = $"{p2_bapt.EventString}.";
            else if (p1_birt != null)
                p1_birt.EventString = $"{p1_birt.EventString}.";
            else
                p.Append(".");

            // and we can detect and optimize out consecutive repeats of the same citation
            // NB this list is ORDERED by the appearance of the cited facts
            CitationProposals cp = new CitationProposals();

            cp.AddNonNull(indi.CitableEvents?.Find(indi.EventTag(TagCode.BIRT)));
            cp.AddNonNull(indi.CitableEvents?.Find(indi.EventTag(TagCode.BAPM)));
            cp.AddNonNull(indi.CitableEvents?.Find(indi.EventTag(TagCode.DEAT)));
            cp.AddNonNull(indi.CitableEvents?.Find(indi.EventTag(TagCode.BURI)));

            for (int mnbr = 0; mnbr < indi.SafeFamilies.Count; mnbr++)
            {
                GedcomFamily family = indi.SafeFamilies[mnbr];
                EventCitations ec = family.CitableEvents?.Find(family.EventTag(TagCode.MARR));
                cp.AddNonNull(ec, family.FamilyView.Id);
            }

            CitationProposals chosenCitations = CitationCoordinator.Optimize(cp);

            if (p4_buri != null)
            {
                // style choice: always saying burial as a separate sentence
                p4_buri.EventString = $" {indi.Pronoun}{p4_buri.EventString}.";
            }
            
            bool doCite = _c.Settings.Citations && !doNotCite && (!_c.Settings.ObscureLiving || !_c.Settings.OmitLivingCitations || indi.NotLiving);
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
                    //ec1?.EmitNote(_c.Model.Doc, p);
                }
                ConditionallyEmitPlaceIndex(_c.Model.Doc, p, p1_birt);
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
                    //chosenCitations[TagCode.BAPM.ToString()]?.EmitNote(_c.Model.Doc, p);
                    //ec2?.EmitNote(_c.Model.Doc, p);
                }
                ConditionallyEmitPlaceIndex(_c.Model.Doc, p, p2_bapt);
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
                    //chosenCitations[TagCode.DEAT.ToString()]?.EmitNote(_c.Model.Doc, p);
                    //ec3?.EmitNote(_c.Model.Doc, p);
                }
                ConditionallyEmitPlaceIndex(_c.Model.Doc, p, p3_deat);
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
                    //chosenCitations[TagCode.BURI.ToString()]?.EmitNote(_c.Model.Doc, p);
                    //ec4?.EmitNote(_c.Model.Doc, p);
                }
                ConditionallyEmitPlaceIndex(_c.Model.Doc, p, p4_buri);
            }

            if (reduced) return toDoNotes;

            if (_c.Settings.MainPersonNotes)
                toDoNotes.Add(indi);

            bool storyAppended = false;
            for (int mnbr = 0; mnbr < indi.SafeFamilies.Count; mnbr++)
            {
                string mid = (indi.SafeFamilies.Count > 1) 
                    ? (mnbr + 1 < _wordsForNumbers.Length) ? $" {_wordsForNumbers[mnbr + 1]}" : (mnbr + 1).ToString() 
                    : null;
                if (mid != null && !indi.FamiliesAreSorted)
                    mid += "[?]";
                GedcomFamily family = indi.SafeFamilies[mnbr];
                GedcomIndividual spouse = (family.Husband == indi) ? family.Wife : family.Husband;
                if (_c.Settings.SpousesNotes)
                    toDoNotes.Add(spouse);
                p.Append($" {(storyAppended ? indi.SafeNameForward : indi.Pronoun)} married{mid} ");
                 if (_c.Settings.DebuggingOutput)
                {
                    p.Append($"[{family.FamilyView.Id}] ");
                }
                if (spouse != null)
                {
                    p.Append(spouse.SafeNameForward);
                    if (isChild && indi.AssignedMainNumber == 0)
                    {
                        MyReportStats.NonContinuedSpouses++;
                        if (!indi.NotLiving)
                            MyReportStats.MaybeLiving++;
                    }
                    else if (!isChild)
                    {
                        MyReportStats.MainSpouse++;
                        if (!indi.NotLiving)
                            MyReportStats.MaybeLiving++;
                    }
                    ConditionallyEmitNameIndex(_c.Model.Doc, p, spouse);
                    if (_c.Settings.DebuggingOutput)
                    {
                        p.Append($" [{spouse.IndividualView.Id}]");
                    }
                }
                else
                {
                    p.Append("(unknown)");
                }
                FormattedEvent p5_marr = ConditionalEvent("", family.DateMarried, family.PlaceMarried, reduced ? null : family.MarriageDescription);
                if (!string.IsNullOrEmpty(p5_marr?.EventString))
                {
                    p.Append($"{p5_marr.EventString}");
                    ConditionallyEmitPlaceIndex(_c.Model.Doc, p, p5_marr);
                }
                // todo: divorces (complicates the punctuation and citation placement)
                p.Append(".");
                if (doCite)
                {
                    EventCitations ec = chosenCitations[TagCode.MARR.ToString() + family.FamilyView.Id];
                    if (ec?.SelectedItem != null)
                    {
                        MyReportStats.Citations++;
                        if (!ec.SelectedItem.IsEmitted)
                            MyReportStats.DistinctCitations++;
                        ec.EmitNote(_c.Model.Doc, p);
                    }
                    //chosenCitations[TagCode.MARR.ToString()+family.FamilyView.Id]?.EmitNote(_c.Model.Doc, p);
                    //EventCitations ec = family.CitableEvents?.Find(family.EventTag(TagCode.MARR));
                    //ec?.EmitNote(_c.Model.Doc, p);
                }
                storyAppended = AppendSpouseSentence2(p, spouse);
                //...
            }

            return toDoNotes;
        }

        private bool AppendSpouseSentence2(IWpdParagraph p, GedcomIndividual spouse)
        {
            GedcomFamily spousesChildhoodFamily = ReportContext.Instance.Model.FindAsChildInFamily(spouse);
            if (spousesChildhoodFamily == null) return false;

            // here we will list the baptism iff there is no birth 
            FormattedEvent p1_birt = ConditionalEvent("born", spouse.Born, spouse.PlaceBorn, spouse.BirthDescription);
            FormattedEvent p2_bapt = p1_birt == null
                ? ConditionalEvent("baptized", spouse.Baptized, spouse.PlaceBaptized, spouse.BaptizedDescription)
                : null;

            // here we will list the burial iff there is no death
            FormattedEvent p3_deat = ConditionalEvent("died", spouse.Died, spouse.PlaceDied, spouse.DeathDescription);
            FormattedEvent p4_buri = p3_deat == null
                ? ConditionalEvent("buried", spouse.Buried, spouse.PlaceBuried, spouse.BurialDescription, _c.Settings.OmitBurialDate)
                : null;

            // to reduce the number of cites and get the minimal case... b&d same source... down to one
            // note that birt/bapm and deat/buri are natural synonyms, and 
            // we are listing only one from each pair here, so, we have at most two citations
            EventCitations ec_bb = spouse.CitableEvents?.Find(spouse.EventTag(p1_birt == null ? TagCode.BAPM : TagCode.BIRT));
            EventCitations ec_di = spouse.CitableEvents?.Find(spouse.EventTag(p3_deat == null ? TagCode.BURI : TagCode.DEAT));

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

            p.Append($" {spouse.Pronoun}");
            bool isOpen = false;
            bool doCite = _c.Settings.Citations && (!_c.Settings.ObscureLiving || !_c.Settings.OmitLivingCitations || spouse.NotLiving);
            string connector = null;
            string closer = ".";

            if (hasRents)
            {
                p.Append($" was the {spouse.NounAsChild.ToLower()} of");
                string conj = null;
                if (hasPere)
                {
                    p.Append($" {spousesChildhoodFamily.Husband.SafeNameForward}");
                    MyReportStats.SpouseParents++;
                    if (!spousesChildhoodFamily.Husband.NotLiving)
                        MyReportStats.MaybeLiving++;
                    ConditionallyEmitNameIndex(_c.Model.Doc, p, spousesChildhoodFamily.Husband);
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
                    if (!spousesChildhoodFamily.Wife.NotLiving)
                        MyReportStats.MaybeLiving++;
                    ConditionallyEmitNameIndex(_c.Model.Doc, p, spousesChildhoodFamily.Wife);
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
                        connector = $" and {spouse.Pronoun.ToLower()} was";
                    else
                        connector = $",";
                }
                else
                {
                    connector = " was";
                }

                p.Append($"{connector}{p1_birt?.EventString??p2_bapt?.EventString}{(hasDb?",":null)}");
                ConditionallyEmitPlaceIndex(_c.Model.Doc, p, p1_birt??p2_bapt);
                if (hasDb)
                {
                    connector = $" and {spouse.Pronoun.ToLower()}";
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
                    //ec_bb?.EmitNote(_c.Model.Doc, p);
                }
            }

            if (hasDb)
            {
                p.Append($"{connector}{p3_deat?.EventString ?? p4_buri?.EventString}");
                ConditionallyEmitPlaceIndex(_c.Model.Doc, p, p3_deat??p4_buri);
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
                    //ec_di?.EmitNote(_c.Model.Doc, p);
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

        private char[] _splitSpace = {' '};
        private int _currentGeneration;
        private CitationsMap _cm;

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

        public class ReportStats
        {
            public TimeSpan PrepTime { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public TimeSpan ReportTime => EndTime.Subtract(StartTime);
            public int MainPerson { get; set; }
            public int MainSpouse { get; set; }
            public int NonContinuedPerson { get; set; }
            public int NonContinuedSpouses { get; set; }
            public int Citations { get; set; }
            public int DistinctCitations { get; set; }
            public int SpouseParents { get; set; }
            public int MaybeLiving { get; set; }

            public ReportStats Init(TimeSpan prep)
            {
                PrepTime = prep;
                StartTime = DateTime.Now;
                return this;
            }
        }
    }
}
