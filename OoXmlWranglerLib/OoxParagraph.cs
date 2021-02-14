using DocumentFormat.OpenXml.Wordprocessing;
using System;
using DocumentFormat.OpenXml;
using WpdInterfaceLib;

namespace DocxAdapterLib
{
    public class OoxParagraph : IWpdParagraph
    {
        public OoxParagraph(Paragraph myParagraph)
        {
            MyParagraph = myParagraph;
        }

        internal Paragraph MyParagraph { get; }

        private string _styleName;
        public string StyleName
        {
            get { return _styleName; }
            set
            {
                _styleName = value;
                ParagraphProperties pp = MyParagraph.ParagraphProperties ?? (MyParagraph.ParagraphProperties = new ParagraphProperties());
                pp.ParagraphStyleId = new ParagraphStyleId(){Val = _styleName};
                
            }
        }

        public IWpdParagraph Append(string text)
        {
            if (text == null)
                return this;
            MyParagraph.AppendChild(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
            return this;
        }

        public void AppendField(WpdFieldBase field)
        {
            field.Apply(this);
        }

        public void Append(string text, bool unk, Formatting formatting)
        {
            if (text == null)
                return ;
            MyParagraph.AppendChild(new Run(new Text(text){ Space = SpaceProcessingModeValues.Preserve }){RunProperties = OoxBuilders.BuildRunProperties(formatting)});
        }

        public void InsertHorizontalLine(string lineType, string position = "bottom")
        {
            BorderValues lt = BorderValues.Single;
            Enum.TryParse<BorderValues>(lineType, true, out lt);

            ParagraphBorders pb = new ParagraphBorders();
            BorderType border = "bottom".Equals(position) ? (BorderType) new BottomBorder() : new TopBorder();
            border.Color = "auto";
            border.Val = lt;
            border.Space = 1;
            border.Size = 6;

            ParagraphProperties pp = MyParagraph.ParagraphProperties ??=
                MyParagraph.ParagraphProperties = new ParagraphProperties();
            pp.AppendChild(border);
        }

        public void Append(Run run)
        {
            MyParagraph.AppendChild(run);
        }

        public void Append(Hyperlink h)
        {
            MyParagraph.AppendChild((Hyperlink)h);
        }
    }
}
