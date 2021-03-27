using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GedcomObfuscationLib
{
    public class StreetAddressScrubber
    {

        private static Dictionary<string, string> _assigned = new Dictionary<string, string>();

        public static void Reset() { _assigned.Clear(); _rand = new Random(DateTime.Now.Millisecond);}

        private static Regex _streetNameScrubber
        = new Regex(@"^(?<prefix>\d [A-Z]{4} .*?)(?<street>\b(?<nbr>\d{1,3}) (?<name>([A-Za-z]+[ ]?){1,2}) (?<type>(Street|St|Avenue|Ave|Av|Blvd|Boulevard|Pl)\b))");

        private static Random _rand = new Random(DateTime.Now.Millisecond);

        public static string[] MyNewStreets { get; set; } =
        {
            "Easy",
            "West",
            "North",
            "Hidden",
            "Lost",
            "Forgotten",
            "Park",
            "Laurel",
            "Maple",
            "Oak",
            "Sunset",
            "Lake"
        };

        public static string Apply(string s)
        {
            if ((s ?? string.Empty).Length < 10) return s;

            Match m = _streetNameScrubber.Match(s);
            if (!(m.Success && m.Groups["street"].Success)) return s;
            Group g = m.Groups["street"];

            if (!_assigned.TryGetValue(m.Groups["name"].Value, out string ss))
            {
                ss = MyNewStreets[_rand.Next(0, MyNewStreets.Length - 1)];
               _assigned.Add(m.Groups["name"].Value, ss);
            }

            string number = _rand.Next(10, 99).ToString();
            ss = $"{number} {ss} {m.Groups["type"].Value}";
 
            StringBuilder sb = new StringBuilder(s);
            sb.Replace(g.Value, ss, g.Index, g.Length);
            return sb.ToString();
        }

    }
}
