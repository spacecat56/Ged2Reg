// OdtSpan.cs
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
