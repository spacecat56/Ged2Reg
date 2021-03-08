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

        public void BreakForIndex()
        {
            //  no-op here
        }

        #endregion
    }
}
