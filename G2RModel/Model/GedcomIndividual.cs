using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using SimpleGedcomLib;

namespace Ged2Reg.Model
{

    public class GedcomIndividual
    {
        public GedcomIndividual() { }
        private static G2RSettings _settings;
        internal static G2RSettings Settings => _settings ?? (_settings = ReportContext.Instance.Settings);

        public string Name => $"{Surname}, {GivenName}";
        public string NameForward => $"{GivenName} {Surname}";
        public string GivenName => IsUnknown(IndividualView?.GivenName)
            ? Settings.UnknownInReport
            : $"{IndividualView?.GivenName}";

        public string Surname => IsUnknown(IndividualView?.Surname) 
            ? Settings.UnknownInReport 
            : $"{IndividualView?.Surname}";

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

        public string SafeName => IsUnknown(IndividualView?.GivenName, IndividualView?.Surname)
            ? Settings.UnknownInReport 
            : $"{SafeSurname}, {SafeGivenName}";

        public string SafeNameForward => IsUnknown(IndividualView?.GivenName, IndividualView?.Surname)
            ? Settings.UnknownInReport
            : $"{SafeGivenName} {SafeSurname}";

        public string SafeGivenName => NotLiving ? $"{GivenName}" : "(Living)";
        public string SafeSurname => $"{Surname}"; // this usage is ambiguous

        //public bool MayBeLiving { get; set; }
        public bool NotLiving { get; set; }
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
        public string BaptizedDescription { get; set; }

        #region transients
        public BigInteger AssignedMainNumber { get; set; }
        public int AssignedChildNumber { get; set; }
        public AncestryNameList Ancestry { get; set; }

        // these are used to control output positioning 
        // on the Ancestors report
        public bool SuppressSpouseInfo { get; set; }
        public bool EmitChildrenAfter { get; set; }

        // this is used on ancestry report to implement
        // options and repositioning of list(s) of children
        public List<GedcomFamily> FamiliesToReport { get; set; }
        // this is used where we stop exploding to avoid repetition /
        // recursion, to reference the number of the repeated ancestor
        public List<GedcomIndividual> ContinuesWith { get; set; }
        #endregion

        public string Pronoun => "M".Equals(Gender) ? "He" : "She";
        public string AntiPronoun => "M".Equals(Gender) ? "She" : "He";

        public string NounAsChild => "M".Equals(Gender) ? "Son" : "Daughter";

        public string PronounPossessive => "M".Equals(Gender) ? "His" : "Her";

        public string Gender => IndividualView?.IndiTag?.GetChild(TagCode.SEX)?.Content ?? "U";

        public string ChildNumberRoman => AssignedChildNumber.ToRoman();
        public bool ChildEntryEmitted { get; set; }

        private int? _numberOfChildren;
        private string _reportableSpan;
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

        public bool FamiliesAreSorted { get; set; }

        public List<GedcomIndividual> Spouses { get; set; }

        public GedcomFamily ChildhoodFamily { get; set; }

        public CitablePersonEvents CitableEvents { get; set; }

        public int GenerationInCurrentReport { get; set; }

        public string GetNumber(bool withGeneration) => withGeneration 
            ? $"{GenerationInCurrentReport:00}-{AssignedMainNumber}" 
            : $"{AssignedMainNumber}";

        private const string NullSpan = "0000 - 0000";

        private IndividualView _individualView;

        public void Reset()
        {
            foreach (GedcomFamily family in SafeFamilies)
            {
                family.Reset();
            }

            ContinuesWith = null;
            FamiliesToReport = null;
            Families = null;
            Spouses = null;
            AssignedChildNumber = 0;
            AssignedMainNumber = 0;
            GenerationInCurrentReport = 0;
            ChildEntryEmitted = false;
            SortFamilies();
        }

