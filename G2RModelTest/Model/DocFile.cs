using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace G2RModelTest.Model
{
    public class DocFile
    {
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
    }
}
