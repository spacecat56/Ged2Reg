// ReportEntryFactory.cs
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
    public class ReportEntryFactory
    {
        private static ReportEntryFactory _instance;

        /// <summary>
        /// "Somehow", state such as the ancestornamelist is surviving from one
        /// run to the next... this needs to be tracked down and fixed...
        /// </summary>
        /// <param name="unique"></param>
        /// <returns></returns>
        public static ReportEntryFactory Init(bool unique = false)
        {
            return _instance = new ReportEntryFactory(unique);
        }

        public static ReportEntryFactory Instance => _instance;

        private ReportEntryFactory(bool unique)
        {
            Unique = unique;
        }

        public ReportEntry GetReportEntry(GedcomIndividual indi)
        {
            if (!Unique)
                return new ReportEntry(indi);
            if (KnownEntries.TryGetValue(indi, out ReportEntry rv))
                return rv;
            rv = new ReportEntry(indi);
            KnownEntries.Add(indi, rv);
            return rv;
        }

        public ReportFamilyEntry GetReportFamily(GedcomFamily family)
        {
            if (family == null) return null;
            if (!Unique)
                return new ReportFamilyEntry(family);
            if (KnownFamilies.TryGetValue(family, out ReportFamilyEntry rv))
                return rv;
            rv = new ReportFamilyEntry(family);
            KnownFamilies.Add(family, rv);
            return rv;
        }

        public List<ReportEntry> GetParents(GedcomIndividual indi)
        {
            List<ReportEntry> rvl = new List<ReportEntry>();
            if (indi.ChildhoodFamily == null)
                indi.FindFamilies(false);
            if (indi.ChildhoodFamily == null)
                return rvl;
            if (indi.ChildhoodFamily.Husband != null)
                rvl.Add(GetReportEntry(indi.ChildhoodFamily.Husband));
            if (indi.ChildhoodFamily.Wife != null)
                rvl.Add(GetReportEntry(indi.ChildhoodFamily.Wife));
            return rvl;
        }


        //private Dictionary<BigInteger, ReportEntry> KnownMainNumbers = new Dictionary<BigInteger, ReportEntry>();
        private Dictionary<GedcomIndividual, ReportEntry> KnownEntries = new Dictionary<GedcomIndividual, ReportEntry>();
        private Dictionary<GedcomFamily, ReportFamilyEntry> KnownFamilies = new Dictionary<GedcomFamily, ReportFamilyEntry>();
        public bool Unique { get; }

        /// <summary>
        /// "Somehow", state such as the ancestor name list is surviving from one
        /// run to the next... this needs to be tracked down and fixed...
        /// </summary>
        public static void Reset()
        {
            Init(); // other code suggests this is not enough...
        }
    }
}