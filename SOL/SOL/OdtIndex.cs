using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SOL
{
    public class OdtIndex : OdtBodyElement
    {
        /*
            Indexes are difficult, we are going to do it one way that should look ok
            If the user wants to change the layout, "good luck with that"
            Using a model built from a resource - there's only a few things we might want to change
        */

        public int Columns { get; set; }
        public string Heading { get; set; }
        public string Placeholder { get; set; }

        #region Overrides of OdtBodyElement

        public override OdtBodyElement Build()
        {
            var textNamespace = OdtNames.NamespaceManager.LookupNamespace(OdtNames.Text);
            var styleNamespace = OdtNames.NamespaceManager.LookupNamespace(OdtNames.Style);

            Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("SimpleOdtLib.Resources.IndexExample.xml");
            ContentElement = XElement.Load(s);

            // then we may need to tweak a few things
            // the model assumes 2 colums, if 1, change the style
            if (Columns == 1)
            {
                ContentElement.SetAttributeValue(XName.Get("style-name", textNamespace), "SOLSect1");
            }

            // if we are given a heading, use it
            if (!string.IsNullOrEmpty(Heading))
            {
                XElement tt = ContentElement.Descendants(XName.Get("index-title-template", textNamespace)).FirstOrDefault();
                tt?.SetValue(Heading);
            }

            // if we are given a placeholder, use it
            if (!string.IsNullOrEmpty(Placeholder))
            {
                XElement ibp = ContentElement.Descendants(XName.Get("index-body", textNamespace)).FirstOrDefault();
                ibp = ibp?.Descendants(XName.Get("p", textNamespace)).FirstOrDefault();
                ibp?.SetValue(Placeholder);
            }
            
            return this;
        }

        #endregion
    }



}
