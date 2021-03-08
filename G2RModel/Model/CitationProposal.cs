using System.Collections.Generic;
using System.Text;
using SimpleGedcomLib;

namespace Ged2Reg.Model
{
    public class CitationProposal
    {
        public string InstanceId { get; set; }
        public EventCitations Citation { get; set; }
        public string EventId { get; set; }
        public TagCode EventTagCode { get; set; }
        public string Id => $"{EventId}{InstanceId}";

        public List<CitationProposal> ApplicableEvents { get; set; }

        public bool Matches(CitationProposal other)
        {
            return Citation?.Matches(other.Citation) ?? false;
        }

        public string AppliesTo(bool onlyIfMultiple = true)
        {
            if (onlyIfMultiple && (ApplicableEvents?.Count ?? 0) == 0)
                return null;
            StringBuilder sb = new StringBuilder();
            sb.Append("Cited for: ");
            string sep = null;
            foreach (CitationProposal other in ApplicableEvents)
            {
                sb.Append(sep).Append(other.EventTagCode.Map().ToString());
                sep = ", ";
            }
            sb.Append(sep).Append(EventTagCode.Map().ToString()).Append(".");
            return sb.ToString();
        }

        public void AddApplicableEvents(CitationProposal other)
        {
            if (ApplicableEvents == null)
            {
                ApplicableEvents = new List<CitationProposal>();
            }
            if (other.ApplicableEvents != null)
                ApplicableEvents.AddRange(other.ApplicableEvents);
            ApplicableEvents.Add(other);
        }
    }
}