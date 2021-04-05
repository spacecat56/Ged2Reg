using System;
using System.Linq;
using System.Xml.Linq;

namespace SOL
{
    public class MetaPart : XmlPart
    {
        public XElement MetaElement { get; internal set; }

        #region Overrides of XmlPart

        public override void Init(OdtDocument doc)
        {
            base.Init(doc);
            doc.TheMetaPart = this;
            MetaElement = TheXDocument.Root.Descendants(XName.Get("meta", OdtNames.NamespaceManager.LookupNamespace("office")))
                .FirstOrDefault();
        }

        #endregion

        public void SetCreator(string s)
        {
            GetOrAdd(XName.Get("creator", OdtNames.NamespaceManager.LookupNamespace("dc")))?.SetValue(s);
            // also meta:initial-creator
            GetOrAdd(XName.Get("initial-creator", OdtNames.NamespaceManager.LookupNamespace("meta")))?.SetValue(s);
        }
        public void SetTitle(string s)
        {
            GetOrAdd(XName.Get("title", OdtNames.NamespaceManager.LookupNamespace("dc")))?.SetValue(s);
        }


        /*
            set the dates (seems to only work as local time, if TZ included the apps display Zulu time)
            and also clear out the artifacts from using a model document 

            <meta:editing-duration>PT4M56S</meta:editing-duration>
            <meta:editing-cycles>2</meta:editing-cycles>
            <meta:generator>LibreOffice/6.1.5.2$Windows_X86_64 LibreOffice_project/90f8dcf33c87b3705e78202e3df5142b201bd805</meta:generator>

        */

        public void SetCreationDate()
        {
            //DateTime dt = DateTime.Now; //.ToLocalTime();
            DateTimeOffset dto = DateTimeOffset.Parse($"{DateTime.Now:O}", null);
            GetOrAdd(XName.Get("date", OdtNames.NamespaceManager.LookupNamespace("dc")))?.SetValue(dto.DateTime);
            GetOrAdd(XName.Get("creation-date", OdtNames.NamespaceManager.LookupNamespace("meta")))?.SetValue(dto.DateTime);

            GetOrAdd(XName.Get("editing-duration", OdtNames.NamespaceManager.LookupNamespace("meta")))?.SetValue("PT0M01S");
            GetOrAdd(XName.Get("editing-cycles", OdtNames.NamespaceManager.LookupNamespace("meta")))?.SetValue("1");
            GetOrAdd(XName.Get("generator", OdtNames.NamespaceManager.LookupNamespace("meta")))?.SetValue("OdtAdapterLib");
        }

        internal XElement GetOrAdd(XName xname)
        {
            if (MetaElement == null)
                return null;  // give up.  "should not happen"
            XElement rv = MetaElement.Element(xname);

            if (rv == null)
            {
                rv = new XElement(xname);
                MetaElement.Add(rv);
            }

            return rv;
        }
    }
}