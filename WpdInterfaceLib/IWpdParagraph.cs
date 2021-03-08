
namespace WpdInterfaceLib
{
    public interface IWpdParagraph
    {
        public IWpdDocument Document { get; set; }
        IWpdParagraph Append(string text);
        void AppendField(WpdFieldBase field);
        void Append(string text, bool unk, Formatting formatting);
        void InsertHorizontalLine(string lineType, string position = "bottom");
        //void Append(IWpdHyperlink h);
        string StyleName { get; set; }
    }
}