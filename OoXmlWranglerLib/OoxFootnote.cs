using System;
using System.Collections.Generic;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace WpdInterfaceLib
{
    public class OoxFootnote 
    {

        public static RunStyle FootnoteRefstyle { get; set; } = new RunStyle(){Val = "FootnoteReference" };
        public VerticalTextAlignment FootnoteRefFormat { get; set; } = new VerticalTextAlignment { Val = VerticalPositionValues.Superscript };

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

        public static string BookmarkNamePattern = "_RefN{0}";
 
        public static string DefaultFootnoteStyle { get; set; }  = "FootnoteText";
        public static string DefaultFootnoteRefStyle { get; set; } = "FootnoteReference";

        public string NoteReferenceStyle { get; internal set; }
        public string NoteTextStyle { get; internal set; }
        public string NoteReferenceNode { get; internal set; }
        public string NoteRefNode { get; internal set; }
        public string NoteNode { get; internal set; }
        public bool IsApplied { get; internal set; }
        public int? Id => IsApplied ? id : (int?) null;
        public string BookmarkName { get; set; }
        public int BookmarkId { get; set; }
        public OoxFootnote ReferenceNote { get; set; }
        internal List<WpdNoteFragment> Fragments { get; set; }

        internal OoxDoc doc;
        internal string[] brackets;
        internal XElement noteElement;
        internal XElement noteRefElement;

        // Paragraph implementation of bookmarks is broken in the branch this is based on
        // we can't reverse engineer any code out of the Xceed/master branch, due to its more
        // restrictive license
        // so, we need to keep our bookmarkid counter for notes and "hope" there will be no collisions
        internal static int _nextBookmarkId = 0;
        public static int NextBookmarkId => _nextBookmarkId++;
        private int _id;

        internal int id
        {
            get => _id;
            set
            {
                if (_id == 0)
                    _id = value;
                else
                    throw new InvalidOperationException("footnote id is immutable once set");
            }
        }

        public OoxFootnote(OoxDoc document, string noteText = null, string[] brackets = null) 
        {
            NoteReferenceStyle = DefaultFootnoteRefStyle;
            NoteTextStyle = DefaultFootnoteStyle;
            NoteReferenceNode = "footnoteReference";
            NoteRefNode = "footnoteRef";
            NoteNode = "footnote";

            Init(document, noteText, brackets);
        }

        internal void Init(OoxDoc document, string text, string[] pBrackets)
        {
            doc = document;
            if (!string.IsNullOrEmpty(text))
                (Fragments = new List<WpdNoteFragment>()).Add(new WpdNoteFragment() { Content = text, Type = WpdFragmentType.Text });
            if (pBrackets == null) return;
            if (pBrackets.Length != 2)
                throw new ArgumentException("brackets parameter must be null or two elements");
            brackets = pBrackets;
        }

        public OoxFootnote AppendText(string t)
        {
            (Fragments ??= new List<WpdNoteFragment>())
                .Add(new WpdNoteFragment() { Type = WpdFragmentType.Text, Content = t });
            return this;
        }

        public OoxFootnote AppendNoteRef(OoxFootnote other)
        {
            (Fragments ??= new List<WpdNoteFragment>())
                .Add(new WpdNoteFragment() { Type = WpdFragmentType.Noteref, DataObject = other});
            return this;
        }

        public OoxFootnote AppendHyperlink(string t)
        {
            (Fragments ??= new List<WpdNoteFragment>())
                .Add(new WpdNoteFragment() { Type = WpdFragmentType.Hyperlink, Content = t });
            return this;
        }

        internal virtual void AssignNextId()
        {
            id = (doc.MaxFootnoteId() + 1);
        }

        public void Apply(IWpdParagraph bodyParaI, bool bookmarked = false)
        {
            if (IsApplied)
            {
                throw new InvalidOperationException("note has already been applied");
            }
            if (Fragments?[0] == null)
            {
                throw new InvalidOperationException("note has no content");
            }

            OoxParagraph bodyPara = bodyParaI as OoxParagraph;

            AssignNextId();

            // todo: rework, abstraction needed

            FootnoteEndnoteType f = CreateNoteInstance();
            OoxParagraph footPara = new OoxParagraph(f.AppendChild(new Paragraph()));
            footPara.StyleName = NoteTextStyle;
            footPara.Append(BuildNoteRefMark());
            string space = (Fragments[0].Content ?? "").StartsWith(" ") ? "" : " ";
            foreach (WpdNoteFragment fragment in Fragments)
            {
                switch (fragment.Type)
                {
                    case WpdFragmentType.Text:
                        footPara.Append(new Run(new Text(space+fragment.Content) { Space = SpaceProcessingModeValues.Preserve }));
                        space = "";
                        break;
                    case WpdFragmentType.Noteref:
                        // insert a bookmark reference back to another foot/endnote
                        // this is limited to the note number, because any other text included 
                        // with it will be deleted if/when Word renumbers the notes, e.g. on <Ctrl>A, F9
                        if ((ReferenceNote ??= fragment.DataObject as OoxFootnote)?.BookmarkName == null)
                            break;
                        NoteRefField nrf = new NoteRefField(doc, null) { MarkName = ReferenceNote.BookmarkName, ContentText = $"{ReferenceNote.Id}", InsertHyperlink = true };
                        nrf.Apply(footPara);
                        break;
                    case WpdFragmentType.Hyperlink:
                        try
                        {
                            BuildHyperlink(fragment).Apply(footPara);
                        }
                        catch (UriFormatException ufx)
                        {
                            // giving up on it
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            doc.Apply(f);
            IsApplied = true;

            FootnoteEndnoteReferenceType footnoteReference = CreateNoteReference();
            VerticalTextAlignment footRefFormat = new VerticalTextAlignment { Val = VerticalPositionValues.Superscript };
            Run footnoteReferenceRun = new Run() { RunProperties = new RunProperties(footRefFormat) };
            if (brackets != null)
                footnoteReferenceRun.AppendChild(new Text(brackets[0]));
            footnoteReferenceRun.AppendChild(footnoteReference);
            if (brackets != null)
                footnoteReferenceRun.AppendChild(new Text(brackets[1]));
            if (bookmarked)
            {
                BookmarkId = NextBookmarkId;
                BookmarkName = string.Format(BookmarkNamePattern, BookmarkId);
                bodyPara.MyParagraph.AppendChild(new BookmarkStart(){Id = BookmarkId.ToString(), Name = BookmarkName});
            }
            bodyPara.MyParagraph.AppendChild(footnoteReferenceRun);
            if (bookmarked)
            {
                bodyPara.MyParagraph.AppendChild(new BookmarkEnd(){ Id = BookmarkId.ToString()});
            }
        }

        internal virtual Run BuildNoteRefMark()
        {
            return new Run(new FootnoteReferenceMark()) { RunProperties = new RunProperties((RunStyle)FootnoteRefstyle.Clone()) };
        }

        internal virtual FootnoteEndnoteReferenceType CreateNoteReference()
        {
            return (new FootnoteReference() { Id = id });
        }

        internal virtual FootnoteEndnoteType CreateNoteInstance()
        {
            return new Footnote() { Id = id };
        }

        internal virtual OoxHyperlink BuildHyperlink(WpdNoteFragment fragment)
        {
            OoxHyperlink h = new OoxHyperlink()
            {
                Anchor = fragment.Content,
                Uri = new Uri(fragment.Content),
                Doc = doc,
                Location = OoxHyperlink.LinkLocation.Footnote
            };
            return h;
        }

        internal void ApplyInline(OoxParagraph p, bool bookmarked)
        {
            FootnoteReference fr = (new FootnoteReference() { Id = id });
            Run r = new Run(fr) { RunProperties = new RunProperties(FootnoteRefFormat.Clone() as RunStyle) };
            
            p.MyParagraph.AppendChild(r);
        }
    }
}
