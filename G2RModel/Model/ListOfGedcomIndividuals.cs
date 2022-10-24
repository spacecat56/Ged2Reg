// ListOfGedcomIndividuals.cs
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
using CommonClassesLib;
using SimpleGedcomLib;

namespace Ged2Reg.Model
{
    public class ListOfGedcomIndividuals : SortableBindingList<GedcomIndividual>
    {
        public ListOfGedcomIndividuals()
        {
        }

        public ListOfGedcomIndividuals(List<IndividualView> views)
        {
            foreach (IndividualView view in views)
            {
                Add(new GedcomIndividual(){IndividualView = view});
            }
        }

        public int Locate(string c, int ix)
        {
            for (int i = ix; i < Count; i++)
            {
                if ((Items[i].Surname ?? "`").StartsWith(c)) return i;
            }

            return 0;
        }
    }
}