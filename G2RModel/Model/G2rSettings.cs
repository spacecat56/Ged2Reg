﻿// G2rSettings.cs
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using G2RModel.Model;


namespace Ged2Reg.Model
{
    #region enums
    public enum DocumentType
    {
        DocX,
        Odt
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
    #endregion

    [DataContract]
    public class G2RSettings : AbstractBindableModel
    {
        #region persistence and constructor
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
        
        private static string GetPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"West Leitrim Software\Ged2Reg\Ged2RegSettings.xml");
        }

        public G2RSettings()
        {
            // initialize to push the calculated value to boundary year
            LivingFence = 99;
            InitInternals();
            InitNameIndexSettings();
            InitPlaceIndexSettings();
        }
        #endregion
        
        #region private/backing fields
        private string _stylesFile;
        private string _outfile;
        private string _gedcomFile;
        private ListOfStyleAssignments _styleMap;
        private bool _appendGrandkids;
        
        private bool _obscureLiving = true;
        private int _livingFence = 99;
        private bool _omitLivingCitations;

        private bool _includeBurial = true;
        private bool _omitBurialDate = true;
        private BaptismOptions _baptismOption = BaptismOptions.Always;
        private bool _includeFactDescriptions = true;
        private bool _editDescriptions;
        private bool _italicsInLineage;
        private bool _reduceContinuedChildren = true;
        private bool _minimizeContinuedChildren;
        private bool _stdBriefContd;
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
        private bool _repeatInline;
        private bool _deferConsecutive;
        private bool _citePlaceholders;

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
        private string _unkInReport = "_____";

        private string _lastPersonId;
        private string _lastPersonFile;

        private float _pageW = 8.5f;
        private float _pageH = 11f;
        private float _marginL = 1.5f;
        private float _marginR = 1.5f;
        private float _marginT = 1.3f;
        private float _marginB = 1.2f;

        private bool _debug;

        private IndexSettings _nameIndexSettings;
        private IndexSettings _placeIndexSettings;
        private DocumentType _documentType;
        private bool _indexMarriedNames;

        private bool _reportSummary;
        private DateTime? _lastRun;
        private TimeSpan? _lastRunSpan;
        private string _lastFile;
        private bool _otherEvents;

        private string _author;
        private string _title;

        private TextCleaner _cleaner;
        private bool _lastActive;
        private bool _ancestorsReport;

        private bool _suppressGenNbrs;
        private bool _gernerationPrefix;
        private bool _placeholders;
        private bool _backRefs;
        private bool _includeSibs;
        private int _minFromGen;
        private bool _spaceBetweenCouples;
        private bool _generationHeadings;
        private bool _allowMultiple = true;
        private bool _reduceMargins;
        private bool _useHostName;

        private bool _focus;
        private bool _continue;
        private string _focusName;
        private string _focusId;
        private bool _omitFocusSpouses;
        private bool _findDuplicates;
        private bool _omitBackrefsLater;

        private bool _allFamilies = true;

        private bool _GenNbrAllChildren;
        private bool _placeFirst;
        private string _firstGenNbr;
        private bool _downshiftNames;
        private bool _abbreviate;

        private bool _appendTitle;

        #endregion

        #region initializations
        // note: deserialization does not init (some of) these
        public void InitNameIndexSettings()
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
        public void InitPlaceIndexSettings()
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
        public void ClearLastFileInfo()
        {
            LastRunTimeSpan = null;
            LastRun = null;
            LastFileCreated = null;
        }

        public void Reset()
        {
            _citeShortFormatter = _citeFormatter = null;
        }

        public G2RSettings Defaults()
        {
            ConformToRegister();

            DeferConsecutiveRepeats = true;
            CitationPlaceholders = false;
            ReducedMargins = true;
            UseHostName = true;
            GenerationPrefix = true;
            ReportSummary = true;

            return this;
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
                new StyleAssignment(StyleSlots.KidsAlt),
                new StyleAssignment(StyleSlots.KidsIntro),
                new StyleAssignment(StyleSlots.MainPerson),
                // // new StyleAssignment(StyleSlots.Normal),
                new StyleAssignment(StyleSlots.MainPersonText),
                new StyleAssignment(StyleSlots.BodyTextIndent),
                new StyleAssignment(StyleSlots.GenerationNumber),
            };
            //StyleMap.Add(_saMainText  = new StyleAssignment(StyleSlots.MainPersonText){StyleName = MainPersonTextStyle});

