using System.Collections.Generic;
using Ged2Reg.Model;

namespace G2RModel.Model
{
    public class ReportEntryFactory
    {
        private static ReportEntryFactory _instance;

        public static ReportEntryFactory Init(bool unique = false)
        {
            return _instance = new ReportEntryFactory(unique);
        }

        public static ReportEntryFactory Instance => _instance;

        private ReportEntryFactory() { }

        private ReportEntryFactory(bool unique)
        {
            Unique = unique;
        }

        public ReportEntry GetReportEntry(GedcomIndividual indi)
        {
            if (!Unique)
                return new ReportEntry(indi);
            if (KnownEntries.TryGetValue(indi, out ReportEntry rv))
                return rv;
            rv = new ReportEntry(indi);
            KnownEntries.Add(indi, rv);
            return rv;
        }

        public ReportFamilyEntry GetReportFamily(GedcomFamily family)
        {
            if (family == null) return null;
            if (!Unique)
                return new ReportFamilyEntry(family);
            if (KnownFamilies.TryGetValue(family, out ReportFamilyEntry rv))
                return rv;
            rv = new ReportFamilyEntry(family);
            KnownFamilies.Add(family, rv);
            return rv;
        }

        private Dictionary<GedcomIndividual, ReportEntry> KnownEntries = new Dictionary<GedcomIndividual, ReportEntry>();
        private Dictionary<GedcomFamily, ReportFamilyEntry> KnownFamilies = new Dictionary<GedcomFamily, ReportFamilyEntry>();
        public bool Unique { get; }
    }
}