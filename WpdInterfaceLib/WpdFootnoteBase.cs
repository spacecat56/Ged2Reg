// WpdFootnoteBase.cs
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


namespace WpdInterfaceLib
{
    public abstract class WpdFootnoteBase
    {
        public string[] Brackets;
        public static string DefaultFootnoteStyle { get; set; }  = "FootnoteText";
        public static string DefaultFootnoteRefStyle { get; set; } = "FootnoteReference";
        //public abstract int MaxFootnoteId { get; }
        public string NoteReferenceStyle { get;  set; }
        public string NoteTextStyle { get;  set; }
        public bool IsApplied { get;  set; }
        public string BookmarkName { get; set; }
        public int BookmarkId { get; set; }
        public abstract int? Id { get; }
        public List<WpdNoteFragment> Fragments { get; set; }

        public WpdFootnoteBase AppendText(string t)
        {
            (Fragments ??= new List<WpdNoteFragment>())
                .Add(new WpdNoteFragment() { Type = WpdFragmentType.Text, Content = t });
            return this;
        }

        public WpdFootnoteBase AppendNoteRef(WpdFootnoteBase other)
        {
            (Fragments ??= new List<WpdNoteFragment>())
                .Add(new WpdNoteFragment() { Type = WpdFragmentType.Noteref, DataObject = other});
            return this;
        }

        public WpdFootnoteBase AppendHyperlink(string t)
        {
            (Fragments ??= new List<WpdNoteFragment>())
                .Add(new WpdNoteFragment() { Type = WpdFragmentType.Hyperlink, Content = t });
            return this;
        }

        public abstract void Apply(IWpdParagraph bodyParaI, bool bookmarked = false);
    }
}