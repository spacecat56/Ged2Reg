﻿using System.Collections.Generic;
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