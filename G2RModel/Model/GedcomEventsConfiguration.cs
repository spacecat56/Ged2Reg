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
