using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using G2RModel.Model;
using SimpleGedcomLib;

namespace Ged2Reg.Model
{

    public class GedcomIndividual
    {
        public static bool ConsiderLivingStatus = false;
        public static string UnknownName => ReportContext.Instance.Settings.UnknownInReport;

        private static G2RSettings _settings;
        internal static G2RSettings Settings => _settings ??= ReportContext.Instance?.Settings;
        public static bool DownshiftName;

        public GenealogicalNameFormatter FormattedName { get; set; }

        public GedcomIndividual() { }

        /// <summary>
        /// The next-lower level of the model;
        /// the vital events are pre-processed here in the set {} function.
        /// </summary>
        public IndividualView IndividualView
        {
            get => _iView;
            set
            {
                // this assumes that Settings is NOT determined yet
                _iView = value;
                if (_iView == null) return;

                Tag tag = _iView.IndiTag.GetChild(TagCode.BIRT);
                Born = tag?.GetChild(TagCode.DATE)?.Content;
                PlaceBorn = tag?.GetChild(TagCode.PLAC)?.Content;
                BirthDescription = tag?.FullText();

                Tag tagD = _iView.IndiTag.GetChild(TagCode.DEAT);
                Died = tagD?.GetChild(TagCode.DATE)?.Content;
                PlaceDied = tagD?.GetChild(TagCode.PLAC)?.Content;
                DeathDescription = tagD?.FullText();

                ActualYearBorn = GedDate.ExtractYear(Born, defaultVal: null);
                YearBorn = ActualYearBorn ?? "0000";
                //YearBorn = GedDate.ExtractYear(Born, defaultVal: "0000");
                YearDied = GedDate.ExtractYear(Died, defaultVal: "0000");
                LifeSpan = $"{ParseAndPad(YearBorn)} - {ParseAndPad(YearDied)}";
                if (NullSpan.Equals(LifeSpan)) LifeSpan = "";

                tag = _iView.IndiTag.GetChild(TagCode.BURI);
                Buried = tag?.GetChild(TagCode.DATE)?.Content;
                PlaceBuried = tag?.GetChild(TagCode.PLAC)?.Content;
                BurialDescription = tag?.FullText();

                tag = _iView.IndiTag.GetChild(TagCode.BAPM) ?? _iView.IndiTag.GetChild(TagCode.CHR);
                Baptized = tag?.GetChild(TagCode.DATE)?.Content;
                PlaceBaptized = tag?.GetChild(TagCode.PLAC)?.Content;
                BaptizedDescription = tag?.FullText();
                BaptismTagCode = tag?.Code ?? TagCode.BAPM;

                InitPersonalName();
            }
        }
        private string ParseAndPad(string year)
        {
            if (!int.TryParse(year, out int y))
                return null;
            return $"{y:0000}";
        }

        //private string _givenName;
        //private string _surname;
        //private bool _noSurname;
        //private bool _unknownGivenName;
        //private bool _unknownSurname;

        private GenealogicalNameFormatter InitPersonalName()
        {
            Tag nameTag = _iView.IndiTag.GetChild(TagCode.NAME);
            FormattedName = GenealogicalNameFormatter.Reformat(
                nameTag?.Content,
                nameTag?.GetChild(TagCode.GIVN)?.Content,
                nameTag?.GetChild(TagCode.SURN)?.Content
                );
            return FormattedName;
            //// we're going paper over an omission in the library and 
            //// see the GIVN and SURN tags if they exist
            //_givenName = _iView.IndiTag.GetChild(TagCode.NAME)?.GetChild(TagCode.GIVN)?.Content;
            //_givenName ??= _iView.GivenName;
            //_surname = _iView.IndiTag.GetChild(TagCode.NAME)?.GetChild(TagCode.SURN)?.Content;
            //_surname ??= _iView.Surname;

            //_noSurname = string.IsNullOrEmpty(_surname);
            //if (Settings == null) 
            //    return; // no context yet
            //_unknownGivenName = IsUnknown(_givenName);
            //_unknownSurname = IsUnknown(_surname);

            //if (!DownshiftName)
            //    return;

            //// todo: 

        }

