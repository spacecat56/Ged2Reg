using System.IO;

namespace WpdInterfaceLib
{
    public interface IWpdFactory
    {
        IWpdDocument Load(Stream stream, bool editable = false);
        IWpdDocument Create(string filename);
        string DocType { get; }
    }
}
