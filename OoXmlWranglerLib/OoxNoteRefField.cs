using System.Text;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using WpdInterfaceLib;

namespace DocxAdapterLib
{
    public class OoxNoteRefField : WpdNoteRefField
    {
  
        #region Overrides of AbstractField

        public override WpdFieldBase Build()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" NOTEREF ").Append(MarkName).Append(' ');
            if (SameFormatting)
                sb.Append("\\f ");
            if (InsertRelativePosition)
                sb.Append("\\p ");
            if (InsertHyperlink)
                sb.Append("\\h ");

            FieldText = sb.ToString();
            return this;
        }

        public override void Apply(IWpdParagraph p)
        {
            Build();

            FieldHelper.ApplyField(p as OoxParagraph, this);
        }

        internal void ApplyField(OoxParagraph oxpara)
        {
            FieldCode fc = new FieldCode(FieldText) {Space = SpaceProcessingModeValues.Preserve};
            oxpara.Append(new Run(new FieldChar() {FieldCharType = FieldCharValues.Begin}));
            oxpara.Append(new Run(fc));
            oxpara.Append(new Run(new FieldChar() {FieldCharType = FieldCharValues.Separate}));
            if (!string.IsNullOrEmpty(ContentText))
                oxpara.Append(new Run(new Text(ContentText)));
            oxpara.Append(new Run(new FieldChar() {FieldCharType = FieldCharValues.End}));
        }

        #endregion

        public OoxNoteRefField(OoxDoc document, XElement xml = null) : base(document, null) { }
    }

    internal class FieldHelper
    {
        internal static void ApplyField(OoxParagraph oxpara, WpdFieldBase f)
        {
            FieldCode fc = new FieldCode(f.FieldText) { Space = SpaceProcessingModeValues.Preserve };
            oxpara.Append(new Run(new FieldChar() { FieldCharType = FieldCharValues.Begin }));
            oxpara.Append(new Run(fc));
            oxpara.Append(new Run(new FieldChar() { FieldCharType = FieldCharValues.Separate }));
            if (!string.IsNullOrEmpty(f.ContentText))
                oxpara.Append(new Run(new Text(f.ContentText)));
            oxpara.Append(new Run(new FieldChar() { FieldCharType = FieldCharValues.End }));
        }

    }
}
