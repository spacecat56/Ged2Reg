// OoxEndnote.cs
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
using DocumentFormat.OpenXml.Wordprocessing;
using WpdInterfaceLib;

namespace DocxAdapterLib
{
    public class OoxEndnote : OoxFootnote
    {

        public static string DefaultEndnoteStyle { get; set; } = "EndnoteText";
        public static string DefaultEndnoteRefStyle { get; set; } = "EndnoteReference";


        public OoxEndnote(OoxDoc document, string noteText = null, string[] brackets = null) : base(document, null)
        {
            NoteReferenceStyle = DefaultEndnoteRefStyle;
            NoteTextStyle = DefaultFootnoteStyle; // todo: make sure this exists: DefaultEndnoteStyle;
            NoteReferenceNode = "endnoteReference";
            NoteRefNode = "endnoteRef";
            NoteNode = "endnote";

            Init(document, noteText, brackets);

        }
        internal override void AssignNextId()
        {
            id = (doc.MaxEndnoteId() + 1);
        }
         internal override OoxHyperlink BuildHyperlink(WpdNoteFragment fragment)
        {
            OoxHyperlink h = new OoxHyperlink()
            {
                Anchor = fragment.Content,
                Uri = new Uri(fragment.Content),
                Doc = doc,
                Location = OoxHyperlink.LinkLocation.Endnote
            };
            return h;
        }

        #region Overrides of OoxFootnote

        internal override FootnoteEndnoteType CreateNoteInstance()
        {
            return new Endnote(){Id = id};
        }

        internal override FootnoteEndnoteReferenceType CreateNoteReference()
        {
            return new EndnoteReference() { Id = id };
        }

        internal override Run BuildNoteRefMark()
        {
            return new Run(new EndnoteReferenceMark()) { RunProperties = new RunProperties(new RunStyle() { Val = NoteReferenceStyle }) };
        }

        #endregion
    }
}
