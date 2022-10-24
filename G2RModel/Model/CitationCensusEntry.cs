// CitationCensusEntry.cs
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

using SimpleGedcomLib;

namespace Ged2Reg.Model
{
    public class CitationCensusEntry
    {
        public TagCode TagCode { get; set; }
        public int Occurrences { get; set; }
        public int HaveCites { get; set; }
        public int HaveMultipleCites { get; set; }
        public int HaveSoleCite => HaveCites - HaveMultipleCites;
        public int Uncited => Occurrences - HaveCites;
        public int TotalCites { get; set; }
        public int CvSpread { get; set; }
        public int CitationSelected { get; set; }

        public void Count(EventCitations ec)
        {
            if (ec.TagCode != TagCode) return;
            Occurrences++;
            if (ec.DistinctCitations.Count > 0)
                HaveCites++;
            if (ec.DistinctCitations.Count > 1)
                HaveMultipleCites++;
            TotalCites += ec.DistinctCitations.Count;
            foreach (DistinctCitation dc in ec.DistinctCitations)
            {
                CvSpread += dc.CitationViews.Count;
            }

            if (ec.SelectedItem != null)
                CitationSelected++;
        }
    }
}