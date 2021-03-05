using System;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocxAdapterLib
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
        public static bool UseDomainForNullLinkText { get; set; } = true;

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
            Run r = new Run(new Text(BestText()))
            {
                RunProperties = new RunProperties(new RunStyle() {Val = LinkStyle ?? DefaultHyperlinkStyle})
            };
            MyHyperlink.AppendChild(r);
            para.Append(MyHyperlink);
            return this;
        }

        internal string BestText()
        {
            return LinkText 
                   ?? ((UseDomainForNullLinkText) ? $"{Uri?.Domain()}" : Anchor)
                   ?? Anchor;
        }
    }

    public static class LocalExtensions
    {
        public static string Domain(this Uri uri)
        {
            if (uri == null) return null;
            string host = uri.Host;
            while (host.IndexOf('.') != host.LastIndexOf('.'))
                host = host.Substring(host.IndexOf('.')+1);
            if (host.Length < 2)
                return host;
            return $"{host.Substring(0, 1).ToUpper()}{host.Substring(1)}";
        }
    }
}
