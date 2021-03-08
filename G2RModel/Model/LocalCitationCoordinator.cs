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
            
            Add(new CitationProposal(){Citation = ec, EventId = ec.TagCode.ToString(), InstanceId = iid});
            return true;
        }

        private Dictionary<string, EventCitations> _m;
        private Dictionary<string, EventCitations> _map => _m ?? (_m = ToMap());

        public bool DoCite { get; set; }


        public EventCitations this[string s]
        {
            get
            {
                _map.TryGetValue(s, out EventCitations rv);
                return rv;
            }
        }

        public Dictionary<string, EventCitations> ToMap()
        {
            return this.ToDictionary(citationProposal => citationProposal.Id, citationProposal => citationProposal.Citation);
        }
    }
}