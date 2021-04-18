using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleGedcomLib;

namespace Ged2Reg.Model
{
    public class GedcomFamily
    {
        private static List<GedcomFamily> AllFamilies = new List<GedcomFamily>();
        private static Dictionary<FamilyView, GedcomFamily> _fvgiMap;

        public string DateMarried { get; set; }
        public string PlaceMarried { get; set; }
        public string MarriageDescription { get; set; }
        public string DateDivorced { get; set; }
        public string PlaceDivorced { get; set; }
        public string DivorceDescription { get; set; }

        private Tag _divorceTag;
        public bool Divorced => _divorceTag != null;

        public GedcomIndividual Husband { get; set; }
        public GedcomIndividual Wife { get; set; }
        public List<GedcomIndividual> Children { get; set; } = new List<GedcomIndividual>();
        public bool ChildrenAreSorted { get; set; }
        public FamilyView FamilyView { get; set; }
        private string _bestYear;

        public static void ClearFamilies()
        {
            AllFamilies.Clear();
            _fvgiMap?.Clear();
        }
        public static Dictionary<FamilyView, GedcomFamily> GetFamMap()
        {
            if (_fvgiMap != null) return _fvgiMap;

            _fvgiMap = AllFamilies.ToDictionary(f => f.FamilyView, f => f);

            return _fvgiMap;
        }
        public static GedcomFamily Add(GedcomFamily f)
        {
            AllFamilies.Add(f);
            GetFamMap().Add(f.FamilyView, f);
            return f;
        }

        public string BestYear
        {
            get => _bestYear ?? (_bestYear = FindBestYear());
            private set => _bestYear = value;
        }

        /// <summary>
        /// Find the marriage year, or if not known, the earliest year
        /// of child's birth.  This can be used to order an individuals marriages
        /// </summary>
        /// <returns></returns>
        private string FindBestYear()
        {
            string rv = _bestYear ?? GedDate.ExtractYear(DateMarried, defaultVal:null);
            if (rv != null) return rv;

            foreach (GedcomIndividual child in Children??new List<GedcomIndividual>())
            {
                string x = GedDate.ExtractYear(child.Born, defaultVal: null);
                if (x == null) 
                    continue;
                if (rv == null) 
                    rv = x;
                else if (String.Compare(rv, x, StringComparison.Ordinal) < 0)
                    rv = x;
            }

            if (rv != null) return rv;

            int.TryParse(GedDate.ExtractYear(Husband?.Born, defaultVal: "0"), out int dadBorn);
            int.TryParse(GedDate.ExtractYear(Wife?.Born, defaultVal: "0"), out int momBorn);
            int latest = Math.Max(dadBorn, momBorn);
            if (latest > 0)
            {
                rv = $"{(latest + 25):0000}";
                return rv;
            }

            return rv;
        }

        public GedcomFamily(FamilyView fv)
        {
            FamilyView = fv;
            Tag marrTag = fv.FamTag.GetChild(TagCode.MARR);
            DateMarried = marrTag?.GetChild(TagCode.DATE)?.Content;
            PlaceMarried = marrTag?.GetChild(TagCode.PLAC)?.Content;
            MarriageDescription = marrTag?.FullText();

            _divorceTag = fv.FamTag.GetChild(TagCode.DIV);
            DateDivorced = _divorceTag?.GetChild(TagCode.DATE)?.Content;
            PlaceDivorced = _divorceTag?.GetChild(TagCode.PLAC)?.Content;
            DivorceDescription = _divorceTag?.FullText();

            Husband = ReportContext.Instance.Model.FindIndividual(fv.Husband);
            Wife = ReportContext.Instance.Model.FindIndividual(fv.Wife);

            foreach (IndividualView view in fv.Chiildren)
            {
                Children.Add(ReportContext.Instance.Model.FindIndividual(view));
            }

            SortChildren();
        }

        public void SortChildren()
        {
            if ((Children?.Count ?? 0) > 1)
            {
                bool canOrder = true;
                foreach (GedcomIndividual child in Children)
                {
                    canOrder &= child.YearBorn != null;
                }

                if (canOrder)
                {
                    Children.Sort(new GedcomIndividual.BirthDateComparer());
                }

                ChildrenAreSorted = canOrder;
            }
        }

        public GedcomFamily Expand()
        {
            ReportContext context = ReportContext.Instance;
            
            foreach (GedcomIndividual child in Children)
            {
                child.Expand();
            }

            return this;
        }

        public bool CoupleAreBinomial()
        {
            bool notBinomial = Husband?.HasNoSurname ?? false;
            notBinomial |= Wife?.HasNoSurname ?? false;
            return !notBinomial;
        }

        public void Reset()
        {
            IsIncluded = false;
        }

        public class MarriageDateComparer : IComparer<GedcomFamily>
        {
            #region Implementation of IComparer<in GedcomFamily>

            public int Compare(GedcomFamily x, GedcomFamily y)
            {
                return String.Compare((x?.BestYear??""), y?.BestYear??"", StringComparison.Ordinal);
            }

            #endregion
        }

        public void LocateCitations(CitationsMap cm)
        {
            Citations = new GedcomCitationSet(this, cm);
        }
        public Tag EventTag(TagCode tagcode)
        {
            return FamilyView?.FamTag?.GetChild(tagcode);
        }

        public GedcomCitationSet Citations { get; set; } // todo: is this unused?

        public CitableMarriageEvents CitableEvents { get; set; }

        /// <summary>
        /// Set to indicate the family is chosen to list in ancestry report
        /// </summary>
        public bool IsIncluded { get; set; }

        public GedcomIndividual SpouseOf(GedcomIndividual indi)
        {
            return indi == Husband ? Wife : Husband;
        }
        private string WrapSafely(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            s = s.Trim();
            if (s.StartsWith('(') && s.EndsWith(')'))
                return s;
            return $"({s})";
        }

        public string ExtendedWifeName(string placeholder = "_____")
        {
            if (Wife?.Families == null)
            {
                return null;
            }

            // names predating the common use of the binomial are fraught
            // and not always clearly represented in GEDCOM data.
            // if the surname is the empty string, that came from // in the GEDCOM,
            // whcih is the correct way to show, no surname.  if we see that, we
            // are going to stop trying to assemble a conventional, Given (Maiden) Married 
            // name, and just use the woman's name.
            if (Wife.HasNoSurname)
                return Wife.SafeGivenName;

            StringBuilder sb = new StringBuilder();

            // start with given name and maiden name
            sb.Append(Wife.SafeGivenName);
            sb.Append(' ').Append(WrapSafely(Wife.SafeSurname));

            // add the surnames of all prior marriages
            int ix = Wife.Families.IndexOf(this);
            for (int i = 0; i < ix; i++)
            {
                sb.Append(' ').Append(WrapSafely(SafeName(Wife?.Families[i].Husband?.SafeSurname, placeholder)));
            }

            if (!Husband?.HasNoSurname??true) // ugh.  todo: true or false here
                sb.Append(' ').Append(SafeName(Husband?.SafeSurname, placeholder));

            return sb.ToString();
        }
        private string SafeName(string s, string placeholder)
        {
            return !string.IsNullOrEmpty(s) ? s : placeholder;
        }

    }
}