        public string Name => $"{Surname}, {GivenName}";
        public string NameForward => string.IsNullOrEmpty(Surname) ? GivenName : $"{GivenName} {Surname}";
        public string GivenName => FormattedName.UnknownGivenName
            ? Settings.UnknownInReport
            : FormattedName.GivenNames;

        /// <summary>
        /// If we fail to consider the given name here we wind up with nonsense
        /// such as Charlemagne (Unknown) in the output
        /// Heh.  this can be called when there IS NO 'report context"
        /// making rather a mess for the grid... todo make better sense of this...
        /// </summary>
        public string Surname => FormattedName.UnknownGivenName && FormattedName.UnknownSurname || FormattedName.UnknownSurname
            ? Settings?.UnknownInReport ?? FormattedName.Surname
            : FormattedName.Surname;

        internal bool IsUnknown(params string[] s)
        {
            if (!Settings.HandleUnknownNames)
                return false;

            foreach (string t in s)
            {
                if (!(string.IsNullOrEmpty(t) || t.Equals(Settings.UnknownInSource))) 
                    return false;
            }

            return true;
        }

        public bool NameIsUnknown => IsUnknown(IndividualView?.Surname, IndividualView?.GivenName);

        //public string SafeName => IsUnknown(IndividualView?.GivenName, IndividualView?.Surname)
        //    ? Settings.UnknownInReport
        //    : $"{SafeSurname}, {SafeGivenName}";

        public string SafeNameForward => IsUnknown(IndividualView?.GivenName, IndividualView?.Surname)
            ? Settings.UnknownInReport
            : (string.IsNullOrEmpty(SafeSurname) ? SafeGivenName : $"{SafeGivenName} {SafeSurname}");

        public string SafeGivenName => PresumedDeceased || !ConsiderLivingStatus ? $"{GivenName}" : "(Living)";
        public string SafeSurname => $"{Surname}"; // this usage is ambiguous
        public string IndexableSurname => string.IsNullOrEmpty(SafeSurname) ? NoSurnameIndexValue : SafeSurname;

        public static string NoSurnameIndexValue { get; set; } = "(No surname)";

        public bool HasNoSurname => FormattedName.NoSurname; // the NAME tag has a // (empty surname)

        //public bool MayBeLiving { get; set; }
        private bool _presumedDeceased;

        public bool PresumedDeceased
        {
            get
            {
                if (!DidEvaluateLivingStatus)
                    EvalLivingStatus(); // last chance!
                return _presumedDeceased;
            }
            set => _presumedDeceased = value;
        }

        public bool DidEvaluateLivingStatus { get; set; }

        public string Born { get; set; }
        public string PlaceBorn { get; set; }
        public string YearBorn { get; set; }
        internal string ActualYearBorn { get; private set; }
        public string BirthDescription { get; set; }
        public string Died { get; set; }
        public string PlaceDied { get; set; }
        public string YearDied { get; set; }
        public string DeathDescription { get; set; }
        public string LifeSpan { get; set; }
        public string Buried { get; set; }
        public string PlaceBuried { get; set; }
        public string BurialDescription { get; set; }
        public string Baptized { get; set; }
        public string PlaceBaptized { get; set; }
        public TagCode BaptismTagCode { get; set; } = TagCode.BAPM;
        public string BaptizedDescription { get; set; }

        #region transients
        //public BigInteger AssignedMainNumber { get; set; }
        //public int AssignedChildNumber { get; set; }
        //public AncestryNameList Ancestry { get; set; }

        //// these are used to control output positioning 
        //// on the Ancestors report
        //public bool SuppressSpouseInfo { get; set; }
        //public bool EmitChildrenAfter { get; set; }

        //// this is used on ancestry report to implement
        //// options and repositioning of list(s) of children
        //public List<GedcomFamily> FamiliesToReport { get; set; }
        //// this is used where we stop exploding to avoid repetition /
        //// recursion, to reference the number of the repeated ancestor
        //public List<GedcomIndividual> ContinuesWith { get; set; }
        //public int GenerationInCurrentReport { get; set; }
        public ReportEntry FirstReportEntry { get; set; }
        public bool FamiliesAreSorted { get; set; }
        #endregion

        public string Pronoun => "M".Equals(Gender) ? "He" : "She";
        public string AntiPronoun => "M".Equals(Gender) ? "She" : "He";

