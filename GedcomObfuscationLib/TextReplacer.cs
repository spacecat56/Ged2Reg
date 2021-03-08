using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GedcomObfuscationLib
{
    public class TextReplacer
    {
        public static int Next;
        public static string NextInterValue => $"@@@Interval{Next++:00000000}";


        public string Input { get; set; }
        public string Output { get; set; }
        public string Intermediate { get; set; } = NextInterValue;
        public int Length => (Input ?? "").Length;

        /// <summary>
        /// Phase 1 Apply, requires use of Regex to avoid
        /// hitting innocent substrings of other words
        /// </summary>
        /// <param name="s"></param>
        public string Apply(string s)
        {
            Regex r = new Regex($"(?i)\\b(?<hit>{Input})\\b");
            string sr = r.Replace(s, Intermediate);
            return sr;
        }

        public void Apply(StringBuilder sb)
        {
            sb.Replace(Intermediate, Output);
        }
    }
}
