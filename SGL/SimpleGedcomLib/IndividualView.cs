using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SimpleGedcomLib
{
    public class IndividualView
    {
        public static Regex NameRex = new Regex(@"(?<given>.*?)\s/(?<surn>.*)/");

        private Tag _indiTag;
        public Tag IndiTag
        {
            get { return _indiTag; }
            set
            {
                if (value == null || value.Code != TagCode.INDI || value.Level != 0)
                    throw new ArgumentException("not a level-0 Individual");
                _indiTag = value;
            }
        }

        public string GivenName { get; set; }
        public string Surname { get; set; }

        public string Name => $"{Surname}, {GivenName}";
        public string Id => _indiTag?.Id;
        public List<Tag> Notes { get; set; } = new List<Tag>();

        public IndividualView(Tag tag)
        {
            IndiTag = tag;
            Tag n = IndiTag.GetChild(TagCode.NAME);
            if (n == null) return;

            Match m = NameRex.Match(n.Content ?? "");
            if (!m.Success) return;
            GivenName = m.Groups["given"].Value;
            Surname = m.Groups["surn"].Value;
        }

        public void Link(Dictionary<string, Tag> notesMap)
        {
            foreach (Tag nref in _indiTag.GetChildren(TagCode.NOTE))
            {
                if (notesMap.TryGetValue(nref.Content, out Tag nn))
                    Notes.Add(nn);
            }
        }
    }
}