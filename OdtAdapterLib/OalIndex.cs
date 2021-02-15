using SOL;
using WpdInterfaceLib;

namespace OdtAdapterLib
{
    class OalIndex : WpdIndexField
    {
        private bool _didBuild;
        public OdtIndex Index { get; private set; }

        public OalIndex(IWpdDocument document) : base(document)
        {
            SingleIndexOnly = true;
        }

        #region Overrides of WpdFieldBase

        public override WpdFieldBase Build()
        {
            if (_didBuild) return this;
            _didBuild = true;
            Index = new OdtIndex(){Columns = Columns, Heading = Heading, Placeholder = UpdateFieldPrompt};
            Index.Build();
            return this;
        }

        public override void Apply(IWpdParagraph p)
        {
            Build();
            (p as OalParagraph)?.Paragraph.Document.Append(Index);
        }

        #endregion
    }
}
