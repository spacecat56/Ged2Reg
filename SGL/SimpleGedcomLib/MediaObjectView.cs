using System;
using System.Collections.Generic;

namespace SimpleGedcomLib
{
    public class MediaObjectView
    {
        private Tag _objectTag;

        public Tag ObjectTag
        {
            get { return _objectTag; }
            set
            {
                if (value == null || value.Code != TagCode.OBJE || value.Level != 0)
                    throw new ArgumentException("not a level-0 Object tag");
                _objectTag = value;
            }
        }

        public List<CitationView> Citations = new List<CitationView>();
        public SourceView SourceView { get; set; }

        public string FilePath => _objectTag?.GetChild(TagCode.FILE)?.Content;
        public string FileDate => _objectTag?.GetChild(TagCode.FILE)?.GetChild(TagCode._DATE)?.Content;
        public string Caption => _objectTag?.GetChild(TagCode.FILE)?.GetChild(TagCode.TITL)?.FullText();
        public string Comment => _objectTag?.GetChild(TagCode.FILE)?.GetChild(TagCode.TEXT)?.FullText();
        public string Id => _objectTag?.Id;
        public bool IsPrivate => _objectTag.Has(TagCode._PRIV);

        public MediaObjectView(Tag t)
        {
            ObjectTag = t;
        }
    }
}