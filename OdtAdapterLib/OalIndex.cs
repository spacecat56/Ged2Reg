// OalIndex.cs
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

using SOL;
using WpdInterfaceLib;

namespace OdtAdapterLib
{
    class OalIndex : WpdIndexField
    {
        private bool _didBuild;
        public OdtIndex Index { get; private set; }

        public OalIndex(IWpdDocument document) : base(document)
        {
            SingleIndexOnly = true;
        }

        #region Overrides of WpdFieldBase

        public override WpdFieldBase Build()
        {
            if (_didBuild) return this;
            _didBuild = true;
            Index = new OdtIndex(){Columns = Columns, Heading = Heading, Placeholder = UpdateFieldPrompt};
            Index.Build();
            return this;
        }

        public override void Apply(IWpdParagraph p)
        {
            Build();
            (p as OalParagraph)?.Paragraph.Document.Append(Index);
        }

        #endregion
    }
}