        public string NounAsChild => "M".Equals(Gender) ? "Son" : "Daughter";
        public string NounAsSpouse => "M".Equals(Gender) ? "Husband" : "Wife";

        public string PronounPossessive => "M".Equals(Gender) ? "His" : "Her";

        public string Gender => IndividualView?.IndiTag?.GetChild(TagCode.SEX)?.Content ?? "U";

        //public string ChildNumberRoman => AssignedChildNumber.ToRoman();
        public bool ChildEntryEmitted { get; set; }

        private string _reportableSpan;
        private int? _numberOfChildren;
        public int NumberOfChildren => _numberOfChildren ?? (_numberOfChildren = CountChildren()) ?? 0;
        public bool HasDescendants => NumberOfChildren > 0;
        public bool HasParents => ChildhoodFamily?.Husband != null || ChildhoodFamily?.Wife != null;

        public string ReportableSpan => _reportableSpan ?? (_reportableSpan = BuildReportableSpan());

        private List<GedcomFamily> _families;

        public static readonly List<GedcomFamily> SafeEmptyFamilies = new List<GedcomFamily>();

        public List<GedcomFamily> SafeFamilies => Families ?? SafeEmptyFamilies;

        public List<GedcomFamily> Families
        {
            get => _families;
            set
            {
                _families = value;
                FamiliesAreSorted = false;
            }
        }


        public List<GedcomIndividual> Spouses { get; set; }

        public GedcomFamily ChildhoodFamily { get; set; }

        public CitablePersonEvents CitableEvents { get; set; }


        //public string GetNumber(bool withGeneration) => withGeneration 
        //    ? $"{GenerationInCurrentReport:00}-{AssignedMainNumber}" 
        //    : $"{AssignedMainNumber}";

        private const string NullSpan = "0000 - 0000";

        private IndividualView _iView;

        public void Reset()
        {
            //InitPersonalName();
            (FormattedName ??= InitPersonalName()).Reformat();

            foreach (GedcomFamily family in SafeFamilies)
            {
                family.Reset();
            }

            //ContinuesWith = null;
            //FamiliesToReport = null;
            Families = null;
            Spouses = null;
            FirstReportEntry = null;
            //AssignedChildNumber = 0;
            //AssignedMainNumber = 0;
            //GenerationInCurrentReport = 0;
            ChildEntryEmitted = false;
            DidEvaluateLivingStatus = false;
            // = null;
            SortFamilies();
        }

