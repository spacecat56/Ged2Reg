// GedcomEventsConfiguration.cs
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
using System.Text;
using System.Threading.Tasks;
using SimpleGedcomLib;

namespace G2RModel.Model
{
    public enum EventGroup
    {
        None,
        Residence,
        LDS,
        Popular,
        Custom
        
    }

    public enum EventContent
    {
        Unclassified,
        Dated,
        Undated
    }

    public class GedcomEvent
    {
        public TagCode Tag { get; set; }
        public bool Selected { get; set; }
        public bool EmitMultiple { get; set; }
        public EventGroup Group { get; set; }
        public EventContent Content { get; set; }
        public string CustomGroup { get; set; }

        public string Decsription => Tag.Map().ToString();



    }

    public class GedcomEventGroup
    {

    }

    public class GedcomEventsConfiguration
    {

    }
}
