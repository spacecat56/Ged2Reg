namespace WpdInterfaceLib
{
    public abstract class WpdFieldBase : IWpdFieldBase
    {
        public IWpdDocument Document { get; internal set; }

        protected WpdFieldBase(IWpdDocument document)
        {
            Document = document;
        }
        public string FieldText { get; set; }
        public string ContentText { get; set; }
        public abstract WpdFieldBase Build();
        public abstract void Apply(IWpdParagraph p);
    }

    public abstract class WpdNoteRefField : WpdFieldBase
    {
        public string MarkName { get; set; }
        public bool SameFormatting { get; set; }
        public bool InsertHyperlink { get; set; }
        public bool InsertRelativePosition { get; set; }



        protected WpdNoteRefField(IWpdDocument document) : base(document) { }

    }

    public abstract class WpdIndexEntry : WpdFieldBase
    {
        public string IndexValue { get; set; }
        public string IndexName { get; set; }
        public string SeeInstead { get; set; }


        protected WpdIndexEntry(IWpdDocument document) : base(document) { }
    }

    public abstract class WpdIndexField : WpdFieldBase
    {
        public static string UpdateFieldPrompt { get; set; } = "Right-click and Update (this) field to generate the index";
        public int Columns { get; set; }
        public string IndexName { get; set; }
        public string EntryPageSeparator { get; set; }

        protected WpdIndexField(IWpdDocument document) : base(document) { }
    }
}