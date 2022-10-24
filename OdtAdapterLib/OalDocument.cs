// OalDocument.cs
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
using System.Text;
using System.Threading.Tasks;
using SOL;
using WpdInterfaceLib;

namespace OdtAdapterLib
{
    public class OalDocument : IWpdDocument
    {
        public OdtDocument Document { get; internal set; }
        public string FileName { get; set; }

        #region Implementation of IDisposable

        public void Dispose()
        {
            // todo: does the odt doc need to do something?
            Document = null;
        }

        #endregion

        #region Implementation of IWpdDocument

        public IWpdDocument Init()
        {
            return this;
        }

        public void Save()
        {
            Document.Save(FileName);
        }

        public void ApplyTemplateStyles(Stream stream, bool fonts)
        {
            // todo
        }

        public bool SetCoreProperty(string propertyName, string propertyValue)
        {
            return Document.SetProperty(propertyName, propertyValue);
        }

        public void Apply(WpdPageSettings ps)
        {
            Document.TheStylesPart.SetMargins(
                ps.MarginLeft / ps.PerInchFactorApplied,
                ps.MarginRight / ps.PerInchFactorApplied,
                ps.MarginTop / ps.PerInchFactorApplied,
                ps.MarginBottom / ps.PerInchFactorApplied);
        }

        public void InsertPageBreak()
        {
            Document.AppendPageBreak();
        }

        public List<WpdStyleInfo> ListStyles()
        {
            // todo
            return new List<WpdStyleInfo>();
        }

        public void ConfigureFootnotes(bool asEndnotes, string[] brackets)
        {
            Document.ConfigureNotes(asEndnotes, brackets);
        }

        public IWpdParagraph InsertParagraph(string text = null)
        {
            return new OalParagraph()
            {
                Paragraph = Document.AppendParagraph(text),
                Document = this
            };
        }

        public WpdNoteRefField BuildNoteRef(WpdFootnoteBase fn)
        {
            return new OalNoteRef(this, fn);
        }

        public WpdIndexField BuildIndexField()
        {
            return new OalIndex(this);
        }

        public WpdIndexEntry BuildIndexEntryField(string indexName, string indexValue)
        {
            return new OalIndexEntry(this){ContentText = indexValue};
        }

        public WpdFootnoteBase BuildFootNote(string noteText = null, string[] brackets = null)
        {
            var f = new OdtFootnote(){Document = Document, Brackets = brackets}; 
            var rv = new OalFootnote(noteText){Footnote = f, Brackets = brackets, Document = Document};
            
            return rv;
        }

        public WpdFootnoteBase BuildEndNote(string noteText = null, string[] brackets = null)
        {
            var f = new OdtEndnote() { Document = Document, Brackets = brackets };
            var rv = new OalFootnote(noteText) { Footnote = f, Brackets = brackets, Document = Document };

            return rv;
        }

        public bool HasNonDefaultEndnotes()
        { // todo
            return false;
        }

        public bool HasNonDefaultFootnotes()
        {// todo
            return false;
        }
        public bool HasReachedFootnoteLimit()
        {
            return false; // not clear if there is / what is the limit here
        }
        public void BreakForIndex()
        {
            //  no-op here
        }

        #endregion
    }
}