        public void EvalLivingStatus()
        {
            if (DidEvaluateLivingStatus)
                return;

            // decide if the person is, or may possibly be, still living
            if (_iView.IndiTag.GetChild(TagCode.DEAT) != null)
            {
                // take this to mean, some basis to suppose not living
                _presumedDeceased = true;
            }

            int boundsYear = ReportContext.Instance.Settings.PresumedLivingBoundaryYear;

            // born 100+ years ago, assume not
            int iBorn = GenealogicalDateFormatter.ParseYear(YearBorn);
            //int.TryParse(YearBorn, out int iBorn);
            if (iBorn > 0 && iBorn < boundsYear)
            {
                _presumedDeceased = true;
            }

            if (_presumedDeceased)
            {
                DidEvaluateLivingStatus = true;
                return;
            }

            // still undecided...
            // we can apply further rules for inferring a person not living
            // we have the indi in relation to spouse and children, as well as the 
            // children of the indi to consider
            if (Families == null) 
                FindFamilies(false);

            int iParentsMarr = GenealogicalDateFormatter.ParseYear(ChildhoodFamily?.BestYear);
            _presumedDeceased |= iParentsMarr < boundsYear - 25;

            if (_presumedDeceased)
            {
                DidEvaluateLivingStatus = true;
                return;
            }

            foreach (GedcomFamily family in SafeFamilies)
            {
                int yMarr = GenealogicalDateFormatter.ParseYear(family.BestYear);
                GedcomIndividual spouse = family.SpouseOf(this);
                // too late! Spouses.Add(spouse);

                int ySpouseBirt = GenealogicalDateFormatter.ParseYear(spouse?.YearBorn ?? "");
                int ySpouseDeat = GenealogicalDateFormatter.ParseYear(spouse?.YearDied ?? "");

                if (!_presumedDeceased)
                {
                    // married over 75 years ago, assume not 
                    _presumedDeceased |= yMarr < boundsYear + 25;

                    // spouse born 100 years ago, assume not
                    _presumedDeceased |= ySpouseBirt < boundsYear;

                    // spouse died 75 years ago, assume not
                    _presumedDeceased |= ySpouseDeat < boundsYear + 25;
                }

                //todo: pull this loop out and apply to childhoodfamily also
                if (!_presumedDeceased)
                {
                    // if any child was born or died or married more than 75 YA, assume not 
                    foreach (GedcomIndividual child in family.Children)
                    {
                        child.ChildhoodFamily = child.ChildhoodFamily ?? family;
                        int icBorn = GenealogicalDateFormatter.ParseYear(child.YearBorn ?? "");
                        int icDeat = GenealogicalDateFormatter.ParseYear(child.YearDied ?? "");
                        int icMarr = GenealogicalDateFormatter.ParseYear(child.EarliestMarriage ?? "");
                        if (icBorn >= boundsYear + 25 && icDeat >= boundsYear + 25 && icMarr >= boundsYear + 25)
                            continue;
                        _presumedDeceased = true;
                        break;
                    }
                }

                // todo: reconsider this, probably unreachable in any case where it might matter
                // consider the children... as explained by the parents' data
                // if either parent was born 125+ YA, or died 100+ YA, assume the child is not living 
                int iBirt = GenealogicalDateFormatter.ParseYear(YearBorn);
                int iDeat = GenealogicalDateFormatter.ParseYear(YearDied);

                bool oldParents = iBirt < boundsYear - 25 || iDeat < boundsYear;
                oldParents |= ySpouseBirt < boundsYear - 25 || ySpouseDeat < boundsYear;

                if (oldParents)
                {
                    foreach (GedcomIndividual child in family.Children)
                    {
                        child.PresumedDeceased = true;
                    }
                }

            }
            //FindFamilies(false); // temp, for debugging
            DidEvaluateLivingStatus = true; // NB DO NOT return from this method early unless this is set
        }

        public string EarliestMarriage
        {
            get
            {
                string rv = null;
                foreach (GedcomFamily family in SafeFamilies)
                {
                    if (rv == null || (family.BestYear != null && String.Compare(family.DateMarried, rv, StringComparison.Ordinal) < 0))
                        rv = family.BestYear;
                }

                return rv;
            }
        }

        public string PersonNotes
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (Tag note in IndividualView.Notes)
                {
                    sb.Append(note.FullText(true));
                }

