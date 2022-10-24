// OalParagraph.cs
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

using System.Collections.Generic;
using SOL;
using WpdInterfaceLib;

namespace OdtAdapterLib
{
    public class OalParagraph : IWpdParagraph
    {
        public OdtParagraph Paragraph { get; internal set; }
        public IWpdDocument Document { get; set; }

        private static Formatting _emptyFormatting = new Formatting();
        #region Implementation of IWpdParagraph

        public IWpdParagraph Append(string text)
        {
            Append(text, false, _emptyFormatting);
            return this;
        }

        public void AppendField(WpdFieldBase field)
        {
            field.Apply(this);
        }

        public void Append(string text, bool unk, Formatting formatting)
        { // todo: bold, italic as direct-applied stylings
            List<TaggedText> runs = WpdTextHelper.Split(text);

            foreach (TaggedText run in runs)
            {
                Paragraph.Append(run.Text, formatting.CharacterStyleName,
                    (formatting.Bold??false)||run.IsBold, (formatting.Italic??false)||run.IsItalic);
            }

            //Paragraph.Append(text, formatting.CharacterStyleName, formatting.Bold, formatting.Italic);
        }
        /// <summary>
        /// There is a mismatch between docx and odt, in the way these "lines" are defined.
        /// In docx it is a border applied by paragraph properties, but in odt it is
        /// done using attributes in the paragraph style.  
        /// </summary>
        /// <param name="lineType"></param>
        /// <param name="position"></param>
        public void InsertHorizontalLine(string lineType, string position = "bottom")
        { // todo: honor position; try nesting to get the same effect as on docx
            OdtDocument.LineStyle ls = ("single".Equals(lineType))
                ? OdtDocument.LineStyle.Single
                : OdtDocument.LineStyle.Double;
            Paragraph.Document.AppendHorizontalLine(ls, position, Paragraph);
        }

        public string StyleName
        {
            get => Paragraph?.Style;
            set => Paragraph.Style = value;
        }

        #endregion
    }
}
