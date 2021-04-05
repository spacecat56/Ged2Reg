using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace SimpleGedcomLib
{
    
    [DataContract]
    public class MediaCitationModel
    {
        internal static Type[] ModelTypes = new[] { typeof(CitationData), typeof(CitedPerson), typeof(CitedEvent) };

        public static MediaCitationModel Load(string path)
        {
            DataContractSerializer dcs = new DataContractSerializer(typeof(MediaCitationModel), ModelTypes);
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas()))
                {
                    MediaCitationModel rv = (MediaCitationModel)dcs.ReadObject(reader);
                    return rv;

                }
            }
        }

        public void Persist(string path)
        {
            DataContractSerializer dcs = new DataContractSerializer(typeof(MediaCitationModel));
            using (BufferedStream bs = new BufferedStream(File.Create(path)))
            {
                using (XmlDictionaryWriter xdw = XmlDictionaryWriter.CreateTextWriter(bs, Encoding.UTF8))
                {
                    dcs.WriteObject(xdw, this);
                }
            }
        }


        [DataMember]
        public List<CitationData> Citations { get; set; } = new List<CitationData>();

        [DataMember]
        public Exception LastException { get; set; }

        [DataMember]
        public bool IsValid { get; set; }


        public CitationData Find(string fullPath)
        {
            string fn = Path.GetFileName(fullPath);
            CitationData rv;
            GetMap().TryGetValue(fn ?? "nothere", out rv);
            return rv;
        }

        protected Dictionary<string, CitationData> GetMap()
        {
            if (_map != null) return _map;
            _map = new Dictionary<string, CitationData>();
            foreach (CitationData c in Citations)
            {
                _map.Add(c.MediaFileName, c);
            }
            return _map;
        }

        private Dictionary<string, CitationData> _map;


        public MediaCitationModel(GedcomFile ged)
        {
            try
            {
                foreach (MediaObjectView mov in ged.MediaObjectViews)
                {
                    Citations.Add(new CitationData(mov, ged));               
                }
                IsValid = true;
            }
            catch (Exception ex)
            {
                IsValid = false;
                LastException = ex;
            }
        }
    }

    [DataContract]
    public class CitationData
    {
        [DataMember]
        public string MediaFileName { get; set; }

        [DataMember]
        public string SourceTitle { get; set; }

        [DataMember]
        public string CitationText { get; set; }

        [DataMember]
        public List<CitedPerson> Persons { get; set; } = new List<CitedPerson>();

        /// <summary>
        /// Properties for the UI
        /// </summary>
        public string Summary => BuildSummary();
        public BindingList<CitedName> Names => BuildNames();
        public BindingList<string> Years => BuildYears();


        private BindingList<CitedName> _names;
        private BindingList<string> _years;
        private string _summary;

        public CitationData(MediaObjectView mov, GedcomFile ged)
        {
            MediaFileName = Path.GetFileName(mov.FilePath);
            SourceTitle = mov.SourceView?.Title;


            StringBuilder sbc = new StringBuilder();
            foreach (CitationView citv in mov.Citations)
            {
                if (string.IsNullOrEmpty(SourceTitle))
                    SourceTitle = citv.TheSourceView?.Title;
                // report-based version wants citation text.  
                // in this ged-derived model, that is not singular.
                // usefullness of this text is "TBD"
                if (sbc.ToString().IndexOf(citv.FullText) < 0)
                {
                    sbc.Append(citv.TheSourceView?.Title).Append(", ");
                    sbc.AppendLine(citv.FullText);
                }

                // find and represent the event/fact that this citation is linked to
                Tag fact = citv.SourceTag.ParentTag;
                if (fact == null) continue;
                CitedEvent ce = new CitedEvent(fact);

                // find the person(s) subject of the citation
                List<IndividualView> citedPersons = new List<IndividualView>();
                Tag subject = citv.SourceTag.GetAncestor(0);
                if (subject==null) continue;
                if (subject.Code == TagCode.INDI)
                {
                    IndividualView indi = ged.IndividualViews.Find(x => x.Id == subject.Id);
                    if (indi != null)
                        citedPersons.Add(indi);
                }
                else if (subject.Code == TagCode.FAM)
                {
                    FamilyView famv = ged.FamilyViews.Find(x => x.Id == subject.Id);
                    if (famv != null)
                    {
                        if (famv.Husband != null)
                            citedPersons.Add(famv.Husband);
                        if (famv.Wife != null)
                            citedPersons.Add(famv.Wife);
                    }
                }
                else
                {
                    continue; // lost here
                }

                // if we already have the cited person(s), add the fact
                // otherwise build new one(s) with the fact
                foreach (IndividualView iv in citedPersons)
                {
                    CitedPerson cp = Persons.Find(x => x.GedcomId == iv.Id);
                    if (cp == null)
                        Persons.Add(cp = new CitedPerson(iv));
                    cp.CitedEvents.Add(ce);
                }
            }

            CitationText = sbc.ToString();
        }

        public CitationData() { }

        private BindingList<CitedName> BuildNames()
        {
            if (_names != null) return _names;
            BuildViewModel();
            return _names;
        }

        private void BuildViewModel()
        {
            _names = new BindingList<CitedName>();
            List<string> xYears = new List<string>();
            if (Persons != null)
            {
                foreach (CitedPerson citedPerson in Persons)
                {
                    citedPerson.AddDetails(_names, xYears);
                }
                
            }
            xYears = xYears.Distinct().ToList();
            xYears.Sort();
            _years = new BindingList<string>(xYears);
        }

        private BindingList<string> BuildYears()
        {
            if (_years != null) return _years;
            BuildViewModel();
            return _years;
        }

        private string BuildSummary()
        {
            if (_summary != null) return _summary;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(CitationText);

            if (Persons != null)
            {
                foreach (CitedPerson person in Persons)
                {
                    person.Summarize(sb);
                }                
            }

            sb.AppendLine().AppendLine(MediaFileName);

            _summary = sb.ToString();
            return _summary;
        }

    }

    [DataContract]
    public class CitedPerson
    {
        [DataMember]
        public string GedcomId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<CitedEvent> CitedEvents { get; set; } = new List<CitedEvent>();

        public void Summarize(StringBuilder sb)
        {
            sb.AppendLine().AppendLine($"Name: {Name}");
            foreach (CitedEvent citedEvent in CitedEvents)
            {
                citedEvent.Summarize(sb);
            }
            sb.AppendLine();
        }

        public void AddDetails(ICollection<CitedName> names, ICollection<string> years)
        {
            CitedName myname = CitedName.Build(Name);
            if (myname!=null)
                names.Add(myname);
            foreach (CitedEvent citedEvent in CitedEvents)
            {
                citedEvent.AddDetails(years);
            }
        }

        public CitedPerson() { }

        public CitedPerson(IndividualView iv)
        {
            GedcomId = iv.Id;
            Name = iv.Name;
        }

    }

    public class CitedName
    {
        public string Given { get; set; }
        public string Surname { get; set; }

        private static Regex _namRegex;

        static CitedName()
        {
            _namRegex = new Regex("^(.*?),\\s*(.*)$");
        }

        public static CitedName Build(string prospect)
        {
            Match m = _namRegex.Match(prospect);
            if (!m.Success) return null;
            CitedName rv = new CitedName()
            {
                Surname = m.Groups[1].Value,
                Given = m.Groups[2].Value
            };
            return rv;
        }
    }

    [DataContract]
    public class CitedEvent
    {
        [DataMember]
        public string EventName { get; set; }

        [DataMember]
        public string EventText { get; set; }

        [DataMember]
        public string EventDate { get; set; }

        [DataMember]
        public string EventPlace { get; set; }

        private static Regex _dateRegex;

        static CitedEvent()
        {
            _dateRegex = new Regex("([12][06789][0-9][0-9])");
        }

        public CitedEvent(Tag fact)
        {
            EventName = TagMapper.Map(fact.Code).ToString();
            if (fact.Code == TagCode.EVEN)
            {
                Tag typ = fact.GetChild(TagCode.TYPE);
                if (!string.IsNullOrEmpty(typ?.Content))
                    EventName = typ.Content;
            }

            EventText = fact.Content;

            Tag tDate = fact.GetChild(TagCode.DATE);
            if (tDate != null)
                EventDate = tDate.Content;

            Tag tPlace = fact.GetChild(TagCode.PLAC);
            if (tPlace != null)
                EventPlace = tPlace.Content;
        }

        public void Summarize(StringBuilder sb)
        {
            sb.AppendLine($"    {EventName}:  {EventDate} {EventPlace} {EventText}");
        }

        public void AddDetails(ICollection<string> years)
        {
            Match m = _dateRegex.Match(EventDate ?? EventText ?? "");
            if (!m.Success) return;
            years.Add(m.Groups[1].Value); // consider: multiple?
        }
    }

}
