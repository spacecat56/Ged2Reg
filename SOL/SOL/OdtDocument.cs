using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace SOL
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class OdtDocument
    {
        #nullable enable
        public static string? office = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
        public static string? text = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";
        public static string? style = "urn:oasis:names:tc:opendocument:xmlns:style:1.0";

#nullable restore

        public static void ResetContext()
        {
            OdtFootnote.Reset();
        }

        public string PageBreakStyleName { get; set; } = "PageBreakStyle";
        //public string SingleLineStyleName { get; set; } = "SOL_Horizontal_Line_Single";
        //public string DoubleLineStyleName { get; set; } = "SOL_Horizontal_Line_Double";

        public List<OdtPart> Parts { get; set; } = new List<OdtPart>();

        public StylesPart TheStylesPart { get; set; }
        public ContentPart TheContentPart { get; set; }

        public MetaPart TheMetaPart { get; set; }

        public static OdtDocument New(Stream stream = null)
        {
            if (stream == null)
            {
                //string[] resNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                //string rn = resNames.FirstOrDefault(s => s.Contains("Model.odt"));
                stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SimpleOdtLib.Resources.Model2.odt");
            }
            using (stream )
            {
                OdtDocument rv = Load(stream);
                return rv.Init();
            }
        }

        public static OdtDocument Load(string path)
        {
            FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read);
            OdtDocument rv = Load(fs);
            return rv.Init();
        }

        public static OdtDocument Load(Stream fs)
        {
            OdtDocument rv = new OdtDocument(); ;
            using (ZipArchive za = new ZipArchive(fs))
            {
                foreach (ZipArchiveEntry entry in za.Entries)
                {
                    using (Stream s = entry.Open())
                    {
                        OdtPart p = OdtPart.CreatePart(entry.FullName);
                        switch (p)
                        {
                            case null:
                                continue;
                            case XmlPart part:
                                try
                                {
                                   using (XmlReader xr = XmlReader.Create(s))
                                   {
                                        part.TheXDocument = XDocument.Load(xr);
                                        if (xr.NameTable != null)
                                            part.NamespaceManager = new XmlNamespaceManager(xr.NameTable);
                                   }
                                   rv.Parts.Add(part);
                                }
                                catch (XmlException trash) { }

                                 break;
                            case TextPart part:
                            {
                                using (TextReader tr = new StreamReader(s))
                                    part.Text = tr.ReadToEnd();
                                rv.Parts.Add(part);
                                break;
                            }
                            // todo: binary files?
                        }

                        p.Init(rv);
                    }
                }
            }

            return rv;
        }


        internal OdtDocument Init()
        {
            Debug.WriteLine($"Parts: {Parts.Count}");
            TheMetaPart.SetCreationDate();
            return this;
        }

        public bool SetProperty(string name, string value)
        {
            switch (name)
            {
                case "dc:title":
                    TheMetaPart.SetTitle(value);
                    break;
                case "dc:creator":
                    TheMetaPart.SetCreator(value);
                    break;
            }
            return true; 
        }

        public void ConfigureNotes(bool endnotes, string[] brackets)
        {
            OdtFootnote.Configure(this, endnotes, brackets);
            if (endnotes)
                TheContentPart.ConfigureEndnotes();
        }

        public OdtParagraph AppendParagraph(string text = "", string style = null, XElement target = null)
        {
            target ??= TheContentPart.MainSectionElement;
            OdtParagraph rv = new OdtParagraph()
            {
                Document = this,
                Parent = target,
                Part = TheContentPart,
                Text = text,
                Style = style
            };
            return rv.Build() as OdtParagraph;
        }

        public enum LineStyle
        {
            Single,
            Double
        }

        public void AppendHorizontalLine(LineStyle ls = LineStyle.Single, string position = "bottom", OdtParagraph para = null)
        {
            //string lineStyleName = (ls == LineStyle.Single ? SingleLineStyleName : DoubleLineStyleName)
            //                       + ("bottom".Equals(position) ? "" : "_Top");
            StyleConfiguration sc = new StyleConfiguration()
            {
                BasedOn = para?.Style,
                LinePosition = position == "bottom" ? StyleConfiguration.LinePositionOptions.Below : StyleConfiguration.LinePositionOptions.Above,
                LineStyle = ls == LineStyle.Double ? StyleConfiguration.LineStyleOptions.Double : StyleConfiguration.LineStyleOptions.Single,
                Level = "paragraph"
            };
            string lineStyleName = StylesManager.Instance.GetStyle(sc);
            if (para != null)
                para.Style = lineStyleName;
            else
                AppendParagraph("", lineStyleName);
        }

        public OdtParagraph AppendPageBreak()
        {
            // page break is either soft, or done with a style property
            // which is pure shit, since it requires a new style(s)
            // we are faking it with a pre-defined style (also pretty well shit)
            // todo: possibly revisit this mess

            //XElement pb = new XElement(XName.Get("soft-page-break", text));
            //TheContentPart.BodyTextElement.Add(pb);
            //return pb;
            return AppendParagraph("", StylesManager.Instance.GetStyle(new StyleConfiguration(){PageBreak = true, Level = "paragraph"}));
        }

        public void Save(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Create))
                {
                    foreach (OdtPart part in Parts)
                    {
                        ZipArchiveEntry entry = zip.CreateEntry(part.Path);
                        using (StreamWriter sr = new StreamWriter(entry.Open()) )
                        {
                            sr.Write(part.GetContents());
                        }
                    }
                }
            }
        }


        public void Append(OdtIndex ix)
        {
            // yes, this goes directly in the body text element not the main section element
            // and, it seems, not even in the IndexSection... which is just a trick to 
            // get OLO to not trap us in the endnotes.
            //TheContentPart.IndexSectionElement.Add(ix.Build().ContentElement);
            TheContentPart.BodyTextElement.Add(ix.Build().ContentElement);
        }
    }
}
