using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GedcomObfuscationLib
{
    public class ContentCleaner
    {
        Regex _urlRex = new Regex(@"(?i)\b(?<root>https?://.*?[.][a-z]+)/\S+\b");
        public string PruneURLs(string text)
        {
            string rv = _urlRex.Replace(text, "${root}");
            return rv;
        }
    }
}
