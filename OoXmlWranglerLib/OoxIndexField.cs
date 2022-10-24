// OoxIndexField.cs
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

using System.Text;
using System.Xml.Linq;
using WpdInterfaceLib;

namespace DocxAdapterLib
{
    /// <summary>
    /// Class to produce the XML for an index field.
    /// See ECMA-376-1:2016 / Office Open XML File Formats — Fundamentals and Markup Language Reference / October 2016, pages 1220 - 1222
    ///
    /// NB several of the less-common options may not have been tested
    /// 
    /// </summary>
    public class OoxIndexField : WpdIndexField
    {
        public string Bookmark { get; set; }
        public string SequencePageSeparator { get; set; }
        public string PageRangeSeparator { get; set; }
        public string LetterHeading { get; set; }
        public string XrefSeparator { get; set; }
        public string PagePageSeparator { get; set; }
        public string LetterRange { get; set; }
        public bool RunSubentries { get; set; }
        public string SequenceName { get; set; }
        public bool EnableYomi { get; set; }
        public string LanguageId { get; set; }



        public OoxIndexField(OoxDoc document, XElement xml = null) : base(document)
        {
            SingleIndexOnly = false;
        }

        #region Overrides of AbstractField

        public override WpdFieldBase Build()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(" INDEX ");
            AppendNonEmpty(sb, "b", Bookmark);
            if (Columns > 0)
                AppendNonEmpty(sb, "c", Columns.ToString());
            AppendNonEmpty(sb, "d", SequencePageSeparator);
            AppendNonEmpty(sb, "e", EntryPageSeparator);
            AppendNonEmpty(sb, "f", IndexName);
            AppendNonEmpty(sb, "g", PageRangeSeparator);
            AppendNonEmpty(sb, "h", LetterHeading);
            AppendNonEmpty(sb, "k", XrefSeparator);
            AppendNonEmpty(sb, "l", PagePageSeparator);
            AppendNonEmpty(sb, "p", LetterRange);
            if (RunSubentries) sb.Append("\\r ");
            AppendNonEmpty(sb, "s", SequenceName);
            if (EnableYomi) sb.Append("\\y ");
            AppendNonEmpty(sb, "z", LanguageId);

            //Xml = Build(sb.ToString(), UpdateFieldPrompt);
            FieldText = sb.ToString();
            ContentText = UpdateFieldPrompt;
            return this;
        }

        public override void Apply(IWpdParagraph p)
        {
            Build();
            // this flavor does not have a built-in heading, so we emit it into the 
            // given paragraph
            p.Append(Heading);
            // the we need another paragraph
            var p2 = (Document as OoxDoc)?.InsertParagraph();
            FieldHelper.ApplyField((p2??p) as OoxParagraph, this);
        }

        private void AppendNonEmpty(StringBuilder sb, string field, string fieldArg)
        {
            if (string.IsNullOrEmpty(fieldArg)) return;
            // we always leave a trailing space, needed to separate from the field end mark
            sb.Append($"\\{field} \"{fieldArg}\" ");
        }

        #endregion
    }
}