// ReportStats.cs
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

namespace Ged2Reg.Model
{
    public class ReportStats
    {
        public TimeSpan PrepTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan ReportTime => EndTime.Subtract(StartTime);
        public int MainPerson { get; set; }
        public int MainSpouse { get; set; }
        public int NonContinuedPerson { get; set; }
        public int NonContinuedSpouses { get; set; }
        public int Citations { get; set; }
        public int DistinctCitations { get; set; }
        public int SpouseParents { get; set; }
        public int MaybeLiving { get; set; }
        public int NameIndexEntries { get; set; }
        public int PlaceIndexEntries { get; set; }

        public int UncitedEvents { get; set; }
        public int CitesBeyondLimit { get; set; }

        public ReportStats Init(TimeSpan prep)
        {
            PrepTime = prep;
            StartTime = DateTime.Now;
            return this;
        }
    }
}