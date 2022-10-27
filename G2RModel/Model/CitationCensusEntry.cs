using SimpleGedcomLib;

namespace Ged2Reg.Model
{
    public class CitationCensusEntry
    {
        public TagCode TagCode { get; set; }
        public int Occurrences { get; set; }
        public int HaveCites { get; set; }
        public int HaveMultipleCites { get; set; }
        public int HaveSoleCite => HaveCites - HaveMultipleCites;
        public int Uncited => Occurrences - HaveCites;
        public int TotalCites { get; set; }
        public int CvSpread { get; set; }
        public int CitationSelected { get; set; }

        public void Count(EventCitations ec)
        {
            if (ec.TagCode != TagCode) return;
            Occurrences++;
            if (ec.DistinctCitations.Count > 0)
                HaveCites++;
            if (ec.DistinctCitations.Count > 1)
                HaveMultipleCites++;
            TotalCites += ec.DistinctCitations.Count;
            foreach (DistinctCitation dc in ec.DistinctCitations)
            {
                CvSpread += dc.CitationViews.Count;
            }

            if (ec.SelectedItem != null)
                CitationSelected++;
        }
    }
}