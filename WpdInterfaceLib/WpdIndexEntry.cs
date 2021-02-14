namespace WpdInterfaceLib
{
    public abstract class WpdIndexEntry : WpdFieldBase
    {
        public string IndexValue { get; set; }
        public string IndexName { get; set; }
        public string SeeInstead { get; set; }


        protected WpdIndexEntry(IWpdDocument document) : base(document) { }
    }
}