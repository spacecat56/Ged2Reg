using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleGedcomLib
{
    /// <summary>
    /// One of these is constructed for each SOUR tag that is used as a 
    /// citation.  Thus each may have at most one Person? except in the case 
    /// </summary>
    public class CitationView
    {
        private Tag _sourceTag;

        public Tag SourceTag
        {
            get { return _sourceTag; }
            set
            {
                if (value == null || value.Code != TagCode.SOUR || value.Level < 1)
                    throw new ArgumentException("not a level-1+ Source (citation)");
                _sourceTag = value;
            }
        }

        public SourceView TheSourceView { get; set; }
        public List<MediaObjectView> MediaViews { get; set; } = new List<MediaObjectView>();
        //public List<IndividualView> CitedIndividuals { get; set; } = new List<IndividualView>();
        //public List<EventView> CitedEvents { get; set; } = new List<EventView>();

        // trying to accomodate remapped tags that may be at varying depth below the 
        // citation SOUR tag, e.g. Roots Magic with its _WEBTAG / URL structure
        public string URL => _sourceTag?.GetFirstDescendant(TagCode._LINK)?.Content;
        public Tag Source => _sourceTag?.ReferredTag;
        public string Text => _sourceTag.GetChild(TagCode.DATA)?.FullText();
        public string Detail => _sourceTag?.GetChild(TagCode.PAGE)?.FullText();
        public string SourceId => _sourceTag.ReferredTag?.Id;
        public string FullText => $"{Text}; {Detail}";

        public CitationView(Tag t)
        {
            SourceTag = t;
        }

        public bool Link(List<SourceView> svs)
        {
            TheSourceView = svs.FirstOrDefault(x => x.SourceTag == SourceTag.ReferredTag);
            TheSourceView?.Citations.Add(this);
            return TheSourceView != null;
        }

        public int Link(List<MediaObjectView> obs)
        {
            int rv = 0;
            List<Tag> obtags = SourceTag.GetChildren(TagCode.OBJE) ?? new List<Tag>();
            foreach (Tag obtag in obtags)
            {
                MediaObjectView mov = obs.FirstOrDefault(x => x.ObjectTag == obtag.ReferredTag);
                if (mov==null) continue;
                rv++;
                MediaViews.Add(mov);
                mov.Citations.Add(this);
            }
            return rv;
        }

        //public int Link(List<IndividualView> individualViews)
        //{
        //    Tag t = _sourceTag.GetAncestor(TagCode.INDI);
        //    if (string.IsNullOrEmpty(t?.Id))
        //        return 0;
        //    // todo
        //    return 0;
        //}
    }
}