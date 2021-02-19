using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
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
            MyParagraph.AppendChild(new Run(Prepare(text)));
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
            MyParagraph.AppendChild(new Run(Prepare(text)){RunProperties = OoxBuilders.BuildRunProperties(formatting)});
        }

        internal OpenXmlElement[] Prepare(string s)
        {
            void AppendNonEmptyText(StringBuilder stringBuilder, List<OpenXmlElement> list)
            {
                if (stringBuilder.Length > 0)
                {
                    list.Add(new Text(stringBuilder.ToString()) {Space = SpaceProcessingModeValues.Preserve});
                    stringBuilder.Clear();
                }
            }

            List<OpenXmlElement> rvl = new List<OpenXmlElement>();
            //string[] texts = Regex.Split(s, @"(?<=[\t\n])");
            //string[] texts = s.Split(new[] {'\t', '\n'}, StringSplitOptions.None);
            //foreach (string text in texts)
            //{
            //    switch (text)
            //    {
            //        case "\t":
            //            rvl.Add(new TabChar());
            //            break;
            //        case "\n":
            //            rvl.Add(new Break());
            //            break;
            //        default:
            //            rvl.Add(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
            //            break;
            //    }
            //}
            StringBuilder sb = new StringBuilder();
            foreach (char c in s.ToCharArray())
            {
                switch (c)
                {
                    case '\t':
                        AppendNonEmptyText(sb, rvl);
                        rvl.Add(new TabChar());
                        break;
                    case '\n':
                        AppendNonEmptyText(sb, rvl);
                        rvl.Add(new Break());
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            AppendNonEmptyText(sb, rvl);
            return rvl.ToArray();
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
