using Ged2Reg.Model;

namespace G2RModel.Model
{
    public class ReportFamilyEntry
    {
        public ReportEntry Husband { get; set; }
        public ReportEntry Wife { get; set; }
        public ListOfReportEntry Children { get; set; }
        public GedcomFamily Family { get; set; }

        public bool IsIncluded { get; set; }

        internal ReportFamilyEntry(GedcomFamily gf)
        {
            Family = gf;
            gf.SortChildren();
            if (gf.Husband != null)
                Husband = ReportEntryFactory.Instance.GetReportEntry(gf.Husband);
            if (gf.Wife != null)
                Wife = ReportEntryFactory.Instance.GetReportEntry(gf.Wife);
            //Init(gf);
        }

        public ReportFamilyEntry Init()
        {
            Children = new ListOfReportEntry();
            foreach (GedcomIndividual child in Family.Children)
            {
                Children.Add(ReportEntryFactory.Instance.GetReportEntry(child));
            }

            return this;
        }
    }
}