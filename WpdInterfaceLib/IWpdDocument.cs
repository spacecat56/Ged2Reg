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
    }
}