// OalDocFactory.cs
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
using System.IO;
using SOL;
using WpdInterfaceLib;

namespace OdtAdapterLib
{
    public class OalDocFactory : IWpdFactory
    {
        #region Implementation of IWpdFactory
        public void Configure(bool useDomainForHyperlinkText)
        {
            OdtHyperlink.UseDomainForNullLinkText = useDomainForHyperlinkText;
        }

        public IWpdDocument Load(Stream stream, bool editable = false)
        {
            OdtDocument.ResetContext();

            // todo: is the editable flag useful / needed here?
            OdtDocument doc = OdtDocument.Load(stream);
            return new OalDocument() { Document = doc };
        }

        public IWpdDocument Create(string filename)
        {
            OdtDocument.ResetContext();

            OdtDocument doc = string.IsNullOrEmpty(filename) || !File.Exists(filename)
                ? OdtDocument.New() 
                : OdtDocument.Load(filename);
            
            return new OalDocument(){Document = doc, FileName = filename};
        }

        public string DocType => ".odt";

        #endregion
    }
}