                return sb.ToString();
            }
        }

        public int? IntYearBorn => int.TryParse(YearBorn, out int y) && y > 0 ? y : (int?)null;
        public int? IntYearDied => int.TryParse(YearDied, out int y) && y > 0 ? y : (int?)null;

        public string GivenOrFullName
        {
            get
            {
                if (HasNoSurname)
                    return SafeNameForward;
                return SafeGivenName;
            }
        }

        public void Expand()
        {
            // if we already have families, stop
            if (Families != null)
                return;

            // find them and expand them (recursively)
            FindFamilies(false);
            foreach (GedcomFamily family in SafeFamilies)
            {
                family.Expand();
                if (ReportContext.Instance.Model.CheckCancel()) throw new G2RModel.Model.CanceledByUserException();
            }

            SortFamilies(); // this works now because we are doing depth-first
        }

        public void Ascend(bool oneStep = true)
        {
            ChildhoodFamily = ReportContext.Instance.Model.FindAsChildInFamily(this);
            if (oneStep) return;
            ChildhoodFamily?.Husband?.Ascend(oneStep);
            ChildhoodFamily?.Wife?.Ascend(oneStep);
        }

        public void FindFamilies(bool doParents, bool ascend = false)
        {
            Families = new List<GedcomFamily>();
            Spouses = new List<GedcomIndividual>();

            // find families
            var fs = (from view in ReportContext.Instance.Model.GedcomFile.FamilyViews
                where (IndividualView.Equals(view.Husband) || IndividualView.Equals(view.Wife))
                select view).ToList();
            foreach (FamilyView view in fs)
            {
                //GedcomFamily fam = GedcomFamily.AllFamilies.Find(f => f.FamilyView == view);
                GedcomFamily.GetFamMap().TryGetValue(view, out GedcomFamily fam);
                if (fam == null)
                {
                    GedcomFamily.Add(fam = new GedcomFamily(view));
                }
                Families.Add(fam);
                GedcomIndividual spouse = fam.SpouseOf(this);
                if (spouse != null)
                    Spouses.Add(spouse);
            }
            SortFamilies();

            ChildhoodFamily = ReportContext.Instance.Model.FindAsChildInFamily(this);
            if (doParents && ChildhoodFamily != null)
            {
                ChildhoodFamily.Husband?.FindFamilies(ascend);
                ChildhoodFamily.Wife?.FindFamilies(ascend);
            }
        }

        private string BuildReportableSpan()
        {
            if (YearBorn != "0000" && YearDied != "0000")
                return $" ({YearBorn} - {YearDied})";

            if (YearBorn != "0000")
                return $" (born {YearBorn})";

            if (YearDied != "0000")
                return $" (died {YearDied})";

            return "";
        }
        private int? CountChildren()
        {
            int rv = 0;
            foreach (GedcomFamily family in SafeFamilies)
            {
                rv += family.Children.Count;
            }

            return rv;
        }

        public void LocateCitations(CitationsMap cm)
        {
            //Citations = new GedcomCitationSet(this, cm);
            foreach (GedcomFamily family in SafeFamilies)
            {
                family.LocateCitations(cm);
            }
        }

        public Tag EventTag(TagCode tagcode)
        {
            // todo: use this function wherever applicable in this class
            return IndividualView?.IndiTag?.GetChild(tagcode);
        }

        public void SortFamilies()
        {
            // to get them in the right order:
            // if only one, nothing to do
            // if all have a "best year" sort the list by that
            // otherwise leave alone and hope the gedcom creator ordered them
            if (SafeFamilies.Count > 1)
            {
                bool canOrder = true;
                foreach (GedcomFamily family in SafeFamilies)
                {
                    canOrder &= family.BestYear != null;
                }

                if (canOrder)
                {
                    Families.Sort(new GedcomFamily.MarriageDateComparer());
                }

                FamiliesAreSorted = canOrder;
            }
        }
        public class BirthDateComparer : IComparer<GedcomIndividual>
        {
            #region Implementation of IComparer<in GedcomFamily>

            public int Compare(GedcomIndividual x, GedcomIndividual y)
            {
                return String.Compare((x?.YearBorn ?? ""), y?.YearBorn ?? "", StringComparison.Ordinal);
            }

            #endregion
        }

        public IEnumerable<GedcomFamily> MyParentsFamilies()
        {
            List<GedcomFamily> rvl = new List<GedcomFamily>();
            rvl.AddRange(ChildhoodFamily?.Husband?.Families ?? SafeEmptyFamilies);
            rvl.AddRange(ChildhoodFamily?.Wife?.Families ?? SafeEmptyFamilies);
            return rvl;
        }

        //public void SetContinuation(GedcomIndividual indi)
        //{
        //    (ContinuesWith ??= new List<GedcomIndividual>()).Add(indi);
        //}

        //public string GetContinuation(bool withGeneration)
        //{
        //    if ((ContinuesWith?.Count ?? 0) == 0) return null;

        //    StringBuilder sb = new StringBuilder();
        //    string sep = " ";
        //    sb.Append("(Continues with");
        //    foreach (GedcomIndividual indi in ContinuesWith)
        //    {
        //        sb.Append(sep).Append(indi.GetNumber(withGeneration));
        //        sep = ", ";
        //    }

        //    sb.Append(".)");

        //    return sb.ToString();
        //}
        public string PresentationName()
        {
            string rv = $"{Name} {ReportableSpan}";
            if (rv.Length < 3) return rv;
            if (!rv.StartsWith(", ")) return rv;
            return rv.Substring(2);
        }

        public int SpouseIndex(GedcomIndividual indi)
        {
            if ((Families?.Count ?? 0) == 0) return 0;
            GedcomFamily marriage = Families.Find(gf => gf.SpouseOf(this) == indi);
            return marriage == null ? 0 : Families.IndexOf(marriage) + 1;
        }
    }
}
