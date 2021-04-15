using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ged2Reg.Model;

namespace G2RModel.Model
{
    public class ReportFamilyEntry : ReportEntryBase
    {
        #region Overrides of ReportEntryBase
        public override string Who => $"{Husband?.Who}, {Wife?.Who}";
        #endregion

        public ReportEntry Husband { get; set; }
        public ReportEntry Wife { get; set; }
        public ListOfReportEntry Children { get; set; }
        public GedcomFamily Family { get; set; }
        public ReportEntry[] Couple => new[] {Husband, Wife};

        public string ExtendedWifeName(string placeholder = "_____")
        {
            if (Wife == null)
            {
                return null;
            }
            
            StringBuilder sb = new StringBuilder();

            // start with given name 
            sb.Append(Wife.Individual.SafeGivenName);

            // extend with names up to this marriage
            if (!Wife.Individual.HasNoSurname)
                sb.Append(' ').Append(ExtendedWifeSurname(placeholder));

            //sb.Append(" (").Append(Wife.Individual.SafeSurname).Append(')');

            //// add the surnames of all prior marriages
            //int ix = Wife.Families.IndexOf(Family);
            //for (int i = 0; i < ix; i++)
            //{
            //    sb.Append(" (").Append(SafeSurname(Wife?.Individual?.Families[i].Husband?.SafeSurname, placeholder)).Append(')');
            //}

            //sb.Append(' ').Append(SafeSurname(Husband?.Individual?.SafeSurname, placeholder));

            return sb.ToString();
        }
        public string IndexableExtendedWifeForeName
        {
            get
            {
                if (Wife == null)
                {
                    return null;
                }

                // are we in the era of non-binary names? or, husband unknown?
                if (string.IsNullOrEmpty(Husband?.Individual?.Surname))
                    return null;

                if (Wife.NameIsUnknown())
                    return null;

                StringBuilder sb = new StringBuilder();

                // start with the given name and add the maiden name and all PRIOR marriages
                sb.Append(Wife.Individual.SafeGivenName).Append(' ')
                    .Append(ExtendedWifeSurname(includeHusbandSurname: false));

                return sb.ToString();
            }
        }

        public string ExtendedWifeSurname(string placeholder = "_____", bool includeHusbandSurname = true)
        {
            if (Wife == null)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();

            // start with the maiden name
            sb.Append("(").Append(Wife.Individual.SafeSurname).Append(')');

            // add the surnames of all prior marriages
            int ix = Wife.Families.IndexOf(Family);
            for (int i = 0; i < ix; i++)
            {
                sb.Append(" (").Append(SafeSurname(Wife?.Individual?.Families[i].Husband?.SafeSurname, placeholder)).Append(')');
            }

            if (includeHusbandSurname)
            {
                // add the husband's surname, as an assumption!  user beware of modern couples
                sb.Append(' ').Append(SafeSurname(Husband?.Individual?.SafeSurname, placeholder));
            }

            return sb.ToString();
        }

        private string SafeSurname(string s, string placeholder)
        {
            return !string.IsNullOrEmpty(s) ? s : placeholder;
        }

        public bool IsIncluded { get; set; }

        public bool CoupleAreBinomial => Family?.CoupleAreBinomial() ?? false;

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

        public ReportFamilyEntry Init(ReportEntry linked = null)
        {
            if (Children != null)
            {
                return this;
            }
            Children = new ListOfReportEntry();
            foreach (GedcomIndividual child in Family.Children)
            {
                if (linked?.Individual == child)
                    // this is essential for correct ancestry numbering (of children)
                    Children.Add(linked); 
                else
                    Children.Add(ReportEntryFactory.Instance.GetReportEntry(child));
            }

            return this;
        }

        public List<ReportEntry> FindMainNumberedChildren()
        {
            if (Children == null)
                return new List<ReportEntry>();
            List<ReportEntry> rvl = Children.Where(c => c.AssignedMainNumber > 0).ToList();
            return rvl;
        }
        public IEnumerable<ReportEntry> GatherVisiblePersons()
        {
            List<ReportEntry> rvl = new List<ReportEntry> {Husband, Wife};
            if (Children == null)
                return rvl;
            rvl.AddRange(Children);

            // we also need to locate spouses of children,
            // and their parents.  We are NOT calling
            // Gather() methods because we do not 
            // want to recurse through the whole file
            foreach (ReportEntry child in Children)
            {
                if (child.Individual.ChildhoodFamily == null)
                    child.Individual.FindFamilies(false);
                foreach (GedcomFamily family in child.Individual.Families)
                {
                    var spouse = family.SpouseOf(child.Individual);
                    if (spouse == null)
                        continue;
                    rvl.Add(ReportEntryFactory.Instance.GetReportEntry(spouse));
                    
                    // parents of child's spouse
                    rvl.AddRange(ReportEntryFactory.Instance.GetParents(spouse));
                }
            }
            return rvl;
        }

        public static void GatherParents(GedcomIndividual indi, List<ReportEntry> list)
        {
            if (indi.ChildhoodFamily == null)
                indi.FindFamilies(false);
            if (indi.ChildhoodFamily == null)
                return;
            if (indi.ChildhoodFamily.Husband != null)
                list.Add(ReportEntryFactory.Instance.GetReportEntry(indi.ChildhoodFamily.Husband));
            if (indi.ChildhoodFamily.Wife != null)
                list.Add(ReportEntryFactory.Instance.GetReportEntry(indi.ChildhoodFamily.Wife));
        }
    }
}