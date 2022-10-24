// OoxIndexEntry.cs
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

using WpdInterfaceLib;

namespace DocxAdapterLib
{
    public class OoxIndexEntry : WpdIndexEntry
    {
        public OoxIndexEntry(IWpdDocument document) : base(document) { }

        #region Overrides of AbstractField

        public override WpdFieldBase Build()
        {
            // build the contents of the field
            string fieldContents = $" XE \"{IndexValue}\" ";
            if (!string.IsNullOrWhiteSpace(SeeInstead))
                fieldContents = $"{fieldContents}\\t \"See {SeeInstead}\" ";
            if (!string.IsNullOrWhiteSpace(IndexName))
                fieldContents = $"{fieldContents}\\f \"{IndexName}\" ";

            // wrap it in the field delimiters
            //Xml = Build(fieldContents);
            FieldText = fieldContents;
            return this;
        }

        public override void Apply(IWpdParagraph p)
        {
            Build();
            FieldHelper.ApplyField(p as OoxParagraph, this);
        }

        #endregion
    }
}
