using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace OoXmlWranglerLib

{
    public abstract class AbstractField
    {
        public string FieldText { get; set; }

        public string ContentText { get; set; }
 
        public abstract AbstractField Build();

        protected AbstractField(OoxDoc document)  { }

        public void Apply(OoxParagraph p)
        {
            Build();
            FieldCode fc = new FieldCode(FieldText){Space = SpaceProcessingModeValues.Preserve};

            p.Append(new Run(new FieldChar() {FieldCharType = FieldCharValues.Begin}));
            p.Append(new Run(fc));
            p.Append(new Run(new FieldChar() { FieldCharType = FieldCharValues.Separate }));
            if (!string.IsNullOrEmpty(ContentText))
                p.Append(new Run(new Text(ContentText)));
            p.Append(new Run(new FieldChar() { FieldCharType = FieldCharValues.End }));
        }
    }
}
