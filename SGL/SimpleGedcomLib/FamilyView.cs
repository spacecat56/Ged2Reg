// FamilyView.cs
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
using System.Linq;

namespace SimpleGedcomLib
{
    public class FamilyView
    {
        private Tag _famTag;

        public Tag FamTag
        {
            get { return _famTag; }
            set
            {
                if (value == null || value.Code != TagCode.FAM || value.Level != 0)
                    throw new ArgumentException("not a level-0 Family");
                _famTag = value;
            }
        }

        public IndividualView Husband { get; set; }
        public IndividualView Wife { get; set; }
        public List<IndividualView> Chiildren { get; set; } = new List<IndividualView>();

        public string Id => _famTag?.Id;

        public FamilyView(Tag family)
        {
            FamTag = family;
        }

        public int Link(Dictionary<string, IndividualView> ivs)
        {
            int rv = 0;
            Tag t = _famTag.GetChild(TagCode.HUSB);
            IndividualView ivTemp;
            if (t?.ReferredTag != null && ivs.TryGetValue(t.ReferredTag.Id, out ivTemp))
            {
                Husband = ivTemp; //ivs.FirstOrDefault(x => x.Id == t.ReferredTag?.Id);
                rv++;
            }
            t = _famTag.GetChild(TagCode.WIFE);
            if (t?.ReferredTag != null && ivs.TryGetValue(t.ReferredTag.Id, out ivTemp))
            {
                Wife = ivTemp; //ivs.FirstOrDefault(x => x.Id == t.ReferredTag?.Id);
                rv++;
            }

            List<Tag> childs = _famTag.GetChildren(TagCode.CHIL);
            foreach (Tag child in childs)
            {
                if (child?.ReferredTag==null || !ivs.TryGetValue(child.ReferredTag.Id, out ivTemp)) continue;
                //IndividualView cq = ivs.FirstOrDefault(x => x.Id == child.ReferredTag?.Id);
                //if (cq != null)
                //    Chiildren.Add(cq);
                Chiildren.Add(ivTemp);
            }

            return rv;
        }

        public Tag GetChilTag(string child)
        {
            Tag rv = (from t in _famTag.GetChildren(TagCode.CHIL) where child.Equals(t.Content) select t)
                .FirstOrDefault();
            return rv;
        }
    }
}