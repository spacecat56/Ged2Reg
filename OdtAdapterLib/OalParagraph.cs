using SOL;
using WpdInterfaceLib;

namespace OdtAdapterLib
{
    public class OalParagraph : IWpdParagraph
    {
        public OdtParagraph Paragraph { get; internal set; }

        #region Implementation of IWpdParagraph

        public IWpdParagraph Append(string text)
        {
            Paragraph.Append(text);
            return this;
        }

        public void AppendField(WpdFieldBase field)
        {
            field.Apply(this);
        }

        public void Append(string text, bool unk, Formatting formatting)
        { // todo: bold, italic as direct-applied stylings
            Paragraph.Append(text, formatting.CharacterStyleName);
        }
        /// <summary>
        /// There is a mismatch between docx and odt, in the way these "lines" are defined.
        /// In docx it is a border applied by paragraph properties, but in odt it is
        /// done using attributes in the paragraph style.  
        /// </summary>
        /// <param name="lineType"></param>
        /// <param name="position"></param>
        public void InsertHorizontalLine(string lineType, string position = "bottom")
        { // todo: honor position; try nesting to get the same effect as on docx
            OdtDocument.LineStyle ls = ("single".Equals(lineType))
                ? OdtDocument.LineStyle.Single
                : OdtDocument.LineStyle.Double;
            Paragraph.Document.AppendHorizontalLine(ls, position, Paragraph);
        }

        public string StyleName
        {
            get => Paragraph?.Style;
            set => Paragraph.Style = value;
        }

        #endregion
    }
}
