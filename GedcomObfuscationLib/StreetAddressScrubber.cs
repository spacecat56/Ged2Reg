// StreetAddressScrubber.cs
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
