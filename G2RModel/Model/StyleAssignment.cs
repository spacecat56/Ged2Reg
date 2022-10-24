// StyleAssignment.cs
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

using CommonClassesLib;

namespace Ged2Reg.Model
{

    public enum StyleSlots
    {
        Normal,
        MainPerson,
        MainPersonText,
        BodyTextIndent, 
        KidsIntro,
        Kids,
        ChildName,
        KidMoreText,
        Grandkids,
        GrandkidName,
        GenerationNumber,
        KidsAlt,
        GenerationDivider,
        GenerationDivider3Plus,
        BodyTextNotes
    }
    public class ListOfStyleAssignments : SortableBindingList<StyleAssignment> { }

    public class StyleAssignment  : AbstractBindableModel
    {
        private StyleSlots _slot;
        private string _styleName;

        public StyleAssignment() { }


        public StyleAssignment(StyleSlots slot)
        {
            _slot = slot;
            _styleName = slot.ToString();
        }

        public StyleSlots Style
        {
            get { return _slot; }
            set { _slot = value; OnPropertyChanged(); }
        }

        public string StyleName
        {
            get { return _styleName; }
            set { _styleName = value; OnPropertyChanged(); }
        }

    }
}