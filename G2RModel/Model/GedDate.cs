// GedDate.cs
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

using System.Text.RegularExpressions;

namespace Ged2Reg.Model
{
    public static class GedDate
    {
        private static readonly Regex _rexYear = new Regex("(?<year>\\d{4})");
        private static readonly Regex _rexYear6 = new Regex("(?<year>\\d{4}([/]\\d)?)");
        private static readonly Regex _rexYearOnly = new Regex(@"\A(?<year>\d{1,4})(?:\s*(AD|BC|BCE|CE))?\z");
        private static readonly Regex _rexYearEra = new Regex(@"(?<year>\d{1,4})(?:\s*(AD|BC|BCE|CE))");
        private static readonly Regex _rexMonYearEra = new Regex(@"(?:[A-Z]+)\s+(?<year>\d{1,4})(?:\s*(AD|BC|BCE|CE))?");
        public static string ExtractYear(string dat, bool allow6 = false, string defaultVal = "")
        {
            string rv = defaultVal;
            if (string.IsNullOrEmpty(dat))
                return rv;

            Match m = (allow6 ? _rexYear6 : _rexYear).Match(dat);
            if (!m.Success)
                m = _rexYearOnly.Match(dat);
            if (!m.Success)
                m = _rexYearEra.Match(dat);
            if (!m.Success)
                m = _rexMonYearEra.Match(dat);

            if (m.Success)
                rv = m.Groups["year"].Value;

            return rv;
        }
    }
}
