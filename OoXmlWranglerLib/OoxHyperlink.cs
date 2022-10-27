using System;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace OoXmlWranglerLib
{
    public class OoxHyperlink
    {
        public enum LinkLocation
        {
            Main,
            Footnote,
            Endnote
        }
        public static string DefaultHyperlinkStyle { get; set; } = "Hyperlink";

        public string LinkStyle { get; set; }

        public Hyperlink MyHyperlink { get; set; }
        public string Anchor { get; set; }
        public string LinkText { get; set; }
        public Uri Uri { get; set; }
        public OoxDoc Doc { get; set; }
        public HyperlinkRelationship Rel { get; set; }
        public LinkLocation Location { get; set; }

        public OoxHyperlink Apply(OoxParagraph para )
        {

            Rel = Doc.AddHyperlinkRelationship(Uri, true, Location);
            MyHyperlink = new Hyperlink(){Anchor = Anchor, Id = Rel.Id};
            Run r = new Run(new Text(LinkText ?? Anchor))
            {
                RunProperties = new RunProperties(new RunStyle() {Val = LinkStyle ?? DefaultHyperlinkStyle})
            };
            MyHyperlink.AppendChild(r);
            para.Append(MyHyperlink);
            return this;
        }
    }
}
