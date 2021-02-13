using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace WpdInterfaceLib

{
    public abstract class OoxFieldBase : WpdFieldBase
    {
        protected OoxFieldBase(IWpdDocument document) : base(document) { }

        public override void Apply(IWpdParagraph p)
        {
            Build();
            FieldCode fc = new FieldCode(FieldText){Space = SpaceProcessingModeValues.Preserve};

            ((OoxParagraph)p).Append(new Run(new FieldChar() {FieldCharType = FieldCharValues.Begin}));
            ((OoxParagraph)p).Append(new Run(fc));
            ((OoxParagraph)p).Append(new Run(new FieldChar() { FieldCharType = FieldCharValues.Separate }));
            if (!string.IsNullOrEmpty(ContentText))
                ((OoxParagraph)p).Append(new Run(new Text(ContentText)));
            ((OoxParagraph)p).Append(new Run(new FieldChar() { FieldCharType = FieldCharValues.End }));
        }
    }
}
