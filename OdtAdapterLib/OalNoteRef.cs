using SOL;
using WpdInterfaceLib;

namespace OdtAdapterLib
{
    public class OalNoteRef : WpdNoteRefField
    {

        public OalNoteRef(IWpdDocument document, WpdFootnoteBase fn) : base(document, fn) { }

        #region Overrides of WpdFieldBase

        public override WpdFieldBase Build()
        {
            return this;
        }

        public override void Apply(IWpdParagraph p)
        {
            OdtNoteRef x = new OdtNoteRef(){EnclosedInSpan = true, Document = (Document as OalDocument)?.Document}.Build((this.Footnote as OalFootnote).Footnote);
            (p as OalParagraph)?.Paragraph.Append(x);
        }

        #endregion
    }
}
