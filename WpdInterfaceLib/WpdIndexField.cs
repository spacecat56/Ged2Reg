namespace WpdInterfaceLib
{
    public abstract class WpdIndexField : WpdFieldBase
    {
        public static string UpdateFieldPrompt { get; set; } = "Right-click and Update (this) field to generate the index";
        public int Columns { get; set; }
        public string IndexName { get; set; }
        public string EntryPageSeparator { get; set; }
        public string Heading { get; set; } = "Index";
        public bool SingleIndexOnly { get; set; }

        protected WpdIndexField(IWpdDocument document) : base(document) { }
    }
}