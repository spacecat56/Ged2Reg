using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleGedcomLib
{
    public class SourceView
    {
        private Tag _sourceTag;

        public Tag SourceTag
        {
            get { return _sourceTag; }
            set
            {
                if (value == null || value.Code!=TagCode.SOUR || value.Level !=0)
                    throw new ArgumentException("not a level-0 Source");
                _sourceTag = value;
            }
        }

        public List<CitationView> Citations = new List<CitationView>();

        public string Author => _sourceTag.GetChild(TagCode.AUTH)?.FullText();
        public string Title => _sourceTag.GetChild(TagCode.TITL)?.FullText();
        public string Publisher => _sourceTag.GetChild(TagCode.PUBL)?.FullText();
        public string ReferenceNote => _sourceTag.GetChild(TagCode.NOTE)?.FullText();
        public string Repository => _sourceTag.GetChild(TagCode.REPO)?.ReferredTag?.GetChild(TagCode.NAME)?.Content;
        public string Id => _sourceTag.Id;

        public SourceView(Tag t)
        {
            SourceTag = t;
        }

        public int Link(List<MediaObjectView> movs)
        {
            int rv = 0;
            foreach (Tag tag in SourceTag.Children)
            {
                if (!tag.Is(TagCode.OBJE)) continue;
                MediaObjectView mov = movs.FirstOrDefault(x => x.Id.Equals(tag.ReferredTag.Id));
                if (mov==null) continue;
                mov.SourceView = this;
                rv++;
            }
            return rv;
        }
        
    }
}