        public IndividualView IndividualView
        {
            get => _individualView;
            set
            {
                _individualView = value;
                if (_individualView == null) return;
                
                Tag tag = _individualView.IndiTag.GetChild(TagCode.BIRT);
                Born = tag?.GetChild(TagCode.DATE)?.Content;
                PlaceBorn = tag?.GetChild(TagCode.PLAC)?.Content;
                BirthDescription = tag?.FullText();
                
                Tag tagD = _individualView.IndiTag.GetChild(TagCode.DEAT);
                Died = tagD?.GetChild(TagCode.DATE)?.Content;
                PlaceDied = tagD?.GetChild(TagCode.PLAC)?.Content;
                DeathDescription = tagD?.FullText();
                
                ActualYearBorn = GedDate.ExtractYear(Born, defaultVal: null);
                YearBorn = ActualYearBorn ?? "0000";
                //YearBorn = GedDate.ExtractYear(Born, defaultVal: "0000");
                YearDied = GedDate.ExtractYear(Died, defaultVal: "0000");
                LifeSpan = $"{YearBorn} - {YearDied}";
                if (NullSpan.Equals(LifeSpan)) LifeSpan = "";

                tag = _individualView.IndiTag.GetChild(TagCode.BURI);
                Buried = tag?.GetChild(TagCode.DATE)?.Content;
                PlaceBuried = tag?.GetChild(TagCode.PLAC)?.Content;
                BurialDescription = tag?.FullText();

                tag = _individualView.IndiTag.GetChild(TagCode.BAPM) ?? _individualView.IndiTag.GetChild(TagCode.CHR);
                Baptized = tag?.GetChild(TagCode.DATE)?.Content;
                PlaceBaptized = tag?.GetChild(TagCode.PLAC)?.Content;
                BaptizedDescription = tag?.FullText();
            }
        }

        public void EvalLivingStatus()
        {
            if (DidEvaluateLivingStatus)
                return;

            // decide if the person is, or may possibly be, still living
            if (_individualView.IndiTag.GetChild(TagCode.DEAT) != null)
            {
                // take this to mean, some basis to suppose not living
                NotLiving = true;
            }

            int boundsYear = ReportContext.Instance.Settings.PresumedLivingBoundaryYear;

            // born 100+ years ago, assume not
            int iBorn = GenealogicalDateFormatter.ParseYear(YearBorn);
            //int.TryParse(YearBorn, out int iBorn);
            if (iBorn > 0 && iBorn < boundsYear)
            {
                NotLiving = true;
            }

            // now we can apply further rules for inferring a person not living
            // we have the indi in relation to spouse and children, as well as the 
            // children of the indi to consider
            foreach (GedcomFamily family in SafeFamilies)
            {
                int yMarr = GenealogicalDateFormatter.ParseYear(family.BestYear);
                GedcomIndividual spouse = family.SpouseOf(this);
                // too late! Spouses.Add(spouse);

                int ySpouseBirt = GenealogicalDateFormatter.ParseYear(spouse?.YearBorn ?? "");
                int ySpouseDeat = GenealogicalDateFormatter.ParseYear(spouse?.YearDied ?? "");

                if (!NotLiving)
                {
                    // married over 75 years ago, assume not 
                    NotLiving |= yMarr < boundsYear + 25;

                    // spouse born 100 years ago, assume not
                    NotLiving |= ySpouseBirt < boundsYear;

                    // spouse died 75 years ago, assume not
                    NotLiving |= ySpouseDeat < boundsYear + 25;
                }

                if (!NotLiving)
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
                        NotLiving = true;
                        break;
                    }
                }

                // todo: reconsider this
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
                        child.NotLiving = true;
                    }
                }

            }
            FindFamilies(false); // temp, for debugging
            DidEvaluateLivingStatus = true; // NB DO NOT return from this method early
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

        public void SetContinuation(GedcomIndividual indi)
        {
            (ContinuesWith ??= new List<GedcomIndividual>()).Add(indi);
        }

        public string GetContinuation(bool withGeneration)
        {
            if ((ContinuesWith?.Count ?? 0) == 0) return null;

            StringBuilder sb = new StringBuilder();
            string sep = " ";
            sb.Append("(Continues with");
            foreach (GedcomIndividual indi in ContinuesWith)
            {
                sb.Append(sep).Append(indi.GetNumber(withGeneration));
                sep = ", ";
            }

            sb.Append(".)");

            return sb.ToString();
        }
    }
}
