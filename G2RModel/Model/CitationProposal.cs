// CitationProposal.cs
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
using SimpleGedcomLib;

namespace Ged2Reg.Model
{
    public class CitationProposal
    {
        private static Dictionary<TagCode, string> _renamedEvents;
        public static Dictionary<TagCode, string> RenamedEvents
        {
            get { return _renamedEvents ??= new Dictionary<TagCode, string>(); }
            //set { _renamedEvents = value; }
        }

        static CitationProposal()
        {
            RenamedEvents.Add(TagCode.CHR, "Baptism");
        }

        public string InstanceId { get; set; }
        public EventCitations Citation { get; set; }
        public string EventId { get; set; }
        public TagCode EventTagCode { get; set; }
        public string Id => $"{EventId}{InstanceId}";

        public List<CitationProposal> ApplicableEvents { get; set; }

        public bool Matches(CitationProposal other)
        {
            return Citation?.Matches(other.Citation) ?? false;
        }

        public string AppliesTo(bool onlyIfMultiple = true)
        {
            if (onlyIfMultiple && (ApplicableEvents?.Count ?? 0) == 0)
                return null;
            StringBuilder sb = new StringBuilder();
            sb.Append("Cited for: ");
            string sep = null;
            foreach (CitationProposal other in ApplicableEvents)
            {
                sb.Append(sep).Append(GetEventName(other));
                sep = ", ";
            }
            sb.Append(sep).Append(GetEventName(this)).Append(".");
            return sb.ToString();
        }

        public static string GetEventName(CitationProposal c)
        {
            return RenamedEvents.ContainsKey(c.EventTagCode) 
            ? RenamedEvents[c.EventTagCode]
            : c.EventTagCode.Map().ToString();
        }

        public void AddApplicableEvents(CitationProposal other)
        {
            if (ApplicableEvents == null)
            {
                ApplicableEvents = new List<CitationProposal>();
            }
            if (other.ApplicableEvents != null)
                ApplicableEvents.AddRange(other.ApplicableEvents);
            ApplicableEvents.Add(other);
        }
    }
}