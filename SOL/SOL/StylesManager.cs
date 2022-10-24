// StylesManager.cs
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

using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace SOL
{
    /// <summary>
    /// In OpenDocument, certain things can only be done with styles, including ones we care about:
    /// Page break, lines (borders), bold, and italic.  If done interactively in e.g. Libre Office these
    /// are applied as automatic styles, which are not accessible for modification as styles, only one by
    /// one.  Here, we keep track of teh styles we are using and what they do, and make a new derivative
    /// iff we do not already have the one wanted at a particular point.  By adding them to styles proper,
    /// we make the document more manageable.
    /// </summary>
    public class StylesManager
    {
        private StylesPart _stylesPart;

        private Dictionary<string, StyleConfiguration> _knownStyles = new Dictionary<string, StyleConfiguration>();

        
        public static StylesManager Instance { get; set; }
        public static string NewStyleNamePrefix { get; set; } = "Register_";
        
        internal static StylesManager Init(StylesPart part)
        {
            return Instance = new StylesManager(part);
        }

        private StylesManager() { }

        private StylesManager(StylesPart part)
        {
            _stylesPart = part;

            foreach (XElement style in part.OurStyles)
            {
                StyleConfiguration sc = new StyleConfiguration(style);
                if (sc.IsValid)
                    _knownStyles.Add(sc.StyleName, sc);
            }
        }

        public string GetStyle(StyleConfiguration sc)
        {
            _knownStyles.TryGetValue(sc.NewName(), out StyleConfiguration rv);
            return rv?.StyleName ?? Build(sc);
        }

        private string Build(StyleConfiguration sc)
        {
            //sc.BasedOn ??= NewStyleNamePrefix;
            //_knownStyles.TryGetValue(sc.BasedOn??"unknown", out StyleConfiguration baseSc); // can't help you

            string fo = OdtNames.NamespaceManager.LookupNamespace("fo");

            XElement el = new XElement(XName.Get("style", OdtDocument.style));
            el.SetAttributeValue(XName.Get("name", OdtDocument.style), sc.NewName());
            
            el.SetAttributeValue(XName.Get("family", OdtDocument.style), sc.Level??"text");
            if (!string.IsNullOrEmpty(sc.BasedOn))
                el.SetAttributeValue(XName.Get("parent-style-name", OdtDocument.style), sc.BasedOn);

            bool tp = false;
            bool pp = false;

            XElement tProps = new XElement(XName.Get("text-properties", OdtDocument.style));
            if (sc.Bold)
            {
                tProps.SetAttributeValue(XName.Get("font-weight", fo), "bold" );
                tp = true;
            }

            if (sc.Italic)
            {
                tProps.SetAttributeValue(XName.Get("font-style", fo), "italic");
                tp = true;
            }
            if (tp) el.Add(tProps);

            XElement pProps = new XElement(XName.Get("paragraph-properties", OdtDocument.style));
            // we use this so little, that really working it out is rather pointless here
            // that could entail keeping track of the current para, etc. etc.
            // assume you are applying to a new para and the next thing wants a clean top-of-page
            if (sc.PageBreak)
            {
                pProps.SetAttributeValue(XName.Get("break-after", fo), "page");
                pp = true;
            }

            if (sc.HasLine)
            {
                if (sc.LinePosition == StyleConfiguration.LinePositionOptions.Above)
                {
                    if (sc.LineStyle == StyleConfiguration.LineStyleOptions.Double)
                    {   
                        // top, double
                        pProps.SetAttributeValue(XName.Get("border-line-width-top", OdtDocument.style), "0.0047in 0.006in 0.0047in");
                        pProps.SetAttributeValue(XName.Get("padding-top", fo), "0.100in");
                        pProps.SetAttributeValue(XName.Get("border-top", fo), "0.99pt double #000000"); // hmmm... redundant? who wins?
                    }
                    else
                    {   
                        // top, single
                        pProps.SetAttributeValue(XName.Get("padding-top", fo), "0.100in");
                        pProps.SetAttributeValue(XName.Get("border-top", fo), "0.99pt solid #000000"); 
                    }
                }
                else
                {
                    if (sc.LineStyle == StyleConfiguration.LineStyleOptions.Double)
                    {   
                        // bottom, double
                        pProps.SetAttributeValue(XName.Get("border-line-width-bottom", OdtDocument.style), "0.0047in 0.006in 0.0047in");
                        pProps.SetAttributeValue(XName.Get("padding-bottom", fo), "0.0291in");
                        pProps.SetAttributeValue(XName.Get("border-bottom", fo), "0.99pt double #000000"); // hmmm... redundant? who wins?
                    }
                    else
                    {   
                        // bottom, single
                        pProps.SetAttributeValue(XName.Get("padding-bottom", fo), "0.0291in");
                        pProps.SetAttributeValue(XName.Get("border-bottom", fo), "0.99pt solid #000000"); 
                    }
                }

                pp = true;
            }
            if (pp) el.Add(pProps);



            sc.StyleName = sc.NewName();
            _knownStyles.Add(sc.StyleName, sc);
            _stylesPart.DocumentLevelStyles.Add(el);
            return sc.StyleName; 
        }
    }

    public class StyleConfiguration
    {
        public StyleConfiguration() { }

        internal StyleConfiguration(XElement style)
        { // this is for our internally predefined styles, we need to be able to find them based on the 
            StyleName = style.Attribute(XName.Get("name", OdtDocument.style))?.Value;
            Level = style.Attribute(XName.Get("family", OdtDocument.style))?.Value;

            IsValid = !string.IsNullOrEmpty(StyleName) && !string.IsNullOrEmpty(Level);

            // to be entirely generic we should parse these internal styles to 
            // discover if they are bold, etc.  but we can rely on our own logic
            // not trying to re-bold the already-bold, etc. so here we skate on it.
            return;
        }

        public enum LinePositionOptions
        {
            None,
            Above,
            Below
        }

        public enum LineStyleOptions
        {
            None,
            Double,
            Single
        }

        public bool IsValid { get; private set; }
        public string BasedOn { get; set; }
        public string StyleName { get; set; }
        public string Level { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool PageBreak { get; set; }
        public LinePositionOptions LinePosition { get; set; }
        public LineStyleOptions LineStyle { get; set; }

        public bool HasLine => LinePosition != LinePositionOptions.None && LineStyle != LineStyleOptions.None;

        public string KeyString()
        {
            return $"{BasedOn??StyleName}{Level}{Bold}{Italic}{PageBreak}{LinePosition}{LineStyle}";
        }

        public string NewName()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(BasedOn);
            if (Bold) sb.Append("_B");
            if (Italic) sb.Append("_I");
            if (PageBreak) sb.Append("_PB");
            if (LineStyle != LineStyleOptions.None)
                sb.Append("_Line_").Append(LinePosition.ToString());
            if (LinePosition != LinePositionOptions.None)
                sb.Append("_").Append(LineStyle.ToString());
            return sb.ToString();
        }

    }
}
