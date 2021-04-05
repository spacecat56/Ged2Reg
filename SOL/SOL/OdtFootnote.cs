using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SOL
{
    public class OdtFootnote : OdtBodyElement
    {
        public static string NoteReferenceStyle { get; set; } = "FootnoteReference";
        public static string NoteTextStyle { get; set; } = "FootnoteText";

        private static int _noteNbr;
        public static int NextNoteNumber => ++_noteNbr;

        internal static void Reset()
        {
            _noteNbr = 0;
        }

        public static string NoteIdFormat = "_ftn{0}";

        private XElement _insertPoint;

        private OdtNoteRef _noteRef;

        public override XElement ContentInsertPoint => _insertPoint;

        public string NoteClassName { get; set; } = "footnote";

        public static string BuildNoteId(int id) => string.Format(NoteIdFormat, id);

        public OdtNoteRef GetNoteRef => _noteRef ??= new OdtNoteRef(){Document = Document}.Build(this);

        /*

        Footnote with bookmark and hyperlink

        <text:bookmark-start text:name="_Ref64000640"/>
	        <text:span text:style-name="FootnoteReference">
	        <text:note text:note-class="footnote" text:id="_ftn0">
		        <text:note-citation>1</text:note-citation>
		        <text:note-body>
			        <text:p text:style-name="FootnoteText">		
				        <text:s/><text:span text:style-name="T9">Penelope L. Stratton and Henry B. Hoff</text:span>
				        <text:span text:style-name="T10">… See<text:s/></text:span>
				        <text:a xlink:href="http://www.google.com" office:target-frame-name="_top" xlink:show="replace"><text:span text:style-name="T11">http://www.google.com</text:span></text:a>
			        </text:p>
		        </text:note-body>
	        </text:note>
	        </text:span>
        <text:bookmark-end text:name="_Ref64000640"/>
        */

        internal const string Note = "note";
        internal const string NoteCitation = "note-citation";
        internal const string NoteBody = "note-body";
        internal const string NoteClass = "note-class";

        public string[] Brackets { get; set; }
        public int IdNbr { get; private set; }
        public string Id { get; private set; }

        // to-do--done: whack this, it is not the right stuff
        //public bool Bookmarked { get; set; }
        //public OdtBookMark BookMark { get; private set; }

        public bool IsBracketed => (Brackets?.Length ?? 0) == 2;

        #region Overrides of OdtBodyElement

        public override OdtBodyElement Build()
        {
            IdNbr = NextNoteNumber;
            Id = BuildNoteId(IdNbr);

            string ns = OdtNames.NamespaceManager.LookupNamespace(OdtNames.Text);

            ContentElement = new XElement(XName.Get(OdtNames.Span, ns));
            ContentElement.SetAttributeValue(XName.Get(OdtNames.Style, ns), NoteReferenceStyle);

            if (IsBracketed)
            {
                ContentElement.Add(Brackets[0]);
                //ContentElement.Add(new OdtNoteRef() { Document = Document }.Build(this).ContentElement);
            }

            XElement noteElement = new XElement(XName.Get(Note, ns));
            noteElement.SetAttributeValue(XName.Get(NoteClass, ns), NoteClassName);
            noteElement.SetAttributeValue(XName.Get(OdtNames.Id, ns), Id);
            //if (!IsBracketed)
                noteElement.Add(new XElement(XName.Get(NoteCitation, ns), IdNbr));
            ContentElement.Add(noteElement);
            if (IsBracketed)
                ContentElement.Add(Brackets[1]);

            XElement notebodyElement = new XElement(XName.Get(NoteBody, ns));
            noteElement.Add(notebodyElement);

            _insertPoint = new XElement(XName.Get(OdtNames.Paragraph, ns), 
                new XAttribute(XName.Get(OdtNames.Style, ns), NoteTextStyle),
                new XElement(XName.Get("s", OdtNames.NamespaceManager.LookupNamespace("text"))));


            notebodyElement.Add(_insertPoint);

            return this;
        }
        
        #endregion

        public static void Configure(OdtDocument doc, bool endnotes, string[] brackets)
        {
            XElement cfgEl = doc.TheStylesPart.GetNotesConfiguration(endnotes);
            if (cfgEl == null) return;

            //if (endnotes)
            //{
            //    cfgEl.SetAttributeValue(XName.Get("footnotes-position", OdtDocument.text), "section");
            //}

            if (brackets == null || brackets.Length != 2)
                return;

            cfgEl.SetAttributeValue(XName.Get("num-prefix", OdtDocument.style), brackets[0]);
            cfgEl.SetAttributeValue(XName.Get("num-suffix", OdtDocument.style), brackets[1]);

        }
    }
}
