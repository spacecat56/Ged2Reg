using WpdInterfaceLib;

namespace DocxAdapterLib
{
    public class OoxIndexEntry : WpdIndexEntry
    {
        public OoxIndexEntry(IWpdDocument document) : base(document) { }

        #region Overrides of AbstractField

        public override WpdFieldBase Build()
        {
            // build the contents of the field
            string fieldContents = $" XE \"{IndexValue}\" ";
            if (SeeInstead != null)
                fieldContents = $"{fieldContents}\\t \"See {SeeInstead}\" ";
            if (IndexName != null)
                fieldContents = $"{fieldContents}\\f \"{IndexName}\" ";

            // wrap it in the field delimiters
            //Xml = Build(fieldContents);
            FieldText = fieldContents;
            return this;
        }

        public override void Apply(IWpdParagraph p)
        {
            Build();
            FieldHelper.ApplyField(p as OoxParagraph, this);
        }

        #endregion
    }
}
