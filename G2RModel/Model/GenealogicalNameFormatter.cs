// GenealogicalNameFormatter.cs
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
using Ged2Reg.Model;

namespace G2RModel.Model
{
    public class GenealogicalNameFormatter
    {
        public static bool DownshiftAllCaps { get; set; }
        public static bool HandleUnknownNames { get; set; }
        public static string UnknownIn { get; set; }
        public static string UnknownOut { get; set; }
        public static TextFixer NameFixer { get; set; }

        public static void SetPolicies(bool down, bool unks, string unkIn, string unkOut)
        {
            DownshiftAllCaps = down;
            HandleUnknownNames = unks;
            UnknownIn = unkIn;
            UnknownOut = unkOut;
        }

        public static Regex NameSplitter { get; } = new Regex(@"(?i)((?<givn>.*?)\s)?/(?<surn>.*)/");
        public static Regex RexNameWords { get; } = new Regex(@"(?i)(\b(?<givn>\w+)\b)");
        public static Regex RexMac { get; } = new Regex(@"(?i)((?<mc>m(a)?c\s?)(?<base>\w+)\b)");
        public static Regex RexNotMac { get; } = new Regex(@"(?i)\b(mack|mackey|macias)\b");
        public static HashSet<string> SpecialSurnameParts { get; set; } = new HashSet<string>()
        {
            "ST",
            "SAINT",
        };

        public bool NoSurname { get; }
        public bool UnknownGivenName { get; private set; }
        public bool UnknownSurname { get; private set; }
        public bool IsMacName { get; private set; }
        public bool UnknownName => UnknownGivenName && UnknownSurname;

        public string RawName { get;  }
        public string RawSurname { get;  }
        public string RawGivenName { get;  }
        public string Givn { get; }
        public string Surn { get; }
        public bool CapsSurn { get; }
        public bool CapsGivn { get; }
        public string GivenNames { get; private set; }
        public string Surname { get; private set; }


        public GenealogicalNameFormatter(string rn, string rg, string rs)
        {
            RawGivenName = rg;
            RawSurname = rs;
            RawName = rn;

            if (!string.IsNullOrEmpty(RawName))
            {
                var match = NameSplitter.Match(RawName);
                if (match.Success)
                {
                    if (string.IsNullOrEmpty(RawGivenName) && match.Groups["givn"].Success)
                        Givn = match.Groups["givn"].Value;
                    else
                        Givn = RawGivenName;
                    if (string.IsNullOrEmpty(RawSurname) && match.Groups["surn"].Success)
                        Surn = match.Groups["surn"].Value;
                    else
                        Surn = RawSurname;
                }
            }
            else
            {
                Givn = RawGivenName;
                Surn = RawSurname;
            }

            CapsSurn = Surn?.IsAllUpper() ?? false;
            CapsGivn = Givn?.IsAllUpper() ?? false;

            NoSurname = string.IsNullOrEmpty(Surn) && (rn??"").Trim().EndsWith("//");
            IsMacName = !string.IsNullOrEmpty(Surn) 
                        && RexMac.IsMatch(Surn) 
                        && !RexNotMac.IsMatch(Surn);
        }

        public static GenealogicalNameFormatter Reformat(string rawName, string givn, string surn)
        {
            GenealogicalNameFormatter rv = new GenealogicalNameFormatter(rawName, givn, surn);

            rv.Reformat();

            return rv;
        }

        public GenealogicalNameFormatter Reformat()
        {
            // the settings, injected into statics, amy change
            // we recompute the properties that may change as a result
            UnknownGivenName = HandleUnknownNames && (string.IsNullOrEmpty(Givn) || Givn == UnknownIn);
            UnknownSurname = HandleUnknownNames && Surn == UnknownIn;

            GivenNames = UnknownGivenName ? UnknownOut : Givn;
            Surname = UnknownSurname ? UnknownOut : Surn;

            if (NameFixer != null)
            {
                try
                {
                    Surname = NameFixer.Exec(Surname);
                    GivenNames = NameFixer.Exec(GivenNames);
                }
                catch { }
            }

            if (!DownshiftAllCaps)
                return this;

            if (CapsGivn) GivenNames = NameShift(GivenNames, false);
            if (IsMacName)
                Surname = MacShift(Surname);
            else
                if (CapsSurn) Surname = NameShift(Surname, true);

            return this;
        }

        public static string MacShift(string surname)
        {
            if (string.IsNullOrEmpty(surname))
                return surname; // GIGO

            Match m = RexMac.Match(surname);
            if (!m.Success)
                return surname;

            StringBuilder sb = new StringBuilder(surname.ToLower());
            sb[m.Groups["mc"].Index] = char.ToUpper(sb[m.Groups["mc"].Index]);
            sb[m.Groups["base"].Index] = char.ToUpper(sb[m.Groups["base"].Index]);
            return sb.ToString();
        }

        public static string NameShift(string n, bool asSurname)
        {
            if (string.IsNullOrEmpty(n) || n.Length < 2)
                return n;



            MatchCollection matches = RexNameWords.Matches(n);
            if (matches.Count == 0) // huh?
                return n;

            StringBuilder sb = new StringBuilder(n.ToLower());
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                if (asSurname && i < matches.Count - 1 && !SpecialSurnameParts.Contains(match.Value))
                    continue;
                sb[match.Index] = Char.ToUpper(sb[match.Index]);
            }

            return sb.ToString();
        }
    }

 }
