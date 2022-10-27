using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;

namespace OoXmlWranglerLib
{
    public class OoxParagraph
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

        public OoxParagraph Append(string text)
        {
            if (text == null)
                return this;
            MyParagraph.AppendChild(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
            return this;
        }

        public void AppendField(AbstractField field)
        {
            field.Apply(this);
        }

        public void Append(string text, bool unk, Formatting formatting)
        {
            if (text == null)
                return ;
            MyParagraph.AppendChild(new Run(new Text(text){ Space = SpaceProcessingModeValues.Preserve }){RunProperties = Builders.BuildRunProperties(formatting)});
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

        //internal int Append(OoxFootnote ooxFootnote, bool bookmarked)
        //{
        //    //throw new NotImplementedException();
        //    return -2;
        //}

        public void Append(Run run)
        {
            MyParagraph.AppendChild(run);
        }

        public void Append(Hyperlink h)
        {
            MyParagraph.AppendChild(h);
        }
    }
}
