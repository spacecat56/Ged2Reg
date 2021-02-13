
namespace WpdInterfaceLib
{
    public interface IWpdParagraph
    {
        IWpdParagraph Append(string text);
        void AppendField(IWpdFieldBase field);
        void Append(string text, bool unk, Formatting formatting);
        void InsertHorizontalLine(string lineType, string position = "bottom");
        //void Append(IWpdHyperlink h);
        string StyleName { get; set; }
    }
}