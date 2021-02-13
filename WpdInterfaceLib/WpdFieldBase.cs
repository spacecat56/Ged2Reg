namespace WpdInterfaceLib
{
    public abstract class WpdFieldBase : IWpdFieldBase
    {
        protected WpdFieldBase(IWpdDocument document)  { }
        public string FieldText { get; set; }
        public string ContentText { get; set; }
        public abstract WpdFieldBase Build();
        public abstract void Apply(IWpdParagraph p);
    }
}