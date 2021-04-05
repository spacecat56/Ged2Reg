using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SOL
{
    public class OdtSpan : OdtBodyElement
    {
        public string Text { get; set; }

        public string Style { get; set; }
        //public OdtBookMark BookMark { get; internal set; }

        #region Overrides of OdtBodyElement

        public override OdtBodyElement Build()
        {
            var ns = OdtNames.NamespaceManager.LookupNamespace(OdtNames.Text);
            ContentElement = new XElement(XName.Get(OdtNames.Span, ns), OdtText.Prepare(Text));

            if (!string.IsNullOrEmpty(Style))
            {
                ContentElement.SetAttributeValue(XName.Get(OdtNames.Style, ns), Document.TheStylesPart.StyleName(Style));
            }

            return this;
        }

        #endregion
    }
}
