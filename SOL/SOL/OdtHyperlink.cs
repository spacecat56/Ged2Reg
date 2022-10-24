// OdtHyperlink.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SOL
{
    public class OdtHyperlink : OdtBodyElement
    {
        /*
        <text:a xlink:href="http://www.google.com" office:target-frame-name="_top" xlink:show="replace">
        <text:span text:style-name="T11">http://www.google.com</text:span>
        </text:a>


        */
        public static bool UseDomainForNullLinkText { get; set; } = true;

        public string URL { get; set; }

        #region Overrides of OdtBodyElement

        public override OdtBodyElement Build()
        {
            string nst = OdtNames.NamespaceManager.LookupNamespace(OdtNames.Text);
            string nsx = OdtNames.NamespaceManager.LookupNamespace(OdtNames.Xlink);
            string nso = OdtNames.NamespaceManager.LookupNamespace(OdtNames.Office);

            ContentElement = new XElement(XName.Get(OdtNames.A, nst));
            ContentElement.SetAttributeValue(XName.Get(OdtNames.Href, nsx), URL);
            ContentElement.SetAttributeValue(XName.Get(OdtNames.Show, nsx), "replace");
            ContentElement.SetAttributeValue(XName.Get(OdtNames.TargetFrame, nso), "_top");

            new OdtSpan() {Document = Document, Style = "Hyperlink", Text = BestText()}.Build().ApplyTo(this);

            return this;
        }

        #endregion

        internal string BestText()
        {
            if (!UseDomainForNullLinkText)
                return URL;
            try
            {
                Uri uri = new Uri(URL);
                string host = uri.Host;
                while (host.IndexOf('.') != host.LastIndexOf('.'))
                    host = host.Substring(host.IndexOf('.')+1);
                if (host.Length < 2)
                    return host;
                return $"{host.Substring(0, 1).ToUpper()}{host.Substring(1)}";
            }
            catch
            {
                return URL;
            }
        }
    }
}
