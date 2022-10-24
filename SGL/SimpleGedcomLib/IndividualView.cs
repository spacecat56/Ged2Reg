// IndividualView.cs
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
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SimpleGedcomLib
{
    public class IndividualView
    {
        public static Regex NameRex = new Regex(@"((?<given>.*?)\s)?/(?<surn>.*)/(\s*(?<suffix>\w+)[.]?)?");

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
        public string Suffix { get; set; }
        public string OptionalSuffix => string.IsNullOrEmpty(Suffix) ? null : $" {Suffix}";

        public string Name => $"{Surname}, {GivenName}{OptionalSuffix}";
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
            Suffix = m.Groups["suffix"].Value;
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