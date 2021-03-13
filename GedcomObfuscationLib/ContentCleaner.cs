using System.Text.RegularExpressions;

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
