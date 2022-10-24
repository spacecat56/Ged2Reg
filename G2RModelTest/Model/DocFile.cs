// DocFile.cs
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
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace G2RModelTest.Model
{
    public class DocFile
    {
        private string _mainTextPlainText;
        public string FullText { get; set; } = "";
        public Dictionary<string, string> PartTexts { get; set; } = new Dictionary<string, string>();
        public bool IsDocX { get; private set; }

        public string MainText => IsDocX ? PartTexts["document.xml"] : PartTexts["content.xml"];

        public DocFile Init(string path)
        {
            
            if (!File.Exists(path))
                return this;

            IsDocX = path.ToLower().EndsWith(".docx");

            StringBuilder sb = new StringBuilder();
            using (ZipArchive archive = ZipFile.OpenRead(path))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (!entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)) continue;
                    using TextReader tr = new StreamReader(entry.Open());
                    string txt = tr.ReadToEnd();
                    sb.Append(txt);
                    PartTexts.Add(entry.Name, txt);
                }
            }

            FullText = sb.ToString();

            return this;
        }

        public string MainTextPlainText()
        {
            return _mainTextPlainText ??= XDocument.Parse(MainText).Root?.Value ?? "";
        }
    }
}
