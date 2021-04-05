using System;
using System.Xml.Linq;

namespace SOL
{
    public class OdtNoteRef : OdtBodyElement
    {
        public bool EnclosedInSpan { get; set; } 
        /*

        <text:note-ref text:note-class="footnote" text:ref-name="_ftn5" text:reference-format="text">6</text:note-ref>)

        */
        public OdtNoteRef Build(OdtFootnote fn)
        {
            string noteClass = fn.ContentElement.Attribute(XName.Get("note-class", OdtDocument.text))?.Value
                ?? "footnote";

            ContentElement = new XElement(XName.Get("note-ref", OdtDocument.text), fn.IdNbr);
            ContentElement.SetAttributeValue(XName.Get("note-class", OdtDocument.text), noteClass);
            ContentElement.SetAttributeValue(XName.Get("ref-name", OdtDocument.text), fn.Id);
            // number gives the formatted value? p. 649
            // not supported here; ony page, chapter, direction or text
            ContentElement.SetAttributeValue(XName.Get("reference-format", OdtDocument.text), "text");

            if ((fn.Brackets?.Length ?? 0) == 2)
            {
                XElement spanEl = new OdtSpan() { Style = OdtFootnote.NoteReferenceStyle, Document = Document, Text = fn.Brackets[0]}.Build().ContentElement;
                spanEl.Add(ContentElement);
                spanEl.Add(fn.Brackets[1]);
                ContentElement = spanEl;
                return this;  // already enclosed anyway, never mind
            }


            if (!EnclosedInSpan)
                return this;
            {
                XElement spanEl = new OdtSpan(){Style = OdtFootnote.NoteReferenceStyle, Document = Document}.Build().ContentElement;
                spanEl.Add(ContentElement);
                ContentElement = spanEl;
            }
 
            return this;
        }
        
        
        public override OdtBodyElement Build()
        {
            throw new InvalidOperationException("must be built by the note instance");
        }
    }
}
