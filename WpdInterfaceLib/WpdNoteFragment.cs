namespace WpdInterfaceLib
{
    public enum WpdFragmentType
    {
        Text,
        Noteref,
        Hyperlink
    }
    public class WpdNoteFragment
    {
        public WpdFragmentType Type { get; set; }
        public string Content { get; set; }
        public object DataObject { get; set; }
    }
}