            // default regexes for dates - based on FTM data 
            InitDateRules();

            return this;
        }

        private void InitDateRules()
        {
            DateRules = new List<ContentReformatter>()
            {
                new ContentReformatter()
                {
                    Role = PatternRole.DateAboutBeforeAfter,
                    RecognizerPattern =
                        @"\A(?i)((?<about>ABT)|(?<after>AFT)|(?<before>BEF))\s+(?<date>.+)\z", // era picked up in .+
                    Emitter = @"{0} {1}"
                },
                //new ContentReformatter(){Role = PatternRole.DateAfter, RecognizerPattern = @"AFT\s+(?<year>\d{4})\s+AND\s+(?<year2>\d{4})", Emitter = @"after {0}"},
                //new ContentReformatter(){Role = PatternRole.DateBefore, RecognizerPattern = @"BEF\s+(?<year>\d{4})\s+AND\s+(?<year2>\d{4})", Emitter = @"before {0}"},
                //new ContentReformatter(){Role = PatternRole.DateAbout, RecognizerPattern = @"\AABT\s+(?<date>.+)\z", Emitter = @"about {0}"},
                new ContentReformatter()
                {
                    Role = PatternRole.DateBetween,
                    RecognizerPattern =
                        @"(?i)(BET\s+(?<year>\d+)(?<era>\s*(AD|BC|BCE|CE))?\s+AND\s+(?<year2>\d+)(?<era2>\s*(AD|BC|BCE|CE))?)|((?<year>\d{1,4})\s*-\s*(?<year2>\d{1,4}))",
                    Emitter = @"between ${year}${era} and ${year2}${era2}"
                },
                new ContentReformatter()
                {
                    Role = PatternRole.Date,
                    RecognizerPattern =
                        @"\A(?i)((?<day>\d{1,2})\s+)?((?<month>[A-Z]{3})\s+)?(?<year>\d{1,4}(/\d{1,2})?)?(?<era>\s*(AD|BC|BCE|CE))?\z",
                    Emitter = @"${day} {0} ${year}${era}"
                },
                new ContentReformatter()
                {
                    Role = PatternRole.PlaceUSA, RecognizerPattern = @"\A(?<PLACE>.*?), (USA|(United States of America))\z",
                    Emitter = @""
                },
                new ContentReformatter()
                {
                    Role = PatternRole.Place3or4,
                    RecognizerPattern = @"\A((?<locale>.+, ))?((?<city>.+)(, ))((?<county>.+)(, ))((?<state>.+))\z",
                    Emitter = @"${locale}${city}, ${county}, ${state}", ShortEmitter = @"${locale}${city}"
                },
                new ContentReformatter()
                {
                    Role = PatternRole.Place1or2, RecognizerPattern = @"\A((?<city>.+, ))?((?<state>.+))\z",
                    Emitter = @"${city}${state}"
                },

                new ContentReformatter() {Role = PatternRole.Unknown, RecognizerPattern = @"", Emitter = @""},
            };
        }

        public G2RSettings InitInternals()
        {
            InitStyleSlotChoices();
            InitTextCleanerChoices();
            InitPageMetrics();
            InitDateRules();
            ApplyVersionUpdates();
            return this;
        }

        public G2RSettings ApplyVersionUpdates()
        {
            void AddIfMissing(StyleSlots styleSlots)
            {
                var ka = (from s in StyleMap where s.Style == styleSlots select s).FirstOrDefault();
                if (ka == null)
                {
                    StyleMap.Add(new StyleAssignment(styleSlots));
                }
            }
            if (StyleMap == null) return this; // some odd circumstance
            AddIfMissing(StyleSlots.KidsAlt);
            AddIfMissing(StyleSlots.GenerationDivider);
            AddIfMissing(StyleSlots.GenerationDivider3Plus);
            AddIfMissing(StyleSlots.BodyTextNotes);
            if (AssumedMaxLivingGenerations == 0)
                AssumedMaxLivingGenerations = 6;
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

        public void ApplyReducedMargins()
        {
            PageH = 11f;
            PageW = 8.5f;
            MarginB = 1.0f;
            MarginL = 1.0f;
            MarginR = 1.0f;
            MarginT = 1.0f;
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
                new NamedValue<StyleSlots>() {Name = "Children (alt)", Value = StyleSlots.KidsAlt},
                new NamedValue<StyleSlots>() {Name = "Generation number", Value = StyleSlots.GenerationNumber},
                new NamedValue<StyleSlots>() {Name = "Generation divider", Value = StyleSlots.GenerationDivider},
                new NamedValue<StyleSlots>() {Name = "Generation divider (3+)", Value = StyleSlots.GenerationDivider3Plus},
                //new NamedValue<StyleSlots>(){Name = "Grandchild name", Value = StyleSlots.GrandkidName},
                //new NamedValue<StyleSlots>(){Name = "Grandchildren", Value = StyleSlots.Grandkids},
                new NamedValue<StyleSlots>() {Name = "Main person name (char)", Value = StyleSlots.MainPerson},
                new NamedValue<StyleSlots>() {Name = "Main person text (para)", Value = StyleSlots.MainPersonText},
                new NamedValue<StyleSlots>() {Name = "More text (para)", Value = StyleSlots.BodyTextIndent},
                new NamedValue<StyleSlots>() {Name = "Main Notes", Value = StyleSlots.BodyTextNotes},
            };
        }
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
        #endregion
        
        #region transients
        [IgnoreDataMember] public string ProgramName { get; set; }
        [IgnoreDataMember] public string ProgramVer { get; set; }

        // this does not de/serialize readily because of parent back-ref; it will rebuild from the list on use
        [IgnoreDataMember] 
        public TextCleaner CitationTitleCleaner
        {
            get { return _cleaner ??= new TextCleaner(TextCleaners); }
            set { _cleaner = value; }
        }

        #endregion

        #region persistent/bindable properties
        #region AncestorReportOptions
        [DataMember]
        public bool AncestorsReport
        {
            get { return _ancestorsReport; }
            set { _ancestorsReport = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool Placeholders
        {
            get { return _placeholders; }
            set { _placeholders = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool SuppressGenNbrs
        {
            get { return _suppressGenNbrs; }
            set { _suppressGenNbrs = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool IncludeBackRefs
        {
            get { return _backRefs; }
            set { _backRefs = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool IncludeSiblings
        {
            get { return _includeSibs; }
            set { _includeSibs = value; OnPropertyChanged(); }
        }

        [DataMember][Obsolete]
         public bool AllFamilies
        {
            get { return _allFamilies; }
            set { _allFamilies = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool GenerationPrefix
        {
            get { return _gernerationPrefix; }
            set { _gernerationPrefix = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool GenerationHeadings
        {
            get { return _generationHeadings; }
            set { _generationHeadings = value; OnPropertyChanged(); }
        }

        [DataMember]
        public int MinimizeFromGeneration
        {
            get { return _minFromGen; }
            set { _minFromGen = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool Focus
        {
            get { return _focus; }
            set { _focus = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool ContinuePastFocus
        {
            get { return _continue; }
            set { _continue = value; OnPropertyChanged(); }
        }

        [DataMember]
        public string FocusName
        {
            get { return _focusName; }
            set { _focusName = value; OnPropertyChanged(); }
        }

        [DataMember]
        public string FocusId 
        {
            get { return _focusId; }
            set { _focusId = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool OmitFocusSpouses
        {
            get { return _omitFocusSpouses; }
            set { _omitFocusSpouses = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool FindDuplicates
        {
            get { return _findDuplicates; }
            set { _findDuplicates = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool OmitBackRefsLater
        {
            get { return _omitBackrefsLater; }
            set { _omitBackrefsLater = value; OnPropertyChanged(); }
        }

        #endregion

        #region TextFixers
        private string _finderForNames;
        private string _fixerForNames;

        private string _finderForEvents;
        private string _fixerForEvents;

        [DataMember]
        public string FinderForNames
        {
            get { return _finderForNames; }
            set { _finderForNames = value; OnPropertyChanged(); }
        }

        [DataMember]
        public string FixerForNames
        {
            get { return _fixerForNames; }
            set { _fixerForNames = value; OnPropertyChanged(); }
        }

        [DataMember]
        public string FinderForEvents
        {
            get { return _finderForEvents; }
            set { _finderForEvents = value; OnPropertyChanged(); }
        }

        [DataMember]
        public string FixerForEvents
        {
            get { return _fixerForEvents; }
            set { _fixerForEvents = value; OnPropertyChanged(); }
        }
        #endregion


        [DataMember]
        public bool AppendTitle
        {
            get { return _appendTitle; }
            set { _appendTitle = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool AbbreviateChildEvents
        {
            get { return _abbreviate; }
            set { _abbreviate = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool ReducedMargins
        {
            get { return _reduceMargins; }
            set
            {
                _reduceMargins = value;
                OnPropertyChanged();
                ApplyMarginOption();
            }
        }
        [DataMember]
        public bool DownshiftNames
        {
            get { return _downshiftNames; }
            set { _downshiftNames = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool EditDescriptions
        {
            get { return _editDescriptions; }
            set { _editDescriptions = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool GenNbrAllChildren
        {
            get { return _GenNbrAllChildren; }
            set { _GenNbrAllChildren = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool PlaceFirst
        {
            get { return _placeFirst; }
            set { _placeFirst = value; OnPropertyChanged(); }
        }

        [DataMember]
        public string FirstGenNbr
        {
            get { return _firstGenNbr; }
            set { _firstGenNbr = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool IndexMarriedNames
        {
            get { return _indexMarriedNames; }
            set { _indexMarriedNames = value; OnPropertyChanged(); }
        }
        private bool _minusChild;

        [DataMember]
        public bool MinusChild
        {
            get { return _minusChild; }
            set { _minusChild = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool AllowMultipleAppearances
        {
            get { return _allowMultiple; }
            set { _allowMultiple = value; OnPropertyChanged(); }
        }
        private int _assumedMaxLivingGenerations;

        [DataMember]
        public int AssumedMaxLivingGenerations
        {
            get { return _assumedMaxLivingGenerations; }
            set { _assumedMaxLivingGenerations = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool UseHostName
        {
            get { return _useHostName; }
            set { _useHostName = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool CitationPlaceholders
        {
            get { return _citePlaceholders; }
            set { _citePlaceholders = value; OnPropertyChanged(); }
        }
        private bool _preferEdited = true;

        [DataMember]
        public bool PreferEditedCitations
        {
            get { return _preferEdited; }
            set { _preferEdited = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool SpaceBetweenCouples
        {
            get { return _spaceBetweenCouples; }
            set { _spaceBetweenCouples = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool LastActive
        {
            get { return _lastActive; }
            set { _lastActive = value; } // not intended for binding to UI
        }

        [DataMember]
        public DocumentType DocumentType
        {
            get { return _documentType; }
            set { _documentType = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool IncludeOtherEvents
        {
            get { return _otherEvents; }
            set { _otherEvents = value; OnPropertyChanged(); }
        }

        [DataMember]
        public DateTime? LastRun
        {
            get { return _lastRun; }
            set { _lastRun = value; OnPropertyChanged(); }
        }

        [DataMember]
        public TimeSpan? LastRunTimeSpan
        {
            get { return _lastRunSpan; }
            set { _lastRunSpan = value; OnPropertyChanged(); }
        }

        [DataMember]
        public string LastFileCreated
        {
            get { return _lastFile; }
            set { _lastFile = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool ReportSummary
        {
            get { return _reportSummary; }
            set { _reportSummary = value; OnPropertyChanged(); }
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

        [DataMember]
        public string SetName { get; set; }

        private ListOfTextCleanerEntry _textCleaners;

        [DataMember]
        public ListOfTextCleanerEntry TextCleaners
        {
            get => _textCleaners ??= new ListOfTextCleanerEntry();
            set => _textCleaners = value;
        }

        [DataMember]
        public bool DebuggingOutput
        {
            get { return _debug; }
            set { _debug = value; OnPropertyChanged(); }
        }

        [DataMember]
        public List<ContentReformatter> DateRules { get; set; }

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

        [DataMember]
        public bool RepeatNoteRefInline
        {
            get { return _repeatInline; }
            set { _repeatInline = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool DeferConsecutiveRepeats
        {
            get { return _deferConsecutive; }
            set { _deferConsecutive = value; OnPropertyChanged(); }
        }

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

        [DataMember]
        public bool IncludeFactDescriptions
        {
            get => _includeFactDescriptions;
            set { _includeFactDescriptions = value; OnPropertyChanged(); }
        }

        [DataMember]
        public bool ItalicsNamesInLineageList
        {
            get { return _italicsInLineage; }
            set { _italicsInLineage = value; OnPropertyChanged(); }
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
        public bool StandardBriefContinued
        {
            get { return _stdBriefContd; }
            set { _stdBriefContd = value; OnPropertyChanged(); }
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
        #endregion

        #region public convenience functions

        public bool NoOption { get; set; } // helper for test case cycler - all off state

        public void ApplyMarginOption()
        {
            if (_reduceMargins)
                ApplyReducedMargins();
            else
                InitPageMetrics();
        }

        public void ConformToRegister(bool registerReport = true)
        {
            AncestorsReport = !registerReport;
            BaptismOption = BaptismOptions.Always;
            IncludeBurial = true;
            OmitBurialDate = false;
            ReducePlaceNames = true;
            InjectCounty = true;
            DropUsa = true;
            FullPlaceOncePerGen = false;
            ItalicsNamesInLineageList = true;

            AbbreviateChildEvents = true;
            ReduceContinuedChildren = false;
            MinimizeContinuedChildren = false;
            StandardBriefContinued = true;
            GenNbrAllChildren = false;

            HandleUnknownNames = true;
            UnknownInReport = "_____";

            SuppressGenNbrs = true;
            AllFamilies = false; // option is defunct / not visible anyway
            AllowMultipleAppearances = true;
            Placeholders = true;
            SpaceBetweenCouples = true;
            IncludeBackRefs = false;
            IncludeSiblings = false;
            GenerationPrefix = false;
            Focus = false;
            OmitFocusSpouses = false;
            ContinuePastFocus = false;
            OmitBackRefsLater = false;
            MinimizeFromGeneration = 0;
            GenerationHeadings = false;

            OmitCitesOnContinued = true;
            Brackets = false;
            DeferConsecutiveRepeats = false;
            CitationPlaceholders = true;

            MainPersonNotes = false;
            SpousesNotes = false;

            IndexMarriedNames = true;
            PlaceFirst = true;
            FirstGenNbr = null;
        }
        public string ReportConformanceSettings()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine();
            sb.AppendLine("....Settings ('Register' conformance)");

            Append(sb, $"Report type", $"Descendants{(AncestorsReport ? ' ' : '*')}");
            Append(sb, "Include baptism", this.BaptismOption.ToString());
            Append(sb, "Include burial events", this.IncludeBurial);
            Append(sb, "Omit burial date", this.OmitBurialDate);
            Append(sb, "Reduce place names", this.ReducePlaceNames);
            sb.AppendLine("NB - REVIEW THIS SETTING:");
            Append(sb, "Gen number on all children", this.GenNbrAllChildren);
            Append(sb, "Full place name/gen", this.FullPlaceOncePerGen);
            Append(sb, "Drop 'USA'", this.DropUsa);
            Append(sb, "Inject county", this.InjectCounty);
            Append(sb, "Place before Date", this.PlaceFirst);
            Append(sb, "Lineage list in italics", this.ItalicsNamesInLineageList);

            Append(sb, "Abbreviate child events", this.AbbreviateChildEvents);
            Append(sb, "Brief (standard) children", this.StandardBriefContinued);

            Append(sb, "Reformat unknowns", this.HandleUnknownNames);
            Append(sb, "Unknown name as output", this.UnknownInReport);
            Append(sb, "First Generation Number", this.FirstGenNbr);

            sb.AppendLine();
            sb.AppendLine("....Settings ('Ancestry' report conformance)");
            Append(sb, $"Report type", $"Ancestors{(AncestorsReport ? '*' : ' ')}" );
            Append(sb, "Suppress gen. superscripts", this.SuppressGenNbrs);
            //Append(sb, "All Families", this.AllFamilies);
            Append(sb, "Allow Multiple Appearances", this.AllowMultipleAppearances);
            Append(sb, "Placeholders for unknowns", Placeholders);
            Append(sb, "Space between couples", SpaceBetweenCouples);
            Append(sb, "Include back references", this.IncludeBackRefs);
            //Append(sb, "Also include siblings", this.IncludeSiblings);
            Append(sb, "Focus", this.Focus);
            //Append(sb, "Drop Back Refs", this.OmitBackRefsLater);

            sb.AppendLine();
            sb.AppendLine("....Settings (citation conformance) (incomplete)");
            Append(sb, "Omit On Continued", this.OmitCitesOnContinued);
            Append(sb, "Brackets on footnote nbrs", this.Brackets);
            Append(sb, "Placeholders for uncited", this.CitationPlaceholders);
            sb.AppendLine("NB - REVIEW THIS SETTING:");
            Append(sb, "Defer consecutive repeats", this.DeferConsecutiveRepeats);

            sb.AppendLine();
            sb.AppendLine("....Settings (index conformance)");
            Append(sb, "Also index married names", IndexMarriedNames);

            sb.AppendLine();
            sb.AppendLine("....Settings (notes conformance)");
            Append(sb, "Notes / main persons", this.MainPersonNotes);
            Append(sb, "Notes / spouses", this.SpousesNotes);

            return sb.ToString();
        }


        public string ReportKeySettings()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Settings (selected)");
            Append(sb, "Set name", this.SetName);
            Append(sb, "Program name", this.ProgramName);
            Append(sb, "Program version", this.ProgramVer);
            Append(sb, "Generations", this.Generations);
            Append(sb, $"Report type", AncestorsReport?"Ancestors" : "Descendants");
            if (AncestorsReport)
            {
                //Append(sb, "All families", this.AllFamilies);
                Append(sb, "Allow multiple appearances", this.AllowMultipleAppearances);
                Append(sb, "Suppress gen. superscripts", this.SuppressGenNbrs);
                Append(sb, "Placeholders for unknowns", Placeholders);
                Append(sb, "Include back references", this.IncludeBackRefs);
                Append(sb, "Also include siblings", this.IncludeSiblings);
                Append(sb, "Minimize from generation", this.MinimizeFromGeneration);
                Append(sb, "Focused on one ancestor", this.Focus);
                if (Focus)
                {
                    Append(sb, "Selected ancestor", this.FocusName);
                    Append(sb, "Continue past focus", this.ContinuePastFocus);
                    Append(sb, "Omit focus spouses", this.OmitFocusSpouses);
                    Append(sb, "Omit back-references", this.OmitBackRefsLater);
                }
            }
            Append(sb, "Obscure living", this.ObscureLiving);
            Append(sb, "Reduce place names", this.ReducePlaceNames);
            Append(sb, "Drop 'USA'", this.DropUsa);
            Append(sb, "Inject county", this.InjectCounty);
            Append(sb, "Place before Date", this.PlaceFirst);
            Append(sb, "First Generation Number", this.FirstGenNbr);
            Append(sb, "Abbreviate child events", this.AbbreviateChildEvents);
            Append(sb, "Brief (standard) children", this.StandardBriefContinued);
            Append(sb, "Reformat unknowns", this.HandleUnknownNames);
            Append(sb, "Unknown input", this.UnknownInSource);
            Append(sb, "Unknown output", this.UnknownInReport);
            Append(sb, "Citation strategy", this.CitationStrategy);
            Append(sb, "Citation fill-in strategy", this.FillinCitationStrategy);
            Append(sb, "As end notes", this.AsEndnotes);
            Append(sb, "Summarize additional", this.SummarizeAdditionalCitations);
            Append(sb, "Max in summary", this.NumberCitesToSumamrize);
            Append(sb, "Use 'see note'", this.UseSeeNote);
            Append(sb, "Repeat ref inline", this.RepeatNoteRefInline);
            Append(sb, "Defer consecutive repeats", this.DeferConsecutiveRepeats);
            Append(sb, "Placeholders for uncited", this.CitationPlaceholders);
            Append(sb, "Brackets", this.Brackets);
            Append(sb, "Omit on continued", this.OmitCitesOnContinued);
            Append(sb, "Notes / main persons", this.MainPersonNotes);
            Append(sb, "Notes / spouses", this.SpousesNotes);
            Append(sb, "Convert dividers", this.ConvertDividers);

            if (!string.IsNullOrEmpty(FinderForNames))
            {
                Append(sb, "Name rewrite - finder", this.FinderForNames);
                Append(sb, "Name rewrite - fixer", this.FixerForNames);
                Append(sb, "Name rewrite - valid regex", 
                    new TextFixer(){FinderText = FinderForNames, Fixer = FixerForNames}.Init() != null);
            }
            if (!string.IsNullOrEmpty(FinderForEvents))
            {
                Append(sb, "Event rewrite - finder", this.FinderForEvents);
                Append(sb, "Event rewrite - fixer", this.FixerForEvents);
                Append(sb, "Event rewrite - valid regex",
                    new TextFixer() { FinderText = FinderForEvents, Fixer = FixerForEvents }.Init() != null);
            }

            Append(sb, "Name index", this.NameIndexSettings?.Enabled??false);
            Append(sb, " + married names", IndexMarriedNames);
            Append(sb, "Place index", this.PlaceIndexSettings?.Enabled ?? false);
            Append(sb, "Input file", Path.GetFileName(GedcomFile));
            Append(sb, "Output file", Path.GetFileName(OutFile));

            return sb.ToString();
        }

        private void Append(StringBuilder sb, string caption, object content, int width = 30)
        {
            sb.AppendLine($"{caption.PadRight(width, '.')}{content}");
        }

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
        #endregion

        #region citation formatting
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


        private ListOfCitationParts _citationPartsSeeNote = new ListOfCitationParts()
        {
            //new CitationPart() {Name = CitationPartName.Source_AUTH, FormatString = "{0}, "},
            new CitationPart() {Name = CitationPartName.Source_TITL, FormatString = "{0} ", Sequence = 1},
            //new CitationPart() {Name = CitationPartName.Citation_PAGE, FormatString = "{0}, "},
            //new CitationPart() {Name = CitationPartName.Citation_DATA_TEXT, FormatString = "{0}.", NullValue = "."},
            //new CitationPart() {Name = CitationPartName.Citation_URL, FormatString = ""},
        };
        #endregion

        #region computed properties
        public bool Citations => CitationStrategy != CitationStrategy.None;
        public bool IncludeBaptism => BaptismOption == BaptismOptions.Always;
        public bool BaptIfNoBirt => BaptismOption == BaptismOptions.WhenNoBirth;
        public bool BookMarkNotes => UseSeeNote || RepeatNoteRefInline;
        public int PresumedLivingBoundaryYear { get; private set; }
        #endregion

        #region styles
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

        private List<NamedValue<CitationPartName>> _cpcs;
        public List<NamedValue<CitationPartName>> CitationPartChoices => _cpcs ?? (_cpcs = BuildCitePartChoices());
        #endregion

    }

}
