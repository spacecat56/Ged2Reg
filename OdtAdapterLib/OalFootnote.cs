using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SOL;
using WpdInterfaceLib;

namespace OdtAdapterLib
{
    public class OalFootnote : WpdFootnoteBase
    {
        public OdtDocument Document { get; set; }

        internal OdtFootnote Footnote { get; set; }

        public OalFootnote(string textPart = null)
        {
            if (textPart == null)
                return;

            AppendText(textPart);
        }

        #region Overrides of WpdFootnoteBase

        public override int? Id => Footnote?.IdNbr;

        public override void Apply(IWpdParagraph bodyParaI, bool bookmarked = false)
        {
            OdtBodyElement el = (bodyParaI as OalParagraph)?.Paragraph;
            
            Footnote = new OdtFootnote() { Document = Document, Brackets = Brackets};
            Footnote.Build();
            foreach (WpdNoteFragment fragment in this.Fragments)
            {
                switch (fragment.Type)
                {
                    case WpdFragmentType.Text:
                        Footnote.Append(new OdtSpan(){Text = fragment.Content}.Build());
                        break;
                    case WpdFragmentType.Noteref:
                        var frref = (fragment.DataObject as OalFootnote)?.Footnote?.GetNoteRef;
                        if (frref == null) break;
                        Footnote.Append(frref);
                        break;
                    case WpdFragmentType.Hyperlink:
                        Footnote.Append(new OdtHyperlink()
                        {
                            Document = Document,
                            URL = fragment.Content
                        }.Build());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            //bodyParaI.Append();

            Footnote.ApplyTo(el);
        }

        #endregion
    }
}
