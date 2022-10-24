// TextReplacer.cs
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
