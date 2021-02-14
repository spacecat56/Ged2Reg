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
            // todo
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

        public IWpdParagraph InsertParagraph(string text = null)
        {
            return new OalParagraph() {Paragraph = Document.AppendParagraph(text)};
        }

        public WpdNoteRefField BuildNoteRef()
        {
            throw new NotImplementedException();
        }

        public WpdIndexField BuildIndexField()
        {
            throw new NotImplementedException();
        }

        public WpdIndexEntry BuildIndexEntryField(string indexName, string indexValue)
        {
            throw new NotImplementedException();
        }

        public WpdFootnoteBase BuildFootNote(string noteText = null, string[] brackets = null)
        {
            throw new NotImplementedException();
        }

        public WpdFootnoteBase BuildEndNote(string noteText = null, string[] brackets = null)
        {
            throw new NotImplementedException();
        }

        public bool HasNonDefaultEndnotes()
        { // todo
            return false;
        }

        public bool HasNonDefaultFootnotes()
        {// todo
            return false;
        }

        #endregion
    }
}
