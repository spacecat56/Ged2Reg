using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WpdInterfaceLib
{
    [Flags]
    public enum RunType
    {
        Plain = 0,
        Bold = 1,
        Italic = 2,
    }
    public class WpdTextHelper
    {
        public static Regex TextTagRegex = new Regex(@"(?i)([<][/]?[bi][>])");
        public static List<TaggedText> Split(string t)
        {
            List<TaggedText> rvl = new List<TaggedText>();
            TaggedText current = new TaggedText();
            if (string.IsNullOrEmpty(t))
            {
                current.Text = t;
                rvl.Add(current);
                return rvl;
            }

            MatchCollection matches = TextTagRegex.Matches(t);
            if (matches.Count == 0)
            {
                rvl.Add(current);
                current.Text = t;
                return rvl;
            }

            // it's like a very simple state machine, 
            // where the state is represented in current.CharFormat, plus the charPos
            int charPos = 0;
            foreach (Match match in matches)
            {
                int tagPos = match.Index;
                string tag = match.Value.ToLower();
                // when we hit a tag, whatever precedes it is distinctly formatted
                if (tagPos > charPos)
                {
                    current.Text = t.Substring(charPos, tagPos - charPos);
                    rvl.Add(current);
                    current = new TaggedText(){CharFormat = current.CharFormat};
                }

                // whatever the next text is, it begins after the tag
                charPos = match.Index + match.Length;

                // apply or remove the tag's effect (does not recognize invalid nesting)
                bool closure = tag.Contains('/');
                RunType impact = tag.Contains('b') ? RunType.Bold : RunType.Italic;
                if (closure)
                    current.CharFormat &= ~impact;
                else
                    current.CharFormat |= impact;
            }
            // if there is any remaining text, capture it
            if (charPos < t.Length - 1)
            {
                current.Text = t.Substring(charPos);
                rvl.Add(current);
            }

            return rvl;
        }
    }

    public class TaggedText
    {
        public string Text { get; set; }
        public RunType CharFormat { get; set; } = RunType.Plain;
        public bool IsBold => (CharFormat & RunType.Bold) == RunType.Bold;
        public bool IsItalic => (CharFormat & RunType.Italic) == RunType.Italic;
        public bool IsPlain => CharFormat == RunType.Plain;

        public string StartTag { get; set; }
        public string EndTag { get; set; }

    }
}
