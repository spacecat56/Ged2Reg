// OdtParagraph.cs
// Copyright 2022 Thomas W. Shanley
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Text;
using System.Xml.Linq;

namespace SOL
{
    public class OdtParagraph : OdtBodyElement
    {
        private string _ns;
        private string _style;

        public string Style
        {
            get => _style;
            set { _style = value; ApplyStyle();}
        }

        public string Text { get; set; }

        public override OdtBodyElement Build()
        { // todo: called again? re-build it!? ... then, what about content?
            _ns = OdtNames.NamespaceManager.LookupNamespace(OdtNames.Text);

            ContentElement = new XElement(XName.Get(OdtNames.Paragraph, _ns), OdtText.Prepare(Text));
            ApplyStyle();

            Parent.Add(ContentElement);

            return this;
        }

        private void ApplyStyle()
        {
            if (!string.IsNullOrEmpty(Style))
            {
                ContentElement?.SetAttributeValue(XName.Get(OdtNames.Style, _ns), Document.TheStylesPart.StyleName(Style));
            }
        }

        public override void Append(OdtBodyElement newchild)
        {
            if (ContentElement == null)
                Build();
            //ContentElement.Add(newchild);
            newchild.ApplyTo(this); // hmm. chasing own tail here...
        }

        public OdtParagraph Append(string text, string style = null, bool? bold = null, bool? italic = null)
        {
            if (ContentElement == null)
                Build();

            if ((bold??false) || (italic??false))
            {
                StyleConfiguration sc = new StyleConfiguration()
                {
                    BasedOn = style,
                    Bold = bold??false,
                    Italic = italic??false
                };
                style = StylesManager.Instance.GetStyle(sc);
            }

            if (style == null)
                ContentElement.Add(OdtText.Prepare(text));
            else
            {
                new OdtSpan()
                {
                    Document = Document, Part = Part, Style = style, Text = text
                }.Build().ApplyTo(this);

            }
 
            //XElement span = new XElement(XName.Get(OdtNames.Span, _ns), text);

            //if (!string.IsNullOrEmpty(style))
            //{
            //    span.SetAttributeValue(XName.Get(OdtNames.Style, _ns), style);
            //}

            //ContentElement.Add(span);

            return this;
        }
    }
}
