using System;
using System.Collections.Generic;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace OoXmlWranglerLib
{
    public class OoxFootnote 
    {
        /*
         *
         *
        a complete end/footnote entry is a distinct structure that looks like this (with optional added []s)

        <w:footnote w:id="1">
            <w:p>
                <w:pPr><w:pStyle w:val="FootnoteText"/></w:pPr>
                <w:r>
                    <w:rPr><w:rStyle w:val="FootnoteReference"/></w:rPr>
                    <w:t>[</w:t>
                    <w:footnoteRef/>
                    <w:t>]</w:t>
                 </w:r>
                <w:r><w:t xml:space="preserve"> This is my footnote.</w:t></w:r>
            </w:p>
        </w:footnote>

        and a reference is a run (with added []s) that looks like this

        <w:r>
            <w:rPr>
                <w:rStyle w:val="FootnoteReference" />
            </w:rPr>
            <w:t>[</w:t>
            <w:footnoteReference w:id="2" />
            <w:t>]</w:t>
        </w:r>

         *
         *
         */

        public static RunStyle FootnoteRefstyle { get; set; } = new RunStyle(){Val = "FootnoteReference" };
        public VerticalTextAlignment FootnoteRefFormat { get; set; } = new VerticalTextAlignment { Val = VerticalPositionValues.Superscript };

        public enum FragmentType
        {
            Text,
            Noteref,
            Hyperlink
        }

        internal class Fragment
        {
            public FragmentType Type { get; set; }
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
        internal List<Fragment> Fragments { get; set; }

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
                (Fragments = new List<Fragment>()).Add(new Fragment() { Content = text, Type = FragmentType.Text });
            if (pBrackets == null) return;
            if (pBrackets.Length != 2)
                throw new ArgumentException("brackets parameter must be null or two elements");
            brackets = pBrackets;
        }

        public OoxFootnote AppendText(string t)
        {
            (Fragments ??= new List<Fragment>())
                .Add(new Fragment() { Type = FragmentType.Text, Content = t });
            return this;
        }

        public OoxFootnote AppendNoteRef(OoxFootnote other)
        {
            (Fragments ?? (Fragments = new List<Fragment>()))
                .Add(new Fragment() { Type = FragmentType.Noteref, DataObject = other});
            return this;
        }

        public OoxFootnote AppendHyperlink(string t)
        {
            (Fragments ?? (Fragments = new List<Fragment>()))
                .Add(new Fragment() { Type = FragmentType.Hyperlink, Content = t });
            return this;
        }

        internal virtual void AssignNextId()
        {
            id = (doc.MaxFootnoteId() + 1);
        }

        //internal virtual bool ApplyToDocument()
        //{
        //    //Xml = noteElement;
        //    //return doc.AppendFootnote(noteElement);
        //    return doc.Apply(this);
        //}

        public void Apply(OoxParagraph bodyPara, bool bookmarked = false)
        {
            if (IsApplied)
            {
                throw new InvalidOperationException("note has already been applied");
            }
            if (Fragments?[0] == null)
            {
                throw new InvalidOperationException("note has no content");
            }

            AssignNextId();

            // todo: rework, abstraction needed

            FootnoteEndnoteType f = CreateNoteInstance();
            OoxParagraph footPara = new OoxParagraph(f.AppendChild(new Paragraph()));
            footPara.StyleName = NoteTextStyle;
            footPara.Append(BuildNoteRefMark());
            //Paragraph footPara = f.AppendChild(new Paragraph(new Run(new FootnoteReferenceMark()) { RunProperties = new RunProperties((RunStyle) FootnoteRefstyle.Clone()) }));
            // make sure there is separation between the fn# and the contents
            //Run r = null;
            string space = (Fragments[0].Content ?? "").StartsWith(" ") ? "" : " ";
            foreach (Fragment fragment in Fragments)
            {
                switch (fragment.Type)
                {
                    case FragmentType.Text:
                        footPara.Append(new Run(new Text(space+fragment.Content) { Space = SpaceProcessingModeValues.Preserve }));
                        space = "";
                        break;
                    case FragmentType.Noteref:
                        // insert a bookmark reference back to another foot/endnote
                        // this is limited to the note number, because any other text included 
                        // with it will be deleted if/when Word renumbers the notes, e.g. on <Ctrl>A, F9
                        if ((ReferenceNote ??= fragment.DataObject as OoxFootnote)?.BookmarkName == null)
                            break;
                        NoteRefField nrf = new NoteRefField(doc, null) { MarkName = ReferenceNote.BookmarkName, ContentText = $"{ReferenceNote.Id}", InsertHyperlink = true };
                        nrf.Apply(footPara);
                        break;
                    case FragmentType.Hyperlink:
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
            //doc._footnotesPart.Footnotes.AppendChild(f);

            IsApplied = true;

            //ApplyInline(bodyPara, bookmarked);
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

        internal virtual OoxHyperlink BuildHyperlink(Fragment fragment)
        {
            //Hyperlink h = doc.AddHyperlinkToFootnotes(fragment.Content, new Uri(fragment.Content));
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
            // todo: brackets
            //Run run = p.AppendChild(new Run());
            //run.AppendChild(new Text("Create text in body - CreateWordprocessingDocument"));
            //para.AppendChild(new Run(fr) { RunProperties = new RunProperties(footRefFormat) });

            FootnoteReference fr = (new FootnoteReference() { Id = id });
            Run r = new Run(fr) { RunProperties = new RunProperties(FootnoteRefFormat.Clone() as RunStyle) };
            
            p.MyParagraph.AppendChild(r);


            //// create the reference element...
            //noteRefElement = new XElement(OoxDoc.w  + "r",
            //    new XElement(OoxDoc.w  + "rPr",
            //        new XElement(OoxDoc.w  + "rStyle", new XAttribute(OoxDoc.w  + "val", NoteReferenceStyle))));
            //// ... optionally wrapped in brackets (choose when needed to distinguish footnotes from exponents etc.)
            //if (brackets != null)
            //    noteRefElement.Add(new XElement(OoxDoc.w  + "t", brackets[0]));
            //// optionally wrapped in a bookmark marker
            //if (bookmarked)
            //{
            //    BookmarkId = NextBookmarkId;
            //    BookmarkName = string.Format(BookmarkNamePattern, BookmarkId);
            //    XElement wBookmarkStart = new XElement(
            //        XName.Get("bookmarkStart", OoxDoc.w .NamespaceName),
            //        new XAttribute(XName.Get("id", OoxDoc.w .NamespaceName), BookmarkId),
            //        new XAttribute(XName.Get("name", OoxDoc.w .NamespaceName), BookmarkName));
            //    noteRefElement.Add(wBookmarkStart);
            //}

            //noteRefElement.Add(new XElement(OoxDoc.w  + NoteReferenceNode, new XAttribute(OoxDoc.w  + "id", id)));
            //if (bookmarked)
            //{
            //    XElement wBookmarkEnd = new XElement(
            //        XName.Get("bookmarkEnd", OoxDoc.w .NamespaceName),
            //        new XAttribute(XName.Get("id", OoxDoc.w .NamespaceName), BookmarkId),
            //        new XAttribute(XName.Get("name", OoxDoc.w .NamespaceName), BookmarkName));
            //    noteRefElement.Add(wBookmarkEnd);
            //}

            //if (brackets != null)
            //    noteRefElement.Add(new XElement(OoxDoc.w  + "t", brackets[1]));

            //// append the reference run to the paragraph
            //p.Xml.Add(noteRefElement);
        }
    }
}
