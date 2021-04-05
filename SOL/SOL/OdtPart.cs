using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SOL
{
    public abstract class OdtPart
    {
        public static OdtPart CreatePart(string path)
        {
            OdtPart rv = null;
            if (path.EndsWith("mimetype"))
                rv = new MimetypePart();
            else if (path.EndsWith("content.xml"))
                rv = new ContentPart();
            else if (path.EndsWith("styles.xml"))
                rv = new StylesPart();
            else if (path.EndsWith("meta.xml"))
                rv = new MetaPart();
            else if (path.EndsWith(".xml"))
                rv = new XmlPart();
            else
                rv = new TextPart();
            rv.Path = path;
            return rv;
        }

        public string Path { get; set; }
        public OdtDocument Parent { get; set; }

        public virtual void Init(OdtDocument doc)
        {
            Parent = doc;
        }

        public abstract string GetContents();
    }

    public class XmlPart : OdtPart
    {
        public XDocument TheXDocument { get; set; } 
        public XmlNamespaceManager NamespaceManager { get; set; }

        #region Overrides of OdtPart

        public override string GetContents()
        { // todo: this could be better arranged between this producer and its consumer
            using (Stream os = new MemoryStream())
            {
                using (XmlWriter xw = new XmlTextWriter(os, Encoding.UTF8) {Formatting = Formatting.None})
                {
                    TheXDocument?.Root?.WriteTo(xw);
                    xw.Flush();
                    os.Seek(0, SeekOrigin.Begin);
                    return new StreamReader(os).ReadToEnd();
                }
            }
        }

        //public override void Init(OdtDocument doc)
        //{
        //    base.Init(doc);
        //    //if (XmlDocument?.NameTable == null)
        //    //{
        //    //    Debug.WriteLine("Internal error, XmlDocument is not initialized");
        //    //    return;
        //    //}
        //    //NamespaceManager = new XmlNamespaceManager(XmlDocument.NameTable);
        //}

        #endregion
    }

    public class TextPart : OdtPart
    {
        public string Text { get; set; }

        #region Overrides of OdtPart

        public override string GetContents()
        {
            return Text;
        }
        #endregion
    }

    public class MimetypePart : TextPart
    {

    }
}
