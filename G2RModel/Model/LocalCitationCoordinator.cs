using System.Collections.Generic;
using System.Linq;

namespace Ged2Reg.Model
{
    public class LocalCitationCoordinator : List<CitationProposal>
    {
        public bool AddNonNull(EventCitations ec, string iid = null)
        {
            if (ec == null)
                return false;
            
            Add(new CitationProposal(){Citation = ec, EventTagCode = ec.TagCode, EventId = ec.TagCode.ToString(), InstanceId = iid});
            return true;
        }

        private Dictionary<string, CitationProposal> _m;
        private Dictionary<string, CitationProposal> _map => _m ??= ToMap();

        public bool DoCite { get; set; }


        public CitationProposal this[string s]
        {
            get
            {
                _map.TryGetValue(s, out CitationProposal rv);
                return rv;
            }
        }

        public Dictionary<string, CitationProposal> ToMap()
        {
            return this.ToDictionary(citationProposal => citationProposal.Id, citationProposal => citationProposal);
        }
    }
}