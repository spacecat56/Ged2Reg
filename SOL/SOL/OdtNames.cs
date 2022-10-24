// OdtNames.cs
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

using System.Xml;

namespace SOL
{
    public class OdtNames
    {
        internal const string Paragraph = "p";
        internal const string Style = "style-name";
        internal const string Body = "body";
        internal const string Span = "span";
        internal const string Id = "id";
        internal const string A = "a";
        internal const string Href = "href";
        internal const string Show = "show";
        internal const string TargetFrame = "target-frame-name";



        internal const string Office = "office";
        internal const string Text = "text";
        internal const string Xlink = "xlink";

        public static XmlNamespaceManager NamespaceManager { get; }
        static OdtNames()
        {
            var nt = new NameTable();
            NamespaceManager = new XmlNamespaceManager(nt);

            NamespaceManager.AddNamespace(Office, "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
            NamespaceManager.AddNamespace("style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
            NamespaceManager.AddNamespace(Text, "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
            NamespaceManager.AddNamespace("table", "urn:oasis:names:tc:opendocument:xmlns:table:1.0");
            NamespaceManager.AddNamespace("draw", "urn:oasis:names:tc:opendocument:xmlns:drawing:1.0");
            NamespaceManager.AddNamespace("fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
            NamespaceManager.AddNamespace(Xlink, "http://www.w3.org/1999/xlink");
            NamespaceManager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            NamespaceManager.AddNamespace("meta", "urn:oasis:names:tc:opendocument:xmlns:meta:1.0");
            NamespaceManager.AddNamespace("number", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
            NamespaceManager.AddNamespace("svg", "urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0");
            NamespaceManager.AddNamespace("chart", "urn:oasis:names:tc:opendocument:xmlns:chart:1.0");
            NamespaceManager.AddNamespace("dr3d", "urn:oasis:names:tc:opendocument:xmlns:dr3d:1.0");
            NamespaceManager.AddNamespace("math", "http://www.w3.org/1998/Math/MathML");
            NamespaceManager.AddNamespace("form", "urn:oasis:names:tc:opendocument:xmlns:form:1.0");
            NamespaceManager.AddNamespace("script", "urn:oasis:names:tc:opendocument:xmlns:script:1.0");
            NamespaceManager.AddNamespace("ooo", "http://openoffice.org/2004/office");
            NamespaceManager.AddNamespace("ooow", "http://openoffice.org/2004/writer");
            NamespaceManager.AddNamespace("oooc", "http://openoffice.org/2004/calc");
            NamespaceManager.AddNamespace("dom", "http://www.w3.org/2001/xml-events");
            NamespaceManager.AddNamespace("xforms", "http://www.w3.org/2002/xforms");
            NamespaceManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            NamespaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            NamespaceManager.AddNamespace("rpt", "http://openoffice.org/2005/report");
            NamespaceManager.AddNamespace("of", "urn:oasis:names:tc:opendocument:xmlns:of:1.2");
            NamespaceManager.AddNamespace("rdfa", "http://docs.oasis-open.org/opendocument/meta/rdfa#");
            NamespaceManager.AddNamespace("field", "urn:openoffice:names:experimental:ooo-ms-interop:xmlns:field:1.0");

        }
    }
}