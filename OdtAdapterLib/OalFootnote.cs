// OalFootnote.cs
// Copyright 2022 Thomas W. Shanley
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
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
            
            // todo: this is a problem iff null here && doing endnotes
            Footnote ??= new OdtFootnote() { Document = Document, Brackets = Brackets};
            Footnote.Build();
            foreach (WpdNoteFragment fragment in this.Fragments)
            {
                switch (fragment.Type)
                {
                    case WpdFragmentType.Text:
                        List<TaggedText> runs = WpdTextHelper.Split(fragment.Content);
                        // todo: this gets pretty deep into the internals of the 
                        // library "below" and SB handled down there
                        foreach (TaggedText run in runs)
                        {
                            if (run.IsPlain)
                            {
                                Footnote.Append(new OdtSpan() { Text = run.Text }.Build());
                                continue;
                            }
                            StyleConfiguration sc = new StyleConfiguration()
                            {
                                //BasedOn = style,
                                Bold = run.IsBold,
                                Italic = run.IsItalic
                            };
                            string style = StylesManager.Instance.GetStyle(sc);
                            //Footnote.Append(run.Text, formatting.CharacterStyleName,
                            //    (formatting.Bold ?? false) || run.IsBold, (formatting.Italic ?? false) || run.IsItalic);
                            Footnote.Append(new OdtSpan() { Text = run.Text, Style = style, Document = Document}.Build());
                        }
                        //Footnote.Append(new OdtSpan() { Text = fragment.Content }.Build());
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
