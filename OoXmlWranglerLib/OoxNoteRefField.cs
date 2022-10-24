// OoxNoteRefField.cs
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
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using WpdInterfaceLib;

namespace DocxAdapterLib
{
    public class OoxNoteRefField : WpdNoteRefField
    {
  
        #region Overrides of AbstractField

        public override WpdFieldBase Build()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" NOTEREF ").Append(MarkName).Append(' ');
            if (SameFormatting)
                sb.Append("\\f ");
            if (InsertRelativePosition)
                sb.Append("\\p ");
            if (InsertHyperlink)
                sb.Append("\\h ");

            FieldText = sb.ToString();
            return this;
        }

        public override void Apply(IWpdParagraph p)
        {
            Build();
            FieldHelper.ApplyField(p as OoxParagraph, this);
        }

        #endregion

        public OoxNoteRefField(OoxDoc document, XElement xml = null) : base(document, null) { }
    }

    internal class FieldHelper
    {
        internal static void ApplyField(OoxParagraph oxpara, WpdFieldBase f)
        {
            // if we try to apply the style just to the run with the number,
            // when Word executes processes like print, export, ... it 
            // actually DELETES the style and reverts the stupid thing
            // back to the paragraph's char style.  "nice", Microsoft (NOT!)
            RunProperties rp = string.IsNullOrEmpty(f.ContentStyleName)
                ? new RunProperties()
                : new RunProperties(new RunStyle() { Val = f.ContentStyleName });

            FieldCode fc = new FieldCode(f.FieldText) { Space = SpaceProcessingModeValues.Preserve };
            oxpara.Append(new Run(new FieldChar() { FieldCharType = FieldCharValues.Begin }) 
                { RunProperties = rp.Clone() as RunProperties });
            oxpara.Append(new Run(fc) { RunProperties = rp.Clone() as RunProperties });
            oxpara.Append(new Run(new FieldChar() { FieldCharType = FieldCharValues.Separate })
                { RunProperties = rp.Clone() as RunProperties });
            if (!string.IsNullOrEmpty(f.ContentText))
            {
                oxpara.Append(new Run(new Text(f.ContentText)) { RunProperties = rp.Clone() as RunProperties });
            }
            oxpara.Append(new Run(new FieldChar() { FieldCharType = FieldCharValues.End }) 
                { RunProperties = rp.Clone() as RunProperties });
        }

    }
}
