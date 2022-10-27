using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleGedcomLib;

namespace Ged2Reg.Model
{
    public class GedcomFamily
    {
        public static List<GedcomFamily> AllFamilies = new List<GedcomFamily>();

        public string DateMarried { get; set; }
        public string PlaceMarried { get; set; }
        public string MarriageDescription { get; set; }

        public GedcomIndividual Husband { get; set; }
        public GedcomIndividual Wife { get; set; }
        public List<GedcomIndividual> Children { get; set; } = new List<GedcomIndividual>();
        public bool ChildrenAreSorted { get; set; }
        public FamilyView FamilyView { get; set; }
        private string _bestYear;

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
            if (rv != null || Children == null) return rv;

            foreach (GedcomIndividual child in Children)
            {
                string x = GedDate.ExtractYear(child.Born, defaultVal: null);
                if (x == null) 
                    continue;
                if (rv == null) 
                    rv = x;
                else if (String.Compare(rv, x, StringComparison.Ordinal) < 0)
                    rv = x;
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

        public GedcomIndividual SpouseOf(GedcomIndividual indi)
        {
            return indi == Husband ? Wife : Husband;
        }
    }
}
