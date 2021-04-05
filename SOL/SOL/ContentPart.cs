using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace SOL
{
    public class ContentPart : XmlPart
    {
        public XElement BodyElement { get; internal set; }
        public XElement BodyTextElement { get; internal set; }
        public XElement MainSectionElement { get; set; }
        public XElement IndexSectionElement { get; set; }

        #region Overrides of OdtPart

        public override void Init(OdtDocument doc)
        {
            base.Init(doc);
            Parent.TheContentPart = this;
            BodyElement = TheXDocument.Root?.Element(XName.Get(OdtNames.Body, OdtNames.NamespaceManager.LookupNamespace(OdtNames.Office)));
            if (BodyElement == null)
            {
                // todo: add it?...
            }
            BodyTextElement = BodyElement?.Element(XName.Get(OdtNames.Text, OdtNames.NamespaceManager.LookupNamespace(OdtNames.Office)));
            //BodyTextElement?.Descendants().Remove();

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SOL.Resources.AutomaticStyles.xml"))
            {
                using (XmlReader xr = XmlReader.Create(stream))
                {
                    XDocument xd = XDocument.Load(xr);

                    XElement stylesInDoc = TheXDocument.Root.Element(XName.Get("automatic-styles", OdtDocument.office));
                    if (stylesInDoc == null)
                    {
                        stylesInDoc = new XElement(XName.Get("automatic-styles", OdtDocument.office));
                        TheXDocument.Root.Add(stylesInDoc);
                    }

                    foreach (XElement element in xd.Root.Descendants(XName.Get("style", OdtNames.NamespaceManager.LookupNamespace("style"))))
                    {
                        stylesInDoc.Add(new XElement(element));
                    }
                }
            }

            XName[] suspects = { XName.Get("p", OdtDocument.text), XName.Get("alphabetical-index", OdtDocument.text) };
            foreach (XName name in suspects)
            {
                var victims = BodyTextElement?.Elements(name).ToArray()??new XElement[0];
                foreach (XElement victim in victims)
                {
                    victim?.Remove();
                }
            }

            // after any leftovers are cleaned out...
            MainSectionElement = new XElement(XName.Get("section", OdtDocument.text),
                new XAttribute(XName.Get("style-name", OdtDocument.text), "RegisterSOLMainSection"),
                new XAttribute(XName.Get("name", OdtDocument.text), "MainSection"));
            BodyTextElement.Add(MainSectionElement);

            // hmmm... this achieves a break, but the index doesn't really go inside it
            IndexSectionElement = new XElement(XName.Get("section", OdtDocument.text),
                new XAttribute(XName.Get("style-name", OdtDocument.text), "RegisterSect1"),
                new XAttribute(XName.Get("name", OdtDocument.text), "IndexSection"));
            BodyTextElement.Add(IndexSectionElement);
        }

        #endregion

        internal void ConfigureEndnotes()
        {
            MainSectionElement.SetAttributeValue(XName.Get("style-name", OdtDocument.text), "RegisterMainSectionE");
            Parent.AppendParagraph(target: IndexSectionElement, style: Parent.PageBreakStyleName);
            // this will gives a page break before the placeholder
            // but then, a blank page after the index is updated.
            //Parent.AppendParagraph(target: BodyTextElement, style: Parent.PageBreakStyleName);
        }

        /// <summary>
        /// Remove all the (model) text content from the body
        /// </summary>
        /// <returns></returns>
        //internal string Clear(XElement el)
        //{
        //    string rv = (el as XNode)?.ToString();
        //    foreach (XElement child in el.Elements())
        //    {
        //        child.Remove();
        //    }
        //    return rv;
        //}
    }
}