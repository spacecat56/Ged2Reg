using SOL;
using WpdInterfaceLib;

namespace OdtAdapterLib
{
    public class OalIndexEntry : WpdIndexEntry
    {
        private bool _didBuild;
        public OdtIndexEntry IndexEntry { get; private set; } 
        public OalIndexEntry(IWpdDocument document) : base(document) { }

        #region Overrides of WpdFieldBase

        public override WpdFieldBase Build()
        {
            if (_didBuild) return this;
            _didBuild = true;
            IndexEntry = new OdtIndexEntry(){DelimitedText = ContentText};
            IndexEntry.Build();
            return this;
        }

        public override void Apply(IWpdParagraph p)
        {
            Build();
            (p as OalParagraph)?.Paragraph.Append(IndexEntry);
        }

        #endregion
    }
}
