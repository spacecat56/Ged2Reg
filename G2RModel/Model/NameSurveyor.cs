// NameSurveyor.cs
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

using System.Collections.Generic;
using Ged2Reg.Model;

namespace G2RModel.Model
{
    public static class NameSurveyor
    {
        private static HashSet<string> _knownNames;

        public static void Survey(IEnumerable<GedcomIndividual> individuals, bool reset = true)
        {
            if (reset || _knownNames == null) _knownNames = new HashSet<string>();

            foreach (GedcomIndividual individual in individuals)
            {
                if (!_knownNames.Contains(individual.Surname))
                    _knownNames.Add(individual.Surname);
                string fno = FirstNameOnly(individual.GivenName);
                if (fno != null && !_knownNames.Contains(fno))
                    _knownNames.Add(fno);
            }
        }

        private static string FirstNameOnly(string s)
        {
            s = s?.Trim();
            if (string.IsNullOrEmpty(s)) return s;
            int ix = s.IndexOf(' ');
            return ix < 0 ? s : s.Substring(0, ix);
        }

        public static bool IsKnown(string nameQ)
        {
            return _knownNames.Contains(nameQ);
        }
    }
}
