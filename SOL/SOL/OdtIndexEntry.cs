// OdtIndexEntry.cs
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
    public class OdtIndexEntry : OdtBodyElement
    {
        /*

        Reference: ISO/IEC 26300-1:2015(E), Page 163

        <text:alphabetical-index-mark text:string-value="William" text:key1="Ives"/>

        */
        public string DelimitedText { get; set; }
        public char Delimiter { get; set; } = ':';

        #region Overrides of OdtBodyElement

        public override OdtBodyElement Build()
        {
#nullable enable
            var textNamespace = OdtNames.NamespaceManager.LookupNamespace(OdtNames.Text);
#nullable restore
            ContentElement = new XElement(XName.Get("alphabetical-index-mark", textNamespace));
            if (DelimitedText == null) return this;

            string[] parts = DelimitedText.Split(Delimiter,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length == 1)
            {
                ContentElement.SetAttributeValue(XName.Get("string-value", textNamespace), parts[0]);
                ContentElement.SetAttributeValue(XName.Get($"key1", textNamespace), "(no surname)");
                return this;
            }

            for (int i = 0; i < parts.Length; i++)
            {
                if (i == parts.Length - 1)
                {
                    // the last piece is the value
                    ContentElement.SetAttributeValue(XName.Get("string-value", textNamespace), parts[i]);
                    //if (parts.Length > 1)
                        break;
                }
                ContentElement.SetAttributeValue(XName.Get($"key{i+1:0}", textNamespace), parts[i]);
            }


            return this;
        }

        #endregion
    }
}
