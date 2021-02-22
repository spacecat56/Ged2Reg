using System.Collections.Generic;
using SOL;
using WpdInterfaceLib;

namespace OdtAdapterLib
{
    public class OalParagraph : IWpdParagraph
    {
        public OdtParagraph Paragraph { get; internal set; }

        private static Formatting _emptyFormatting = new Formatting();
        #region Implementation of IWpdParagraph

        public IWpdParagraph Append(string text)
        {
            Append(text, false, _emptyFormatting);
            return this;
        }

        public void AppendField(WpdFieldBase field)
        {
            field.Apply(this);
        }

        public void Append(string text, bool unk, Formatting formatting)
        { // todo: bold, italic as direct-applied stylings
            List<TaggedText> runs = WpdTextHelper.Split(text);

            foreach (TaggedText run in runs)
            {
                Paragraph.Append(run.Text, formatting.CharacterStyleName,
                    (formatting.Bold??false)||run.IsBold, (formatting.Italic??false)||run.IsItalic);
            }

            //Paragraph.Append(text, formatting.CharacterStyleName, formatting.Bold, formatting.Italic);
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
