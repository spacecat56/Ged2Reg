using System;
using System.IO;
using SOL;
using WpdInterfaceLib;

namespace OdtAdapterLib
{
    public class OalDocFactory : IWpdFactory
    {
        #region Implementation of IWpdFactory
        public void Configure(bool useDomainForHyperlinkText)
        {
            OdtHyperlink.UseDomainForNullLinkText = useDomainForHyperlinkText;
        }

        public IWpdDocument Load(Stream stream, bool editable = false)
        {
            OdtDocument.ResetContext();

            // todo: is the editable flag useful / needed here?
            OdtDocument doc = OdtDocument.Load(stream);
            return new OalDocument() { Document = doc };
        }

        public IWpdDocument Create(string filename)
        {
            OdtDocument.ResetContext();

            OdtDocument doc = string.IsNullOrEmpty(filename) || !File.Exists(filename)
                ? OdtDocument.New() 
                : OdtDocument.Load(filename);
            
            return new OalDocument(){Document = doc, FileName = filename};
        }

        public string DocType => ".odt";

        #endregion
    }
}
