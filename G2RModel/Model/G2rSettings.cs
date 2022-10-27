using CommonClassesLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Win32;

//using System.Windows.Forms;

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
    }

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

    public class ListOfStyleAssignments : SortableBindingList<StyleAssignment> { }

    //[DataContract]
    public class ListOfSettingsSets : List<G2RSettings>
    {
        public const string DefaultSetName = "DefaultSettings";
        public ListOfSettingsSets Save(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = GetPath();
            }

            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            SimpleSerializer<ListOfSettingsSets> ser = new SimpleSerializer<ListOfSettingsSets>();
            ser.Persist(path, this);
            return this;
        }
        public static ListOfSettingsSets Load(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = GetPath();
            }

            if (!File.Exists(path))
            {
                ListOfSettingsSets rv0 = new ListOfSettingsSets();
                try
                {
                    G2RSettings rvi = G2RSettings.Load();
                    rvi.SetName = DefaultSetName;
                    rv0.Add(rvi);
                }
                catch (FileNotFoundException)
                {
                    G2RSettings rvi = new G2RSettings() { SetName = DefaultSetName }.Init(); ;
                    rv0.Add(rvi);
                }

                rv0.Save();
                return rv0;
            }
            SimpleSerializer<ListOfSettingsSets> ser = new SimpleSerializer<ListOfSettingsSets>();
            ListOfSettingsSets rv = ser.Load(path);
            return rv;
        }

        public G2RSettings GetSettings(string name = DefaultSetName, G2RSettings cloneFrom = null)
        {
            G2RSettings rv = Find(s => name.Equals(s.SetName));
            if (rv != null) return rv.InitInternals();
            rv = new G2RSettings(){SetName = name}.Init();
            if (cloneFrom != null)
            {
                // todo: clone settings
            }
            Add(rv);
            return rv;
        }

        private static string GetPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"West Leitrim Software\Ged2Reg\Ged2RegSettingsSets.xml");
        }

        public void Update(G2RSettings settings)
        {
            G2RSettings victim = Find(s => settings.SetName.Equals(s.SetName));
            if (victim != null)
                Remove(victim);
            Add(settings);
        }
    }

    [DataContract]
    public class G2RSettings : AbstractBindableModel
    {


        public G2RSettings Save(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = GetPath();
            }
            SimpleSerializer<G2RSettings> ser = new SimpleSerializer<G2RSettings>();
            ser.Persist(path, this);
            return this;
        }
        public static G2RSettings Load(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = GetPath();
            }
            if (!File.Exists(path))
                throw new FileNotFoundException();
            SimpleSerializer<G2RSettings> ser = new SimpleSerializer<G2RSettings>();
            G2RSettings rv = ser.Load(path);
            rv.InitInternals();
            return rv;
        }
        
        private static string GetPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"West Leitrim Software\Ged2Reg\Ged2RegSettings.xml");
        }

        public G2RSettings TestInit()
        {
            Init();
            OutFile = @"test_g2r.docx";
            GedcomFile = @"D:\projects\TestData\gedcom\Shanley-Ticehurst tree 2018-private_2018-09-18.ged";
            TextCleaners = new ListOfTextCleanerEntry()
            {
                new TextCleanerEntry()
                {
                    Context = TextCleanerContext.Everywhere,
                    FirstUseUnchanged = true,
                    ReplaceEntire = true,
                    Input = "The Whitney Family of Connecticut And Its Affiliations",
                    Output = "Whitney Family"
                },
                new TextCleanerEntry()
                {
                    Context = TextCleanerContext.SeeNote,
                    FirstUseUnchanged = true,
                    Input = "History and Genealogy of the Families of Old Fairfield",
                    Output = "Families of Old Fairfield"
                },
                new TextCleanerEntry()
                {
                    Context = TextCleanerContext.OthersList,
                    FirstUseUnchanged = true,
                    Input = "History and Genealogy of the Families of Old Fairfield",
                    Output = "FoF"
                },
                new TextCleanerEntry()
                {
                    Context = TextCleanerContext.FullCitation,
                    FirstUseUnchanged = true,
                    Input = "History and Genealogy of the Families of Old Fairfield",
                    Output = "Families of Old Fairfield"
                },
                new TextCleanerEntry()
                {
                    Context = TextCleanerContext.Everywhere,
                    FirstUseUnchanged = false,
                    Input = "United States Federal Census",
                    Output = "US Census"
                }, 
            };
            return this;
        }

        private string _stylesFile;
        private string _outfile;
        private string _gedcomFile;
        private ListOfStyleAssignments _styleMap;
        private bool _appendGrandkids = false;
        
        private bool _obscureLiving = true;
        private int _livingFence = 99;
        private bool _omitLivingCitations;

        private bool _includeBurial = true;
        private bool _omitBurialDate = true;
        private BaptismOptions _baptismOption = BaptismOptions.Always;
        private bool _includeFactDescriptions = true;
        private bool _reduceContinuedChildren = true;
        private bool _minimizeContinuedChildren;
        private bool _reducePlaceNames = true;
        private int _generations = 5;

        private CitationStrategy _citationStrategy = CitationStrategy.PriorityDriven;
        private CitationStrategy _fillStrategy = CitationStrategy.LeastOftenUsed;
        private string _priorities = "certificate\r\nwill\r\nprobate\r\n(deed)|(mortgage)\r\n";
        private string _antiPriorities = "public member tree\r\n(one world tree)|(OneWorldTree)\r\n";
        private bool _omitCitesOnContinued = true;
        private bool _brackets = true;
        private bool _summarize = true;
        private int _numberCitesToSummarize = 2;
        private bool _useSeeNote = true;
        private bool _asEndnotes;

        private bool _mainPersonNotes;
        private bool _spousesNotes;
        private string _noteIntro = "Additional information for {0}:";
        private bool _introBold;
        private bool _introItalic;
        private bool _convertDividers;
        private bool _noteDivider;

        private bool _oncePer = true;
        private bool _dropUsa = true;
        private bool _injectCounty = true;

        private bool _unknowns = true;
        private string _unkInSource = "_";
        private string _unkInReport = "(Unknown)";

        private string _lastPersonId;
        private string _lastPersonFile;

        private float _pageW = 8.5f;
        private float _pageH = 11f;
        private float _marginL = 1.5f;
        private float _marginR = 1.5f;
        private float _marginT = 1.3f;
        private float _marginB = 1.2f;

        private bool _debug;

        //private string _nameIndexHeading = "Index of Names";
        //private string _placeIndexHeading = "Index of Places";
        //private bool _nameIndex = true;
        //private bool _placeIndex;
        //private string _nameIndexName = "names";
        //private string _placeIndexName = "places";

        private IndexSettings _nameIndexSettings;

        private IndexSettings _placeIndexSettings;

        // todo: dersialization will not init (some of) these
        public void DefaultNameIndex()
        {
            NameIndexSettings = new IndexSettings()
            {
                Columns = 2,
                Enabled = true,
                IndexHeading = "Index of Names",
                IndexName = "names",
                Role = IndexRole.Places,
                Separator = "\t"
            };
        }
        public void DefaultPlaceIndex()
        {
            PlaceIndexSettings = new IndexSettings()
            {
                Columns = 2,
                Enabled = true,
                IndexHeading = "Index of Places",
                IndexName = "places",
                Role = IndexRole.Places,
                Separator = "\t"
            };
        }

        [DataMember]
        public float PageW
        {
            get { return _pageW; }
            set { _pageW = value; OnPropertyChanged(); }
        }

        [DataMember]
        public float PageH
        {
            get { return _pageH; }
            set { _pageH = value; OnPropertyChanged(); }
        }

        [DataMember]
        public float MarginL
        {
            get { return _marginL; }
            set { _marginL = value; OnPropertyChanged(); }
        }

        [DataMember]
        public float MarginR
        {
            get { return _marginR; }
            set { _marginR = value; OnPropertyChanged(); }
        }

        [DataMember]
        public float MarginT
        {
            get { return _marginT; }
            set { _marginT = value; OnPropertyChanged(); }
        }

        [DataMember]
        public float MarginB
        {
            get { return _marginB; }
            set { _marginB = value; OnPropertyChanged(); }
        }



        public G2RSettings()
        {
            // initialize to push the calculated value to boundary year
            LivingFence = 99;
            InitInternals();
            DefaultNameIndex();
            DefaultPlaceIndex();
        }

        [DataMember]
        public string SetName { get; set; }

        [DataMember]
        public ListOfTextCleanerEntry TextCleaners { get; set; }

        private TextCleaner _cleaner;

        // this does not de/serialize readily because of parent back-ref; it will rebuild from the list on use
        [IgnoreDataMember] 
        public TextCleaner CitationTitleCleaner
        {
            get { return _cleaner ?? (_cleaner = new TextCleaner(TextCleaners)); }
            set { _cleaner = value; }
        }

        [DataMember]
        public bool DebuggingOutput
        {
            get { return _debug; }
            set { _debug = value; OnPropertyChanged(); }
        }


        [DataMember]
        public List<ContentReformatter> DateRules { get; set; }

        public Stream GetStylesStream()
        {
            if (!string.IsNullOrEmpty(StylesFile) && File.Exists(StylesFile))
            {
                return File.Open(StylesFile, FileMode.Open);
            }
            string[] resNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            string rn = resNames.FirstOrDefault(s => s.Contains("stylesFile.docx"));
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(rn);
        }
        [DataMember]
        public bool HandleUnknownNames
        {
            get { return _unknowns; }
            set
            {
                _unknowns = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public string UnknownInSource
        {
            get { return _unkInSource; }
            set
            {
                _unkInSource = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public string UnknownInReport
        {
            get { return _unkInReport; }
            set
            {
                _unkInReport = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public CitationStrategy CitationStrategy
        {
            get { return _citationStrategy; }
            set
            {
                _citationStrategy = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public CitationStrategy FillinCitationStrategy
        {
            get { return _fillStrategy; }
            set
            {
                _fillStrategy = value;
                OnPropertyChanged();
            }
        }

        public bool Citations => CitationStrategy != CitationStrategy.None;

        [DataMember]
        public bool MainPersonNotes
        {
            get { return _mainPersonNotes; }
            set
            {
                _mainPersonNotes = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public bool SpousesNotes
        {
            get { return _spousesNotes; }
            set
            {
                _spousesNotes = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public string NoteIntro
        {
            get { return _noteIntro; }
            set
            {
                _noteIntro = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public bool IntroBold
        {
            get { return _introBold; }
            set
            {
                _introBold = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public bool IntroItalic
        {
            get { return _introItalic; }
            set
            {
                _introItalic = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public bool NotesDividers
        {
            get { return _noteDivider; }
            set
            {
                _noteDivider = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public bool ConvertDividers
        {
            get { return _convertDividers; }
            set
            {
                _convertDividers = value;
                OnPropertyChanged();
            }
        }


        //private string _citationStructure = $"";

        //public string CitationStructure
        //{
        //    get { return _citationStructure; }
        //    set
        //    {
        //        _citationStructure = value;
        //        OnPropertyChanged();
        //    }
        //}

        public void Reset()
        {
            _citeShortFormatter = _citeFormatter = null;
        }

        private CitationFormatter _citeShortFormatter;
        private CitationFormatter _citeFormatter;

        public CitationFormatter CitationShortFormatter => _citeShortFormatter
               ?? (_citeShortFormatter = new CitationFormatter() 
                   { Parts = new List<CitationPart>(CitationPartsSeeNote), OperationContext = TextCleanerContext.SeeNote});

        public CitationFormatter CitationFormatter => _citeFormatter
               ?? (_citeFormatter = new CitationFormatter() 
                   { Parts = new List<CitationPart>(CitationPartsFull), OperationContext = TextCleanerContext.FullCitation});

        //private ListOfCitationParts _citationPartsFull = new ListOfCitationParts()
        //{
        //    new CitationPart() {Name = CitationPartName.Source_AUTH, FormatString = "{0}, "},
        //    new CitationPart() {Name = CitationPartName.Source_TITL, FormatString = "{0}: "},
        //    new CitationPart() {Name = CitationPartName.Citation_PAGE, FormatString = "{0}, "},
        //    new CitationPart() {Name = CitationPartName.Citation_DATA_TEXT, FormatString = "{0}.", NullValue = "."},
        //    new CitationPart() {Name = CitationPartName.Citation_URL, FormatString = " [Online (may require membership) at: {0}.]", IsActive = true},
        //}.AutoSequence();

        private ListOfCitationParts _citationPartsFull = new ListOfCitationParts()
        {
            new CitationPart() {Name = CitationPartName.Source_AUTH, FormatString = "{0}", TrailingSeparator = ", "},
            new CitationPart() {Name = CitationPartName.Source_TITL, FormatString = "{0}", TrailingSeparator = ": "},
            new CitationPart() {Name = CitationPartName.Citation_PAGE, FormatString = "{0}", TrailingSeparator = ", "},
            new CitationPart() {Name = CitationPartName.Citation_DATA_TEXT, FormatString = "{0}"},
            new CitationPart() {Name = CitationPartName.LiteralOnly, FormatString = "."},
            new CitationPart() {Name = CitationPartName.Citation_URL, FormatString = " [Online (may require membership) at: {0}.]", IsActive = true},
        }.AutoSequence();


        [DataMember]
        public ListOfCitationParts CitationPartsFull
        {
            get => _citationPartsFull;
            set
            {
                _citationPartsFull = value;
                OnPropertyChanged();
            }
        }

        private ListOfCitationParts _citationPartsSeeNote = new ListOfCitationParts()
        {
            //new CitationPart() {Name = CitationPartName.Source_AUTH, FormatString = "{0}, "},
            new CitationPart() {Name = CitationPartName.Source_TITL, FormatString = "{0} ", Sequence = 1},
            //new CitationPart() {Name = CitationPartName.Citation_PAGE, FormatString = "{0}, "},
            //new CitationPart() {Name = CitationPartName.Citation_DATA_TEXT, FormatString = "{0}.", NullValue = "."},
            //new CitationPart() {Name = CitationPartName.Citation_URL, FormatString = ""},
        };

        private string _author;

        [DataMember]
        public string Author
        {
            get => _author;
            set
            {
                _author = value;
                OnPropertyChanged();
            }
        }
        private string _title;

        [DataMember]
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }


        [DataMember]
        public ListOfCitationParts CitationPartsSeeNote
        {
            get => _citationPartsSeeNote;
            set
            {
                _citationPartsSeeNote = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public string SourcePriorities
        {
            get { return _priorities; }
            set { _priorities = value; OnPropertyChanged(); }
        }

        [DataMember]
        public string SourceAnitPriorities
        {
            get { return _antiPriorities; }
            set { _antiPriorities = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool Brackets
        {
            get { return _brackets; }
            set { _brackets = value; OnPropertyChanged(); }
        }

        public string[] BracketArray { get; set; }

        [DataMember]
        public bool SummarizeAdditionalCitations
        {
            get { return _summarize; }
            set { _summarize = value; OnPropertyChanged(); }
        }

        [DataMember]
        public int NumberCitesToSumamrize
        {
            get { return _numberCitesToSummarize; }
            set { _numberCitesToSummarize = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool UseSeeNote
        {
            get { return _useSeeNote; }
            set { _useSeeNote = value; OnPropertyChanged(); }
        }
        private bool _repeatInline;

        [DataMember]
        public bool RepeatNoteRefInline
        {
            get { return _repeatInline; }
            set { _repeatInline = value; OnPropertyChanged(); }
        }

        public bool BookMarkNotes => UseSeeNote || RepeatNoteRefInline;

        [DataMember]
        public bool OmitCitesOnContinued
        {
            get { return _omitCitesOnContinued; }
            set { _omitCitesOnContinued = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool AsEndnotes
        {
            get { return _asEndnotes; }
            set { _asEndnotes = value; OnPropertyChanged(); }
        }

        [DataMember]
        public string StylesFile
        {
            get => _stylesFile;
            set { _stylesFile = value; OnPropertyChanged(); }
        }

        [DataMember]
        public string OutFile
        {
            get => _outfile;
            set { _outfile = value; OnPropertyChanged(); }
        }

        [DataMember]
        public string GedcomFile
        {
            get => _gedcomFile;
            set { _gedcomFile = value; OnPropertyChanged(); }
        }

        [DataMember]
        public ListOfStyleAssignments StyleMap
        {
            get => _styleMap;
            set { _styleMap = value; OnPropertyChanged(); }
        }

        [DataMember]
        public int Generations
        {
            get => _generations;
            set { _generations = value; OnPropertyChanged(); }
        }

        [DataMember]
       public bool AppendGrandkids
        {
            get => _appendGrandkids;
            set { _appendGrandkids = value; OnPropertyChanged(); }
        }

       [DataMember]
        public bool ObscureLiving
        {
            get { return _obscureLiving; }
            set { _obscureLiving = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool OmitLivingCitations
        {
            get { return _omitLivingCitations; }
            set { _omitLivingCitations = value; OnPropertyChanged(); }
        }

        [DataMember]
        public int LivingFence
        {
            get { return _livingFence; }
            set 
            {
                _livingFence = value; 
                PresumedLivingBoundaryYear = DateTime.Now.Year - value;
                OnPropertyChanged();
            }
        }

        public int PresumedLivingBoundaryYear { get; private set; }

        [DataMember]
        public bool IncludeBurial
        {
           get => _includeBurial;
           set { _includeBurial = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool OmitBurialDate
        {
            get { return _omitBurialDate; }
            set { _omitBurialDate = value; OnPropertyChanged(); }
        }

        [DataMember]
        public BaptismOptions BaptismOption
        {
            get { return _baptismOption; }
            set { _baptismOption = value; OnPropertyChanged(); }
        }

        public bool IncludeBaptism => BaptismOption == BaptismOptions.Always;
        public bool BaptIfNoBirt => BaptismOption == BaptismOptions.WhenNoBirth;

        [DataMember]
        public bool IncludeFactDescriptions
        {
            get => _includeFactDescriptions;
            set { _includeFactDescriptions = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool ReduceContinuedChildren
        {
            get => _reduceContinuedChildren;
            set { _reduceContinuedChildren = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool MinimizeContinuedChildren
        {
            get { return _minimizeContinuedChildren; }
            set { _minimizeContinuedChildren = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool ReducePlaceNames
        {
            get => _reducePlaceNames;
            set { _reducePlaceNames = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool FullPlaceOncePerGen
        {
            get { return _oncePer; }
            set { _oncePer = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool DropUsa
        {
            get { return _dropUsa; }
            set { _dropUsa = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool InjectCounty
        {
            get { return _injectCounty; }
            set { _injectCounty = value; OnPropertyChanged(); }
        }

        [DataMember]
        public string LastPersonId
        {
            get { return _lastPersonId; }
            set { _lastPersonId = value; OnPropertyChanged(); }
        }

        [DataMember]
        public string LastPersonFile
        {
            get { return _lastPersonFile; }
            set { _lastPersonFile = value; OnPropertyChanged(); }
        }


        //public string NameIndexHeading
        //{
        //    get { return _nameIndexHeading; }
        //    set { _nameIndexHeading = value; OnPropertyChanged(); }
        //}

        //public string NameIndexName
        //{
        //    get { return _nameIndexName; }
        //    set { _nameIndexName = value; OnPropertyChanged(); }
        //}

        //public string PlaceIndexHeading
        //{
        //    get { return _placeIndexHeading; }
        //    set { _placeIndexHeading = value; OnPropertyChanged(); }
        //}

        //public string PlaceIndexName
        //{
        //    get { return _placeIndexName; }
        //    set { _placeIndexName = value; OnPropertyChanged(); }
        //}

        //public bool NameIndex
        //{
        //    get { return _nameIndex; }
        //    set { _nameIndex = value; OnPropertyChanged(); }
        //}

        //public bool PlaceIndex
        //{
        //    get { return _placeIndex; }
        //    set { _placeIndex = value; OnPropertyChanged(); }
        //}

        [DataMember]
        public IndexSettings NameIndexSettings
        {
            get => _nameIndexSettings;
            set => _nameIndexSettings = value;
        }

        [DataMember]
        public IndexSettings PlaceIndexSettings
        {
            get => _placeIndexSettings;
            set => _placeIndexSettings = value;
        }

        public G2RSettings Init()
        {
            // For clarity, this list only includes the suggested styles 
            // that are actually used in the report
            // NB any mods to the reporter that use additional styles
            // will require un-commenting the corresponding slots
            // here and in the value list!

            StyleMap = new ListOfStyleAssignments()
            {
                // DO NOT DELETE the commented values here, they may be used later
                new StyleAssignment(StyleSlots.ChildName),
                //new StyleAssignment(StyleSlots.GrandkidName),
                //new StyleAssignment(StyleSlots.Grandkids),
                //new StyleAssignment(StyleSlots.KidMoreText),
                new StyleAssignment(StyleSlots.Kids),
                new StyleAssignment(StyleSlots.KidsIntro),
                new StyleAssignment(StyleSlots.MainPerson),
                // // new StyleAssignment(StyleSlots.Normal),
                new StyleAssignment(StyleSlots.MainPersonText),
                new StyleAssignment(StyleSlots.BodyTextIndent),
                new StyleAssignment(StyleSlots.GenerationNumber),
            };
            //StyleMap.Add(_saMainText  = new StyleAssignment(StyleSlots.MainPersonText){StyleName = MainPersonTextStyle});

            // default regexes for dates - based on FTM data 
            DateRules = new List<ContentReformatter>()
            {
                new ContentReformatter(){Role = PatternRole.DateAboutBeforeAfter, RecognizerPattern = @"\A((?<about>ABT)|(?<after>AFT)|(?<before>BEF))\s+(?<date>.+)\z", Emitter = @"{0} {1}"},
                //new ContentReformatter(){Role = PatternRole.DateAfter, RecognizerPattern = @"AFT\s+(?<year>\d{4})\s+AND\s+(?<year2>\d{4})", Emitter = @"after {0}"},
                //new ContentReformatter(){Role = PatternRole.DateBefore, RecognizerPattern = @"BEF\s+(?<year>\d{4})\s+AND\s+(?<year2>\d{4})", Emitter = @"before {0}"},
                //new ContentReformatter(){Role = PatternRole.DateAbout, RecognizerPattern = @"\AABT\s+(?<date>.+)\z", Emitter = @"about {0}"},
                new ContentReformatter(){Role = PatternRole.DateBetween, RecognizerPattern = @"BET\s+(?<year>\d{4})\s+AND\s+(?<year2>\d{4})", Emitter = @"between ${year} and ${year2}"},
                new ContentReformatter(){Role = PatternRole.Date, RecognizerPattern = @"\A((?<day>\d{1,2})\s+)?((?<month>[A-Z]{3})\s+)?(?<year>\d{4}(/\d{1,2})?)?\z", Emitter = @"${day} {0} ${year}"},
                new ContentReformatter(){Role = PatternRole.PlaceUSA, RecognizerPattern = @"\A(?<PLACE>.*?), (USA|(United States of America))\z", Emitter = @""},
                new ContentReformatter(){Role = PatternRole.Place3or4, RecognizerPattern = @"\A((?<locale>.+, ))?((?<city>.+)(, ))((?<county>.+)(, ))((?<state>.+))\z", Emitter = @"${locale}${city}, ${county}, ${state}", ShortEmitter = @"${locale}${city}"},
                new ContentReformatter(){Role = PatternRole.Place1or2, RecognizerPattern = @"\A((?<city>.+, ))?((?<state>.+))\z", Emitter = @"${city}${state}"},

                new ContentReformatter(){Role = PatternRole.Unknown, RecognizerPattern = @"", Emitter = @""},

            };

            return this;
        }


        public G2RSettings InitInternals()
        {
            InitStyleSlotChoices();
            InitTextCleanerChoices();
            InitPageMetrics();
            return this;
        }

        public void InitPageMetrics()
        {
            PageH = 11f;
            PageW = 8.5f;
            MarginB = 1.2f;
            MarginL = 1.5f;
            MarginR = 1.5f;
            MarginT = 1.3f;
        }

        public List<NamedValue<TextCleanerContext>> TextCleanerChoices;
        //    = new List<NamedValue<TextCleanerContext>>()
        //{
        //    new NamedValue<TextCleanerContext>() {Name = "Nowhere", Value = TextCleanerContext.Nowhere},
        //    new NamedValue<TextCleanerContext>() {Name = "Everywhere", Value = TextCleanerContext.Everywhere},
        //    new NamedValue<TextCleanerContext>() {Name = "Full Citation", Value = TextCleanerContext.FullCitation},
        //    new NamedValue<TextCleanerContext>() {Name = "See Note", Value = TextCleanerContext.SeeNote},
        //    new NamedValue<TextCleanerContext>() {Name = "Others List", Value = TextCleanerContext.OthersList},
        //};

        public void InitTextCleanerChoices()
        {
            TextCleanerChoices = new List<NamedValue<TextCleanerContext>>()
            {
                new NamedValue<TextCleanerContext>() {Name = "Nowhere", Value = TextCleanerContext.Nowhere},
                new NamedValue<TextCleanerContext>() {Name = "Everywhere", Value = TextCleanerContext.Everywhere},
                new NamedValue<TextCleanerContext>() {Name = "Full Citation", Value = TextCleanerContext.FullCitation},
                new NamedValue<TextCleanerContext>() {Name = "See Note", Value = TextCleanerContext.SeeNote},
                new NamedValue<TextCleanerContext>() {Name = "Others List", Value = TextCleanerContext.OthersList},
            };
        }

        public List<NamedValue<StyleSlots>> StyleSlotChoices; 
        //    new List<NamedValue<StyleSlots>>()
        //{
        //    // DO NOT DELETE the commented values here, they may be used later
        //    //new NamedValue<StyleSlots>(){Name = "Additional main person text", Value = StyleSlots.BodyTextIndent},
        //    new NamedValue<StyleSlots>(){Name = "Child name", Value = StyleSlots.ChildName},
        //    new NamedValue<StyleSlots>(){Name = "Children intro", Value = StyleSlots.KidsIntro},
        //    //new NamedValue<StyleSlots>(){Name = "Child additional text", Value = StyleSlots.KidMoreText},
        //    new NamedValue<StyleSlots>(){Name = "Children", Value = StyleSlots.Kids},
        //    new NamedValue<StyleSlots>(){Name = "Generation number", Value = StyleSlots.GenerationNumber},
        //    //new NamedValue<StyleSlots>(){Name = "Grandchild name", Value = StyleSlots.GrandkidName},
        //    //new NamedValue<StyleSlots>(){Name = "Grandchildren", Value = StyleSlots.Grandkids},
        //    new NamedValue<StyleSlots>(){Name = "Main person name (char)", Value = StyleSlots.MainPerson},
        //    new NamedValue<StyleSlots>(){Name = "Main person text (para)", Value = StyleSlots.MainPersonText},
        //};

        public void InitStyleSlotChoices()
        {

            StyleSlotChoices = new List<NamedValue<StyleSlots>>()
            {
                // DO NOT DELETE the commented values here, they may be used later
                //new NamedValue<StyleSlots>(){Name = "Additional main person text", Value = StyleSlots.BodyTextIndent},
                new NamedValue<StyleSlots>() {Name = "Child name", Value = StyleSlots.ChildName},
                new NamedValue<StyleSlots>() {Name = "Children intro", Value = StyleSlots.KidsIntro},
                //new NamedValue<StyleSlots>(){Name = "Child additional text", Value = StyleSlots.KidMoreText},
                new NamedValue<StyleSlots>() {Name = "Children", Value = StyleSlots.Kids},
                new NamedValue<StyleSlots>() {Name = "Generation number", Value = StyleSlots.GenerationNumber},
                //new NamedValue<StyleSlots>(){Name = "Grandchild name", Value = StyleSlots.GrandkidName},
                //new NamedValue<StyleSlots>(){Name = "Grandchildren", Value = StyleSlots.Grandkids},
                new NamedValue<StyleSlots>() {Name = "Main person name (char)", Value = StyleSlots.MainPerson},
                new NamedValue<StyleSlots>() {Name = "Main person text (para)", Value = StyleSlots.MainPersonText},
                new NamedValue<StyleSlots>() {Name = "Main Notes", Value = StyleSlots.BodyTextIndent},
            };
        }

        private List<NamedValue<CitationPartName>> _cpcs;
        public List<NamedValue<CitationPartName>> CitationPartChoices => _cpcs ?? (_cpcs = BuildCitePartChoices());

        private List<NamedValue<CitationPartName>> BuildCitePartChoices()
        {
            return new List<NamedValue<CitationPartName>>()
            {
                new NamedValue<CitationPartName>() {Name = "Source ABBR", Value = CitationPartName.Source_ABBR},
                new NamedValue<CitationPartName>() {Name = "Source AUTH", Value = CitationPartName.Source_AUTH},
                new NamedValue<CitationPartName>() {Name = "Source NOTE", Value = CitationPartName.Source_NOTE},
                new NamedValue<CitationPartName>() {Name = "Source PUBL", Value = CitationPartName.Source_PUBL},
                new NamedValue<CitationPartName>() {Name = "Source REPO", Value = CitationPartName.Source_REPO},
                new NamedValue<CitationPartName>() {Name = "Source TEXT", Value = CitationPartName.Source_TEXT},
                new NamedValue<CitationPartName>() {Name = "Source TITL", Value = CitationPartName.Source_TITL},
                new NamedValue<CitationPartName>() {Name = "Citation  DATA.DATE", Value = CitationPartName.Citation_DATA_DATE},
                new NamedValue<CitationPartName>() {Name = "Citation  DATA.TEXT", Value = CitationPartName.Citation_DATA_TEXT},
                new NamedValue<CitationPartName>() {Name = "Citation  PAGE", Value = CitationPartName.Citation_PAGE},
                new NamedValue<CitationPartName>() {Name = "Citation  URL (_LINK)", Value = CitationPartName.Citation_URL},
                new NamedValue<CitationPartName>() {Name = "Literal", Value = CitationPartName.LiteralOnly},
                new NamedValue<CitationPartName>() {Name = "None", Value = CitationPartName.None},
            };
        }
    }

    public enum PatternRole
    {
        Unknown,
        DateBetween,
        DateAboutBeforeAfter,
        DateBefore,
        DateAbout,
        DateAfter,
        Date,
        PlaceUSA,
        Place3or4,
        Place1or2,
    }

    public enum CitationPartName
    {
        None,
        Source_AUTH,
        Source_TITL,
        Source_PUBL,
        Source_REPO,
        Source_NOTE,
        Source_TEXT,
        Source_ABBR,
        Citation_PAGE,
        Citation_DATA_TEXT,
        Citation_DATA_DATE,
        Citation_URL,
        LiteralOnly
    }




    public class ContentReformatter
    {
        public PatternRole Role { get; set; }
        public string RecognizerPattern { get; set; }
        public string Emitter { get; set; }
        public string ShortEmitter { get; set; }

        private Regex _extractor;
        public Regex Extractor => _extractor ?? (_extractor = new Regex(RecognizerPattern));

    }

    public enum IndexRole
    {
        Names,
        Places
    }
    public class IndexSettings : AbstractBindableModel
    {
        private string _indexName = "names";
        private string _indexHeading = "Index of Names";
        private bool _enabled = true;
        private int _columns = 2;
        private string _sep = "\t";

        private IndexRole _role;

        public IndexRole Role
        {
            get => _role;
            set { _role = value; OnPropertyChanged();}
        }

        // looks good this way:
        // \e "	" \c "2" \z "1033"


        public string Separator
        {
            get { return _sep; }
            set { _sep = value; }
        }

        public int Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }

        public string IndexHeading
        {
            get { return _indexHeading; }
            set { _indexHeading = value; OnPropertyChanged(); }
        }

        //private string x = "\\e \"\t\" \\c \"2\" \\z \"1033\"";

        public string IndexName
        {
            get { return _indexName; }
            set { _indexName = value; OnPropertyChanged(); }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; OnPropertyChanged(); }
        }
    }
}
