namespace Ged2Reg.Model
{
    public class CitationProposal
    {
        public string InstanceId { get; set; }
        public EventCitations Citation { get; set; }
        public string EventId { get; set; }
        public string Id => $"{EventId}{InstanceId}";

        public bool Matches(CitationProposal other)
        {
            return Citation?.Matches(other.Citation) ?? false;
        }
    }
}