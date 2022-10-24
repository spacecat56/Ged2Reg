// OoxDocFactory.cs
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

using System.IO;
using WpdInterfaceLib;

namespace DocxAdapterLib
{
    public class OoxDocFactory : IWpdFactory
    {
        #region Implementation of IWpdFactory

        public string DocType => ".docx";
        public void Configure(bool useDomainForHyperlinkText)
        {
            OoxHyperlink.UseDomainForNullLinkText = useDomainForHyperlinkText;
        }

        public IWpdDocument Load(Stream stream, bool editable = false)
        {
            return OoxDoc.Load(stream, editable);
        }

        public IWpdDocument Create(string filename)
        {
            return OoxDoc.Create(filename);
        }

        #endregion
    }
}
