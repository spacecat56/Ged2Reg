using System;
using System.Collections.Generic;
using System.IO;
//
namespace WpdInterfaceLib
{
    public interface IWpdDocument : IDisposable
    {
        void Save();
        void ApplyTemplateStyles(Stream stream, bool fonts);
        bool SetCoreProperty(string propertyName, string propertyValue);
        void Apply(WpdPageSettings ps);
        void InsertPageBreak();
         List<StyleInfo> ListStyles();
        //void Apply(FootnoteEndnoteType fe);

        // factory methods are required to participate
        IWpdParagraph InsertParagraph(string text = null);
        WpdNoteRefField BuildNoteRef();
        WpdIndexField BuildIndexField();
        WpdIndexEntry BuildIndexEntryField(string indexName, string indexValue);
        WpdFootnoteBase BuildFootNote(string noteText = null, string[] brackets = null);
        WpdFootnoteBase BuildEndNote(string noteText = null, string[] brackets = null); // Endnote must be a subclass of Footnote
        bool HasNonDefaultEndnotes();
        bool HasNonDefaultFootnotes();
    }
}