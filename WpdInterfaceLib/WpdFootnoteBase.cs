using System.Collections.Generic;


namespace WpdInterfaceLib
{
    public abstract class WpdFootnoteBase
    {
        public string[] Brackets;
        public static string DefaultFootnoteStyle { get; set; }  = "FootnoteText";
        public static string DefaultFootnoteRefStyle { get; set; } = "FootnoteReference";
        public string NoteReferenceStyle { get;  set; }
        public string NoteTextStyle { get;  set; }
        public bool IsApplied { get;  set; }
        public string BookmarkName { get; set; }
        public int BookmarkId { get; set; }
        public abstract int? Id { get; }
        public List<WpdNoteFragment> Fragments { get; set; }

        public WpdFootnoteBase AppendText(string t)
        {
            (Fragments ??= new List<WpdNoteFragment>())
                .Add(new WpdNoteFragment() { Type = WpdFragmentType.Text, Content = t });
            return this;
        }

        public WpdFootnoteBase AppendNoteRef(WpdFootnoteBase other)
        {
            (Fragments ??= new List<WpdNoteFragment>())
                .Add(new WpdNoteFragment() { Type = WpdFragmentType.Noteref, DataObject = other});
            return this;
        }

        public WpdFootnoteBase AppendHyperlink(string t)
        {
            (Fragments ??= new List<WpdNoteFragment>())
                .Add(new WpdNoteFragment() { Type = WpdFragmentType.Hyperlink, Content = t });
            return this;
        }

        public abstract void Apply(IWpdParagraph bodyParaI, bool bookmarked = false);
    }
}