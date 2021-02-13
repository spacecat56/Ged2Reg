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
        IWpdParagraph InsertParagraph(string text = null);
        List<StyleInfo> ListStyles();
        //void Apply(FootnoteEndnoteType fe);
    }
}