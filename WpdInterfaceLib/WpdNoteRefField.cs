namespace WpdInterfaceLib
{
    public abstract class WpdNoteRefField : WpdFieldBase
    {
        public string MarkName { get; set; }
        public bool SameFormatting { get; set; }
        public bool InsertHyperlink { get; set; }
        public bool InsertRelativePosition { get; set; }
        public WpdFootnoteBase Footnote { get; set; }


        protected WpdNoteRefField(IWpdDocument document, WpdFootnoteBase fn) : base(document)
        {
            Footnote = fn;
        }

    }
}