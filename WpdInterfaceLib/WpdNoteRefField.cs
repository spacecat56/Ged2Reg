namespace WpdInterfaceLib
{
    public abstract class WpdNoteRefField : WpdFieldBase
    {
        public string MarkName { get; set; }
        public bool SameFormatting { get; set; }
        public bool InsertHyperlink { get; set; }
        public bool InsertRelativePosition { get; set; }



        protected WpdNoteRefField(IWpdDocument document) : base(document) { }

    }
}