namespace WpdInterfaceLib
{
    public abstract class WpdFieldBase
    {
        public IWpdDocument Document { get; internal set; }

        protected WpdFieldBase(IWpdDocument document)
        {
            Document = document;
        }
        public string FieldText { get; set; }
        public string ContentText { get; set; }
        public string ContentStyleName { get; set; }
        public abstract WpdFieldBase Build();
        public abstract void Apply(IWpdParagraph p);
    }
}