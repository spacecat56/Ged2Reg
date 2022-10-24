// OdtIndex.cs
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
