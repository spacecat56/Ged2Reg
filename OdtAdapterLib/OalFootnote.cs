﻿using System;
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
            
            Footnote = new OdtFootnote() { Document = Document};
            Footnote.Build();
            foreach (WpdNoteFragment fragment in this.Fragments)
            {
                switch (fragment.Type)
                {
                    case WpdFragmentType.Text:
                        Footnote.Append(new OdtSpan(){Text = fragment.Content}.Build());
                        break;
                    case WpdFragmentType.Noteref:
                        // todo
                        break;
                    case WpdFragmentType.Hyperlink:
                        // todo
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
