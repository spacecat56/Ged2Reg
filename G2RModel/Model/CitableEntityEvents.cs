// CitableEntityEvents.cs
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
using System.Linq;
using SimpleGedcomLib;

namespace Ged2Reg.Model
{
    public class CitableEntityEvents
    {
        public bool IsEmpty => (CitationsByEvent?.Count ?? 0) == 0;
        public Dictionary<Tag, EventCitations> CitationsByEvent { get; set; } = new Dictionary<Tag, EventCitations>();

        public int Undecided => (from ec in CitationsByEvent.Values where ec.SelectedItem == null select ec).Count();

        public void Cite(Tag tag, DistinctCitation distinctCitations)
        {
            if (!CitationsByEvent.TryGetValue(tag, out EventCitations eCites))
                CitationsByEvent.Add(tag, eCites = new EventCitations() {Tag = tag}.Append(distinctCitations));
            else
                eCites.Append(distinctCitations);
        }

        public int SelectAnySoleCitations()
        {
            int rv = 0;

            foreach (EventCitations eventCitations in CitationsByEvent.Values)
            {
                if (eventCitations.DistinctCitations.Count != 1) continue;
                rv++;
                eventCitations.Select(eventCitations.DistinctCitations[0]);
            }

            return rv;
        }

        public void Census(CitationCensusEntry cce)
        {
            EventCitations ec = CitationsByEvent.Values.FirstOrDefault(e => e.TagCode == cce.TagCode);
            if (ec == null) return;
            cce.Count(ec);
        }

        public int Select(DistinctCitation choice)
        {
            int rv = 0;
            foreach (EventCitations eventCitations in CitationsByEvent.Values)
            {
                if (eventCitations.Select(choice))
                    rv++;
            }

            return rv;
        }

        public EventCitations Find(Tag evnt)
        {
            if (evnt == null) return null;
            CitationsByEvent.TryGetValue(evnt, out EventCitations ec);
            return ec;
        }
    }

    public class CitablePersonEvents : CitableEntityEvents
    {
        public CitablePersonEvents() { }
        public GedcomIndividual Individual { get; set; }

    }

    public class CitableMarriageEvents : CitableEntityEvents
    {
        public GedcomFamily Family { get; set; }

    }
}