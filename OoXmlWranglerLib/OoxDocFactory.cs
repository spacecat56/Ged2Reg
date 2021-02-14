using System.IO;
using WpdInterfaceLib;

namespace DocxAdapterLib
{
    public class OoxDocFactory : IWpdFactory
    {
        #region Implementation of IWpdFactory

        public string DocType => ".docx";

        public IWpdDocument Load(Stream stream, bool editable = false)
        {
            return OoxDoc.Load(stream, editable);
        }

        public IWpdDocument Create(string filename)
        {
            return OoxDoc.Create(filename);
        }

        #endregion
    }
}
