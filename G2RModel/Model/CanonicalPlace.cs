// CanonicalPlace.cs
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
using System.Text;

namespace Ged2Reg.Model
{
    public class CanonicalPlace
    {
        public List<string> Locality { get; set; } = new List<string>();

        public string City { get; set; }

        public string County { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public bool IsUsaPlace { get; set; }
        public string OutputOverride { get; set; }
        public bool IsAmbiguous { get; set; }

        public HashSet<string> CountyQualifierWords { get; set; } = new HashSet<string>()
        {
            "county",
            "parish",
            "municipality"
        };

        public string[] Emit(bool dropUsa, bool injectWordCounty, string nameOfUsa, FormattedPlaceName fpn)
        {
            string[] rv = new string[2];
            string sep = null;

            if (!string.IsNullOrEmpty(OutputOverride))
            {
                fpn.LongName = fpn.ShortName = rv[0] = rv[1] = OutputOverride;
                return rv;
            }

            if (IsUsaPlace)
            {
                Country = dropUsa ? null : nameOfUsa;
            }

            injectWordCounty &= !string.IsNullOrEmpty(County);
            if (injectWordCounty && County.IndexOf(' ') > 0)
            {
                string qual = County.Substring(County.LastIndexOf(' ') + 1); // we 'know' it cannot END with a ' '
                injectWordCounty &= !CountyQualifierWords.Contains(qual.ToLower());
            }
            if (injectWordCounty)
                County = $"{County} County";

            StringBuilder sb = new StringBuilder();
            
            foreach (string s in Locality)
            {
                sb.Append(sep).Append(s);
                sep = sep ?? ", ";
            }

            string[] parts = { City, County, State, Country };
            foreach (string part in parts)
            {
                if (string.IsNullOrEmpty(part?.Trim())) continue;
                sb.Append(sep).Append(part);
                sep = sep ?? ", ";
            }

            rv[0] = sb.ToString() ?? "";
            int ix;
            if ((ix = rv[0].IndexOf(',')) > 0)
            {
                rv[1] = rv[0].Substring(0, ix);
            }
            else
            {
                rv[1] = rv[0];
            }

            fpn.LongName = rv[0];
            fpn.ShortName = rv[1];
            sb.Clear();
            string conn = null;
            for (int i = parts.Length - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(parts[i])) continue;
                sb.Append(conn).Append(parts[i]);
                conn = conn ?? ":";
            }

            for (int i = Locality.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(Locality[i])) continue;
                sb.Append(conn).Append(Locality[i]);
                conn = conn ?? ":";
            }
            fpn.IndexEntry = sb.ToString();

            return rv;
        }
    }
}