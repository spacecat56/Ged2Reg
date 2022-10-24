// StylesPart.cs
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace SOL
{
    public class StylesPart : XmlPart
    {
        public XElement[] NotesConfiguration { get; set; }
        internal List<XElement> OurStyles { get; } = new List<XElement>();
        public XElement DocumentLevelStyles { get; set; }

        public StylesManager TheStylesManager { get; private set; }


        #region Overrides of OdtPart

        public override void Init(OdtDocument doc)
        {
            base.Init(doc);
            Parent.TheStylesPart = this;

            //string[] resNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            //string rn = resNames.FirstOrDefault(s => s.Contains("TextStyles.xml"));
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SimpleOdtLib.Resources.TextStyles.xml"))
            {
                using (XmlReader xr = XmlReader.Create(stream))
                {
                    XDocument xd = XDocument.Load(xr);
                    //XElement addedStyles = xd.Root.Element(XName.Get("styles", OdtDocument.office));

                    // our required index styles; could be expanded to replace the 
                    // ones dragged in in the model odt file:
                    XName copyToName = XName.Get("styles", OdtDocument.office);
                    XName copyFromName = XName.Get("style", OdtDocument.style);
                    CopyStyles(xd, copyFromName, copyToName, keepCopy:true); // hmm... more than needed here?

                    // for convenience of StylesManager
                    DocumentLevelStyles = TheXDocument.Root.Element(copyToName);

                    //// style for our page size, borders, and footnote separator
                    //copyToName = XName.Get("automatic-styles", OdtDocument.office);
                    //copyFromName = XName.Get("page-layout", OdtDocument.style);
                    //CopyStyles(xd, copyFromName, copyToName);

                    //// make it the one and only master page (?: they are zombies)
                    //copyToName = XName.Get("master-styles", OdtDocument.office);
                    //copyFromName = XName.Get("master-page", OdtDocument.style);
                    //CopyStyles(xd, copyFromName, copyToName, true);

                    //// replace the default page layout
                    //XElement container = TheXDocument.Root.Element(XName.Get("styles", OdtDocument.office));
                    //XElement victim = TheXDocument.Root.Element(XName.Get("default-page-layout", OdtDocument.style));
                    //XElement winner = xd.Root.Element(XName.Get("default-page-layout", OdtDocument.style));
                    //if (winner == null) return;
                    //victim?.Remove();
                    //container?.Add(winner);

                    // this is a nightmare stupid little problem
                    // best i can do??? it's ok on even-number pages????
                    foreach (XElement element in 
                        TheXDocument.Root.Descendants(XName.Get("footnote-sep", OdtDocument.style)))
                    {
                        element.SetAttributeValue(XName.Get("width", OdtDocument.style), "0.003in");
                    }

                    // we need to have the notes-configuration in hand for easy changes
                    NotesConfiguration = TheXDocument.Root.Descendants(XName.Get("notes-configuration", OdtDocument.text)).ToArray();
                    XElement[] myNoteConfig = xd.Root.Descendants(XName.Get("notes-configuration", OdtDocument.text)).ToArray();
                    if (myNoteConfig != null)
                    {
                        NotesConfiguration?.Remove();
                        NotesConfiguration = new XElement[myNoteConfig.Length];
                        for (int i = 0; i < myNoteConfig.Length; i++)
                        {
                            NotesConfiguration[i] = new XElement(myNoteConfig[i]);
                            TheXDocument.Root.Descendants(XName.Get("styles", OdtDocument.office)).First().Add(NotesConfiguration[i]);
                        }
                    }
                }
            }

            TheStylesManager = StylesManager.Init(this);
        }

        private void CopyStyles(XDocument xmlDoc, XName copyFromName, XName copyToName, bool clearDestination = false, bool keepCopy = false)
        {
            XElement destination = TheXDocument.Root.Element(copyToName);
            if (clearDestination)
                destination.RemoveNodes();
            foreach (XElement element in xmlDoc.Root.Descendants(copyFromName))
            {
                var xe = new XElement(element);
                destination.Add(new XElement(xe));
                if (keepCopy)
                    OurStyles.Add(xe);
            }
        }

        #endregion

        public string StyleName(string displayName)
        { // mimic what Libre Office does (as of v4, anyway); is this needed anymore
            return (displayName ?? "").Replace(" ", "_20_");
        }

        public XElement GetNotesConfiguration(bool endnotes)
        {
            XElement rv =
                (from el in NotesConfiguration
                    where (string) el.Attribute(XName.Get("note-class", OdtDocument.text)) == (endnotes ? "endnote" : "footnote")
                 select el)
                .FirstOrDefault();
            return rv;
        }

        public void SetMargins(float marginLeft, float marginRight, float marginTop, float marginBottom)
        {
            XElement pageSettings = TheXDocument.Descendants(XName.Get("page-layout-properties", OdtDocument.style)).FirstOrDefault();
            if (pageSettings == null)
                return; // fail!
            var fo = OdtNames.NamespaceManager.LookupNamespace("fo");
            pageSettings.SetAttributeValue(XName.Get("margin-left", fo), $"{marginLeft:0.0}in");
            pageSettings.SetAttributeValue(XName.Get("margin-right", fo), $"{marginRight:0.0}in");
            pageSettings.SetAttributeValue(XName.Get("margin-top", fo), $"{marginTop:0.0}in");
            pageSettings.SetAttributeValue(XName.Get("margin-bottom", fo), $"{marginBottom:0.0}in");
        }
    }
}