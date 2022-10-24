// IWpdDocument.cs
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
//
namespace WpdInterfaceLib
{
    public interface IWpdDocument : IDisposable
    {
        IWpdDocument Init();
        void Save();
        void ApplyTemplateStyles(Stream stream, bool fonts);
        bool SetCoreProperty(string propertyName, string propertyValue);
        void Apply(WpdPageSettings ps);
        void InsertPageBreak();
         List<WpdStyleInfo> ListStyles();

         void ConfigureFootnotes(bool asEndnotes, string[] brackets);
        //void Apply(FootnoteEndnoteType fe);

        // factory methods are required to participate
        IWpdParagraph InsertParagraph(string text = null);
        WpdNoteRefField BuildNoteRef(WpdFootnoteBase fn);
        WpdIndexField BuildIndexField();
        WpdIndexEntry BuildIndexEntryField(string indexName, string indexValue);
        WpdFootnoteBase BuildFootNote(string noteText = null, string[] brackets = null);
        WpdFootnoteBase BuildEndNote(string noteText = null, string[] brackets = null); // Endnote must be a subclass of Footnote
        bool HasNonDefaultEndnotes();
        bool HasNonDefaultFootnotes();
        void BreakForIndex();
        bool HasReachedFootnoteLimit();
    }
}