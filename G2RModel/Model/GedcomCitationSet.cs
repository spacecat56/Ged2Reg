using System.Collections.Generic;
using System.Text;
using SimpleGedcomLib;

namespace Ged2Reg.Model
{
    public class GedcomCitationSet
    {
        public Dictionary<string, List<CitationView>> MyCitations = new Dictionary<string, List<CitationView>>();
        //public GedcomCitationSet(GedcomIndividual indi, CitationsMap allCitationsMap)
        //{
        //    foreach (TagCode code in allCitationsMap.PersonEvents)
        //    {
        //        string evKey = TagMapper.Map(code).ToString();
        //        List<CitationView> cites = allCitationsMap.Find(indi.IndividualView.Id, code);
        //        if (cites == null) continue;
        //        MyCitations.Add(evKey, cites);
        //    }
        //}

        public GedcomCitationSet(GedcomFamily fam, CitationsMap allCitationsMap)
        {
            CitesForTag(fam, TagCode.MARR, allCitationsMap);
            CitesForTag(fam, TagCode.DIV, allCitationsMap);
        }

        private void CitesForTag(GedcomFamily fam, TagCode tag, CitationsMap allCitationsMap)
        {
            string evKey = TagMapper.Map(tag).ToString();
            List<CitationView> cites = allCitationsMap.Find(fam.FamilyView.Id, tag);
            if (cites == null) return;
            MyCitations.Add(evKey, cites);
        }

        //public string BestCitation(string evnt, bool appendSummary)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    CitationView best = pickBest(evnt);
        //    if (best == null) return null;

        //    string data = best.SourceTag.GetChild(TagCode.DATA)?.GetChild(TagCode.TEXT)?.FullText();
        //    string title = best.Source.GetChild(TagCode.TITL)?.FullText();
        //    string detail = best.Detail;

        //    return $"{data}, {title}; {detail}";
        //}

        private CitationView pickBest(string evnt)
        {
            if (!MyCitations.TryGetValue(evnt, out List<CitationView> cites)) 
                return null;
            if (cites.Count == 0)
                return null;
            return cites[0];
        }

        //public Tag MyTag { get; set; }
    }


    public class CitationsMap
    {
        public List<TagCode> PersonEvents = new List<TagCode>()
        {
            TagCode.BIRT, 
            TagCode.DEAT,
            TagCode.MARR,
            TagCode.BAPM,
            TagCode.CHR,
            TagCode.BURI,
        };
        private Dictionary<string, List<CitationView>> _personCitations = new Dictionary<string, List<CitationView>>();
        
        public CitationsMap(GedcomFile ged)
        {
            foreach (CitationView cv in ged.CitationViews)
            {
                Tag evTag = cv.SourceTag.ParentTag;
                if (!PersonEvents.Contains(evTag?.Code??TagCode.UNK))
                    continue;
                Tag entTag = evTag.ParentTag;
                string citeKey = $"{entTag.Id}:{evTag.TagText}";
                if (!_personCitations.TryGetValue(citeKey, out List<CitationView> eventCites))
                    _personCitations.Add(citeKey, eventCites = new List<CitationView>());
                eventCites.Add(cv);
            }
        }

        public List<CitationView> Find(string id, TagCode tc)
        {
            string citeKey = $"{id}:{tc}";
            _personCitations.TryGetValue(citeKey, out List<CitationView> rv);
            return rv;
        }
    }
}
