using System.Collections.Generic;
using System.Linq;
using SimpleGedcomLib;

namespace Ged2Reg.Model
{
    public class LocalCitationCoordinator : List<CitationProposal>
    {
        /// <summary>
        /// iff not null, this setting (true OR false) overrides the global setting
        /// in CitationCoordinator.
        /// </summary>
        public bool? OverrideDeferRepeats { get; set; }

        private Dictionary<string, CitationProposal> _m;
        private Dictionary<string, CitationProposal> _map => _m ??= ToMap();
        
        private HashSet<string> _uncited ;
        public HashSet<string> Uncited
        {
            get => _uncited ??= new HashSet<string>();
            set => _uncited = value;
        }

        public LocalCitationCoordinator() { } // added to easily find all constructions

        public bool AddNonNull(EventCitations ec, string iid = null)
        {
            if (ec == null)
                return false;
            
            Add(new CitationProposal(){Citation = ec, EventTagCode = ec.TagCode, EventId = ec.TagCode.ToString(), InstanceId = iid});
            return true;
        }

        public void NoteUncited(string key, TagCode eventCode)
        {
            Uncited.Add(BuildEventKey(key, eventCode));
        }

        private static string BuildEventKey(string key, TagCode eventCode)
        {
            return $"{key}:{eventCode}";
        }

        public bool IsUncited(string key, TagCode eventCode)
        {
            return Uncited.Contains(BuildEventKey(key, eventCode));
        }

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