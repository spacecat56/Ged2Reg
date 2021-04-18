using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using DocxAdapterLib;
using G2RModel.Model;
using Ged2Reg.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OdtAdapterLib;
using WpdInterfaceLib;

namespace G2RModelTest.Model
{
    [TestClass()]
    public class RegisterReporterTests
    {
        class TestLogger : ILocalLogger
        {
            #region Implementation of ILocalLogger

            public void Log(string txt)
            {
                Debug.WriteLine(txt);
            }
            #endregion
        }

        // todo: these must be externalized / abstracted
        public string OutputPath = Path.Combine(Path.GetTempPath(), @"test\ged2reg\"); // @"c:\test\ged2reg";
        public string FocusAncestorId = "@I4961@";
        public string SampleFilePath = @"D:\projects\public\retarget\Ged2Reg\G2RModel\Resources\sample.ged";

        G2RSettings _settings;
        ReportContext _rc;
        private RegisterReportModel _rrm;
        private TestLogger _logger;
        private AsyncActionDelegates _aad;

        private ModelState _currentModelState;

        [TestInitialize]
        public void InitModel()
        {
            _settings = new G2RSettings()
            {
                SetName = "Unit Testing",
                ProgramName = nameof(RegisterReporterTests),
            }.Init().ApplyVersionUpdates();

            _settings.ReportSummary = true;
            _rc = ReportContext.Init(_settings);
            _rrm = new RegisterReportModel();
            _logger = new TestLogger();
            _aad = new AsyncActionDelegates()
            {
                PostStatusReport = PostStatusReport,
                CancelEnable = CancelEnable
            };
            _rrm = new RegisterReportModel()
            {
                Logger = _logger,
                DocFactory = new OoxDocFactory(),
                Settings = _settings
            };
            Directory.CreateDirectory(OutputPath);
        }

        private bool ReadyModel(ModelState needed)
        {
            _settings.Generations = 5;

            _settings.AncestorsReport = needed == ModelState.AncestorReady;
            string samplePath = RegisterReportModel.PathToFileResource("sample.ged") ?? SampleFilePath;
            if (!File.Exists(samplePath))
                throw new FileNotFoundException("Cannot locate sample file");
            if (_currentModelState != needed)
                _rrm.ConfigureSampleFile(samplePath);
            _rrm.Init();
            _currentModelState = needed;
            return true;
        }

        private void CancelEnable(bool ena)
        {
            //
        }

        private void PostStatusReport(string txt)
        {
            _logger.Log(txt);
        }

        [TestMethod]
        public void DescendantTest1()
        {
            _settings.ConformToRegister();
            ReadyModel(ModelState.DescendantReady);
            _settings.OutFile = Path.Combine(OutputPath, "DescendantTest-01.docx");
            ExecSampleReport();
            Assert.IsTrue(File.Exists(_settings.OutFile));
        }

        private void ExecSampleReport()
        {
            if (File.Exists(_settings.OutFile))
                File.Delete(_settings.OutFile);
            _rrm.Settings.Reset();
            _rrm.Exec();
            _rrm.Doc.Save();
            _rrm.Doc.Dispose();
        }

        private void ExecSampleReport(ReportConfig cfg, string[] exts = null)
        {
            exts ??= new[] {".docx", ".odt"};
            foreach (string ext in exts)
            {
                _settings.OutFile = Path.ChangeExtension(_settings.OutFile, ext);
                if (File.Exists(_settings.OutFile))
                    File.Delete(_settings.OutFile);
                _rrm.Settings.Reset();
                _rrm.DocFactory = ext == ".docx" ? (IWpdFactory) new OoxDocFactory() : new OalDocFactory();
                _rrm.Exec();
                _rrm.Doc.Save();
                _rrm.Doc.Dispose();
                Assert.IsTrue(cfg.Eval());
            }
        }

        [TestMethod]
        public void DescendantTest2()
        {
            _settings.ConformToRegister();
            _settings.UseSeeNote = true;
            _settings.UseHostName = true;
            ReadyModel(ModelState.DescendantReady);
            _settings.OutFile = Path.Combine(OutputPath, "DescendantTest-02.docx");
            ExecSampleReport();
            Assert.IsTrue(File.Exists(_settings.OutFile));
        }


        [TestMethod]
        public void AncestorsTest1()
        {
            _settings.ConformToRegister(false);
            _settings.UseSeeNote = true;
            _settings.UseHostName = true;
            ReadyModel(ModelState.AncestorReady);
            _settings.OutFile = Path.Combine(OutputPath, "AncestorsTest-01.docx");
            ExecSampleReport();
            Assert.IsTrue(File.Exists(_settings.OutFile));
        }

        [TestMethod]
        public void SeeNoteTests()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.DescendantReady,
                OutputPath = OutputPath,
                FileName = "D001_SeeNoteOn.docx",
                Title = "Use SeeNote",
                FlagsOn = new List<string>(){nameof(G2RSettings.UseSeeNote)},
                MustOccur = new List<string>() {"see note "}
            }.Init();

            ReadyModel(cfg.ModelState);
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());

            cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.DescendantReady,
                OutputPath = OutputPath,
                FileName = "D001_SeeNoteOff.docx",
                Title = "Do not Use SeeNote",
                FlagsOff = new List<string>() { nameof(G2RSettings.UseSeeNote) },
                MustNotOccur = new List<string>() { "see note " }
            }.Init();

            ReadyModel(cfg.ModelState);
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());
        }

        [TestMethod]
        public void LivingTests()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.DescendantReady,
                OutputPath = OutputPath,
                FileName = "D002_LivingOn.docx",
                Title = "Hide Living",
                FlagsOn = new List<string>() { nameof(G2RSettings.ObscureLiving) },
                MustOccur = new List<string>() { "(Living)" }
            }.Init();


            ReadyModel(cfg.ModelState);
            int regen = _settings.Generations;
            _settings.Generations = 15;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());

            cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.DescendantReady,
                OutputPath = OutputPath,
                FileName = "D002_LivingOff.docx",
                Title = "Not Hide Living",
                FlagsOff = new List<string>() { nameof(G2RSettings.ObscureLiving) },
                MustNotOccur = new List<string>() { "(Living)" }
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 15;
            ExecSampleReport(cfg);
            _settings.Generations = regen;
            Assert.IsTrue(cfg.Eval());
        }

        [TestMethod]
        public void UsaAndDatePlaceOrder()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.DescendantReady,
                OutputPath = OutputPath,
                FileName = "D003_UsaOkDateFirst.docx",
                Title = "USA Ok, Date First",
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.DropUsa),
                    nameof(G2RSettings.PlaceFirst)
                },
                //MustOccur = new List<string>() { "USA" },
                RegexesToAssertTrue = new List<string>()
                {
                    @"on\s.{10,20}\sat\s", 
                    @"(?-i) USA",
                }
            }.Init();

            ReadyModel(cfg.ModelState);
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());

            cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.DescendantReady,
                OutputPath = OutputPath,
                FileName = "D003_UsaNoDateLast.docx",
                Title = "USA Dropped, Place First",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.DropUsa),
                    nameof(G2RSettings.PlaceFirst)
                },
                //MustNotOccur = new List<string>() { "USA" },
                RegexesToAssertTrue = new List<string>()
                {
                    @"at\s.{10,20}\son\s",
                },
                RegexesToAssertFalse = new List<string>()
                {
                    @"(?-i) USA",
                },
            }.Init();

            ReadyModel(cfg.ModelState);
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());
        }

        [TestMethod]
        public void BaptismBurialOptionsTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.DescendantReady,
                OutputPath = OutputPath,
                FileName = "D004_BapBurOn.docx",
                Title = "Baptisms and Burials - Yes",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.IncludeBurial),
                    nameof(G2RSettings.PlaceFirst)
                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.IncludeFactDescriptions), // text may mention other events
                    nameof(G2RSettings.OmitBurialDate)
                },
                MustOccur = new List<string>() { "baptized at ", "buried at " },
            }.Init();
            _settings.BaptismOption = BaptismOptions.Always;

            ReadyModel(cfg.ModelState);
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());

            cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.DescendantReady,
                OutputPath = OutputPath,
                FileName = "D004_BapBurOff.docx",
                Title = "Baptisms and Burials - No",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.PlaceFirst)
                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.IncludeBurial),
                    nameof(G2RSettings.IncludeFactDescriptions), // text may mention other events
                },
                MustNotOccur = new List<string>() { "baptized at ", "buried at " },
            }.Init();
            _settings.BaptismOption = BaptismOptions.None;

            ReadyModel(cfg.ModelState);
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());
        }

        [TestMethod]
        public void GenerationPrefixesTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A001_GenPrefixesOn.docx",
                Title = "Generation prefixes - Yes",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.GenerationPrefix),
                },
                FlagsOff = new List<string>()
                {
                },
                MustOccur = new List<string>() { "02-2.", },
            }.Init();


            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());

            cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A001_GenPrefixesOff.docx",
                Title = "Generation prefixes - No",
                FlagsOn = new List<string>()
                {
                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.GenerationPrefix),
                },
                MustNotOccur = new List<string>() { "02-2.", },
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());
        }

        [TestMethod]
        public void DebugInfoTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A002_DebugInfoOn.docx",
                Title = "Debug Info - Yes",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.DebuggingOutput),
                },
                FlagsOff = new List<string>()
                {
                },
                RegexesToAssertTrue = new List<string>() { @"@I\d{2,4}@", },
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            _settings.DebuggingOutput = false;
            Assert.IsTrue(cfg.Eval());
        }

        [TestMethod]
        public void FocusTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A003_FocusOn.docx",
                Title = "Focus - Yes",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.Focus),
                    nameof(G2RSettings.OmitFocusSpouses),
                    nameof(G2RSettings.GenerationPrefix),
                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.IncludeBackRefs),
                    nameof(G2RSettings.Placeholders)
                },
                RegexesToAssertFalse = new List<string>() { @"\b03-4\b", },
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            _settings.FocusId = FocusAncestorId;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());


            cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A003_FocusOff.docx",
                Title = "Focus - No",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.OmitFocusSpouses),
                    nameof(G2RSettings.GenerationPrefix),
                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.Focus),
                    nameof(G2RSettings.IncludeBackRefs),
                    nameof(G2RSettings.Placeholders)
                },
                RegexesToAssertTrue = new List<string>() { @"\b03-4[ .]", },
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());
        }

        [TestMethod]
        public void FocusMissingpersonTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A003_FocusOn_b.docx",
                Title = "Focus - Yes",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.Focus),
                    nameof(G2RSettings.OmitFocusSpouses),
                    nameof(G2RSettings.GenerationPrefix),
                    nameof(G2RSettings.IncludeBackRefs),
                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.Placeholders)
                },
                RegexesToAssertTrue = new List<string>() { @"\b06-55\b", },
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            _settings.FocusId = FocusAncestorId;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());

            var matches = Regex.Matches(cfg.GetDocContent(), cfg.RegexesToAssertTrue[0]);
            Assert.AreEqual(2, matches.Count);

        }

        [TestMethod]
        public void BackrefsTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A004_BackrefsOn.docx",
                Title = "Backrefs - Yes",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.OmitFocusSpouses),
                    nameof(G2RSettings.GenerationPrefix),
                    nameof(G2RSettings.IncludeBackRefs),
                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.Focus),
                    nameof(G2RSettings.Placeholders)
                },
                RegexesToAssertTrue = new List<string>() { @"\b03-4\b.*?\b03-4\b", },
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());
            cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A004_BackrefsOff.docx",
                Title = "Backrefs - No",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.OmitFocusSpouses),
                    nameof(G2RSettings.GenerationPrefix),
                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.IncludeBackRefs),
                    nameof(G2RSettings.Focus),
                    nameof(G2RSettings.Placeholders)
                },
                RegexesToAssertFalse = new List<string>() { @"\b03-4\b.*?\b03-4\b", },
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());

        }

        [TestMethod]
        public void NoteNumberInlineTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.DescendantReady,
                OutputPath = OutputPath,
                FileName = "F003_RepeatInline_On.docx",
                Title = "Re-Use Note Number Inline - Yes",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.RepeatNoteRefInline)
                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.DeferConsecutiveRepeats),
                    nameof(G2RSettings.Brackets),
                    nameof(G2RSettings.CitationPlaceholders),
                    nameof(G2RSettings.SummarizeAdditionalCitations),
                    nameof(G2RSettings.UseSeeNote),
                },
                //MustOccur = new List<string>() { "Cited for:", "***Citation needed", "Other sources include" },
                //RegexesToAssertTrue = new List<string>() { @"(\[.+?\].*?){10,}", }, // nb probably not definitive: same pattern in page info removed
                //RestrictRexToMainDoc = true,
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());
            // todo: need an effective evaluation
        }
        string[] justDoc = { ".docx" };
        string[] justOdt = { ".odt" };

        [TestMethod]
        public void CombinedCitationOptionsTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.DescendantReady,
                OutputPath = OutputPath,
                FileName = "F002_Defer,Brackets,Placeholders,Summarize_On.docx",
                Title = "Defer, Brackets, Placeholders, Summarize - Yes",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.DeferConsecutiveRepeats),
                    nameof(G2RSettings.Brackets),
                    nameof(G2RSettings.CitationPlaceholders),
                    nameof(G2RSettings.SummarizeAdditionalCitations),
                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.UseSeeNote),
                    nameof(G2RSettings.RepeatNoteRefInline)
                },
                MustOccur = new List<string>(){"Cited for:", "***Citation needed", "Other sources include"},
                RegexesToAssertTrue = new List<string>() { @"(\[.+?\].*?){10,}", }, // nb probably not definitive: same pattern in page info removed
                RestrictRexToMainDoc = true,
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg, justDoc);
            Assert.IsTrue(cfg.Eval());

            // the footnotes are in the main doc body in odt, making the Restrict ineffective
            cfg.RegexesToAssertTrue.Clear();
            cfg.MustOccur = new List<string>() { ">[<text:note" };
            ExecSampleReport(cfg, justOdt);
            Assert.IsTrue(cfg.Eval());

            cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.DescendantReady,
                OutputPath = OutputPath,
                FileName = "F002_Defer,Brackets,Placeholders,Summarize_Off.docx",
                Title = "Defer, Brackets, Placeholders, Summarize - No",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.UseHostName), // catch this en passant, to cover it
                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.DeferConsecutiveRepeats),
                    nameof(G2RSettings.Brackets),
                    nameof(G2RSettings.CitationPlaceholders),
                    nameof(G2RSettings.SummarizeAdditionalCitations),
                    nameof(G2RSettings.UseSeeNote),
                    nameof(G2RSettings.RepeatNoteRefInline)
                },
                MustNotOccur = new List<string>() { "Cited for:", "***Citation needed", "Other sources include" },
                RegexesToAssertFalse = new List<string>() { @"(\[.+?\].*?){10,}", }, // nb probably not definitive: same pattern in page info removed
                RestrictRexToMainDoc = true,
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg, justDoc);
            Assert.IsTrue(cfg.Eval());

            // the footnotes are in the main doc body in odt, making the Restrict ineffective
            cfg.RegexesToAssertFalse.Clear();
            cfg.MustNotOccur = new List<string>(){ ">[<text:note" };
            ExecSampleReport(cfg, justOdt);
            Assert.IsTrue(cfg.Eval());
        }


        [TestMethod]
        public void NotesEndnotesHostMinimizeTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A005_NotesEndnotesHostMinimize.docx",
                Title = "Notes, Endnotes, Host, Minimize - Yes",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.MainPersonNotes),
                    nameof(G2RSettings.SpousesNotes),
                    nameof(G2RSettings.AsEndnotes),
                    nameof(G2RSettings.NotesDividers),
                    nameof(G2RSettings.ConvertDividers),
                    nameof(G2RSettings.IncludeBackRefs),
                    nameof(G2RSettings.IncludeSiblings),
                    nameof(G2RSettings.GenerationPrefix),
                    nameof(G2RSettings.IntroItalic),
                    nameof(G2RSettings.IntroBold),
                    nameof(G2RSettings.UseHostName),
                },
                FlagsOff = new List<string>()
                {
                },
                MustOccur = new List<string>() { @"iii.", "iv.", },
            }.Init();

            _settings.MinimizeFromGeneration = 8;

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());
        }


        [TestMethod]
        public void EndnotesInlineRerefTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A005_EndnotesInlineReref.docx",
                Title = "Endnotes, Inline - Yes",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.AsEndnotes),
                    nameof(G2RSettings.RepeatNoteRefInline),
                    nameof(G2RSettings.UseHostName),
                },
                FlagsOff = new List<string>()
                {
                },
                MustOccur = new List<string>() { },
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());
        }

        [TestMethod]
        public void LineageitalicSummarizemoreIndexoff()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.DescendantReady,
                OutputPath = OutputPath,
                FileName = "D005_LineageitalicSummarizemoreIndexname_On.docx",
                Title = "Lineage italics, Summarize more, Index name - Yes",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.ItalicsNamesInLineageList),
                    nameof(G2RSettings.SummarizeAdditionalCitations),
                },
                FlagsOff = new List<string>()
                {
                },
                MustOccur = new List<string>() { "theNameIndex", "Other sources include", },
                MustNotOccur = new List<string>() { "thePlaceIndex", },
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.NameIndexSettings.Enabled = true;
            _settings.NameIndexSettings.IndexName = "theNameIndex";
            _settings.PlaceIndexSettings.Enabled = false;
            _settings.PlaceIndexSettings.IndexName = "thePlaceIndex";
            _settings.NumberCitesToSumamrize = 4;

            ExecSampleReport(cfg, justDoc);
            Assert.IsTrue(cfg.Eval());

            cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.DescendantReady,
                OutputPath = OutputPath,
                FileName = "D005_LineageitalicSummarizemoreIndexname_Off.docx",
                Title = "Lineage italics, Summarize more, Index name - No",
                FlagsOn = new List<string>()
                {
                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.ItalicsNamesInLineageList),
                    nameof(G2RSettings.SummarizeAdditionalCitations),
                },
                MustOccur = new List<string>() { },
                MustNotOccur = new List<string>() { "thePlaceIndex", "theNameIndex", "Other sources include", },
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.NameIndexSettings.Enabled = false;
            _settings.NameIndexSettings.IndexName = "theNameIndex";
            _settings.PlaceIndexSettings.Enabled = false;
            _settings.PlaceIndexSettings.IndexName = "thePlaceIndex";
            _settings.NumberCitesToSumamrize = 4;

            ExecSampleReport(cfg, justDoc);
            Assert.IsTrue(cfg.Eval());
        }

        [TestMethod]
        public void MiscCasesTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.DescendantReady,
                OutputPath = OutputPath,
                FileName = "D006_MiscCases.docx",
                Title = "Misc Cases",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.ItalicsNamesInLineageList),
                },
                FlagsOff = new List<string>()
                {
                },
                RegexesToAssertFalse = new List<string>() { @"(?-i)\bEugene w\b" },
                MustOccur = new List<string>() {  },
                MustNotOccur = new List<string>() {  },
            }.Init();

            ReadyModel(cfg.ModelState);
            ExecSampleReport(cfg, justDoc);
            Assert.IsTrue(cfg.Eval());
        }

        class OptionCycler
        {
            public ReportConfig ModelConfig { get; set; }
            public List<string> MutuallyExclusive { get; set; }
            public List<string> OptionsOff { get; set; }
            public List<string> OptionsOn { get; set; }
            public string FileNamePattern { get; set; }
            public string TitlePattern { get; set; }
            public bool ProcessAllOffCase { get; set; }

            public IEnumerable<ReportConfig> CycleOptions()
            {
                
                for (int i = 0; i < MutuallyExclusive.Count; i++)
                {
                    ReportConfig rv;
                    if (ProcessAllOffCase && i == 0)
                    {
                        rv = BuildReportConfig(i, "None");
                        rv.FlagsOff.AddRange(MutuallyExclusive);
                        // assume the checks do not 
                        // work with the all-off case?
                        rv.MustNotOccur.Clear();
                        rv.MustOccur.Clear();
                        rv.RegexesToAssertFalse.Clear();
                        rv.RegexesToAssertTrue.Clear();
                        yield return rv;
                    }

                    string uniq = $"{MutuallyExclusive[i]}-On";
                    rv = BuildReportConfig(i, uniq);
                    rv.FlagsOn.Add(MutuallyExclusive[i]);
                    for (int j = 0; j < MutuallyExclusive.Count; j++)
                    {
                        if (j==i) continue;
                        rv.FlagsOff.Add(MutuallyExclusive[j]);
                    }

                    yield return rv;
                }
                yield break;

                ReportConfig BuildReportConfig(int i, string uniq)
                {
                    ReportConfig rv = (ReportConfig) ModelConfig.Clone();
                    rv.FileName = string.Format(FileNamePattern, uniq);
                    rv.Title = string.Format(TitlePattern, uniq);
                    rv.FlagsOff.Clear();
                    rv.FlagsOn.Clear();
                    rv.FlagsOff.AddRange(OptionsOff ??= new List<string>());
                    rv.FlagsOn.AddRange(OptionsOn ??= new List<string>());
                    return rv;
                }
            }
        }

        [TestMethod]
        public void ChildSectionVariationsTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.DescendantReady,
                OutputPath = OutputPath,
                FileName = "D007_AbbrevOn__{0}.docx",
                Title = "Abbreviation on, {0}",
                FlagsOn = new List<string>()
                {
                },
                FlagsOff = new List<string>()
                {
                },
                RegexesToAssertTrue = new List<string>() { @"b[.](?!.*(w:p)).*d[.]", "m[.] first" },
                MustOccur = new List<string>() { },
                MustNotOccur = new List<string>() { },
            }.Init();

            OptionCycler optionCycler = new OptionCycler()
            {
                OptionsOn = new List<string>() {nameof(G2RSettings.AbbreviateChildEvents)},
                MutuallyExclusive = new List<string>()
                {
                    nameof(G2RSettings.ReduceContinuedChildren),
                    nameof(G2RSettings.MinimizeContinuedChildren),
                    nameof(G2RSettings.StandardBriefContinued),
                },
                FileNamePattern = "D007_AbbrevOn__{0}.docx",
                ModelConfig = cfg,
                TitlePattern = "Abbreviation on, {0}",
                ProcessAllOffCase = true,
            };

            foreach (ReportConfig rc in optionCycler.CycleOptions())
            {
                ReadyModel(rc.Init().ModelState);
                ExecSampleReport(rc);
                Assert.IsTrue(rc.Eval());
            }


            optionCycler = new OptionCycler()
            {
                OptionsOff = new List<string>() { nameof(G2RSettings.AbbreviateChildEvents) },
                MutuallyExclusive = new List<string>()
                {
                    nameof(G2RSettings.ReduceContinuedChildren),
                    nameof(G2RSettings.MinimizeContinuedChildren),
                    nameof(G2RSettings.StandardBriefContinued),
                },
                FileNamePattern = "D007_AbbrevOff_{0}.docx",
                ModelConfig = cfg,
                TitlePattern = "Abbreviation off, {0}",
                ProcessAllOffCase = true,
            };
            cfg.RegexesToAssertFalse = cfg.RegexesToAssertTrue;
            cfg.RegexesToAssertTrue = new List<string>();
            cfg.MustOccur = new List<string>(){"married first"};

            foreach (ReportConfig rc in optionCycler.CycleOptions())
            {
                ReadyModel(rc.Init().ModelState);
                ExecSampleReport(rc);
                Assert.IsTrue(rc.Eval());
            }
        }


        [TestMethod]
        public void AncestorBackrefMalformedTextTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A008_AbbreviationOn.docx",
                Title = "Abbreviation - On",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.GenerationPrefix),
                    nameof(G2RSettings.StandardBriefContinued),
                    nameof(G2RSettings.AbbreviateChildEvents),
                    nameof(G2RSettings.IncludeBackRefs),
                    nameof(G2RSettings.IncludeSiblings),
                },
                FlagsOff = new List<string>()
                {
                },
                MustOccur = new List<string>() { "02-2.", "m. Henry Pierce", "m. first" },
                MustNotOccur = new List<string>() { "; Henry Pierce" },
            }.Init();


            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());

            cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A008_AbbreviationOff.docx",
                Title = "Abbreviation - Off",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.GenerationPrefix),
                    nameof(G2RSettings.StandardBriefContinued),
                    nameof(G2RSettings.IncludeBackRefs),
                    nameof(G2RSettings.IncludeSiblings),
                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.AbbreviateChildEvents),
                },
                MustOccur = new List<string>() { "02-2.", "married Henry Pierce", "married first" },
                MustNotOccur = new List<string>() { "  married" },
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());
        }

        [TestMethod]
        public void MalformedMarriedNamesTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A009_MarriedNames.docx",
                Title = "Married Names Test",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.GenerationPrefix),
                    nameof(G2RSettings.StandardBriefContinued),
                    nameof(G2RSettings.AbbreviateChildEvents),
                    nameof(G2RSettings.IncludeBackRefs),
                    nameof(G2RSettings.IncludeSiblings),
                },
                FlagsOff = new List<string>()
                {
                },
                MustOccur = new List<string>() { "02-2.", "m. Henry Pierce", "m. first" },
                MustNotOccur = new List<string>() { "Maud of Flanders () _____" },
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());
        }

        [TestMethod]
        public void WrongGenerationPrefixTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A010_GenerationNumbers.docx",
                Title = "GenerationNumbers Test",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.GenerationPrefix),
                    nameof(G2RSettings.StandardBriefContinued),
                    nameof(G2RSettings.AbbreviateChildEvents),
                    nameof(G2RSettings.IncludeBackRefs),
                    nameof(G2RSettings.IncludeSiblings),
                },
                FlagsOff = new List<string>()
                {
                },
                MustOccur = new List<string>() { "See 39-473520776200." },
                MustNotOccur = new List<string>() { "See 40-473520776200." },
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());
        }

        // She and she ?? can't find this in the output any more

        [TestMethod]
        public void UnlinkedCitationsTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A011_UnlinkedCitationsTest.docx",
                Title = "Unlinked Citations Test",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.GenerationPrefix),
                    nameof(G2RSettings.StandardBriefContinued),
                    nameof(G2RSettings.AbbreviateChildEvents),
                    nameof(G2RSettings.IncludeBackRefs),
                    nameof(G2RSettings.IncludeSiblings),
                },
                FlagsOff = new List<string>()
                {
                },
                MustOccur = new List<string>() { "BUTLER 12", "BUTLER 10" },
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());
        }


        [TestMethod]
        public void OdtTitleAuthorTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A012_TitleAuthorTest.odt",
                Title = "Title/Author Test",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.GenerationPrefix),
                    nameof(G2RSettings.StandardBriefContinued),
                    nameof(G2RSettings.AbbreviateChildEvents),
                    nameof(G2RSettings.IncludeBackRefs),
                    nameof(G2RSettings.IncludeSiblings),
                },
                    FlagsOff = new List<string>()
                    {
                    },
                    RegexesToAssertTrue = new List<string>()
                {
                    @"dc:creator.*?Title/Author Test Author.*?/dc:creator",
                    @"meta:initial-creator.*?Title/Author Test Author.*?/meta:initial-creator",
                    @"dc:title.*?Title/Author Test Title.*?/dc:title",
                },
                RegexesToAssertFalse = new List<string>()
                {
                    "meta:creation-date.*?2021-02-15.*?/meta:creation-date"
                },
            }.Init();

            ReadyModel(cfg.ModelState);
            cfg.Settings.Title = "Title/Author Test Title";
            cfg.Settings.Author = "Title/Author Test Author";
            _settings.Generations = 5;
            ExecSampleReport(cfg, justOdt);
            Assert.IsTrue(cfg.Eval());

        }



        [TestMethod]
        public void DownshiftTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.DescendantReady,
                OutputPath = OutputPath,
                FileName = "D007_Downshift-off.docx",
                Title = "Downshift - Off",
                FlagsOn = new List<string>()
                {
                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.DownshiftNames),
                }, 
                RegexesToAssertTrue = new List<string>() { @"(?-i)\bBARBARA\b.*?BRIGGS\b" },
                MustOccur = new List<string>() { },
                MustNotOccur = new List<string>() { },
            }.Init();

            ReadyModel(cfg.ModelState);
            ExecSampleReport(cfg, justDoc);
            Assert.IsTrue(cfg.Eval());

            cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.DescendantReady,
                OutputPath = OutputPath,
                FileName = "D007_Downshift-on.docx",
                Title = "Downshift - On",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.DownshiftNames),
                },
                FlagsOff = new List<string>()
                {
                },
                RegexesToAssertFalse = new List<string>() { @"(?-i)\bWILMA\b.*?ADAMS\b" },
                MustOccur = new List<string>() { },
                MustNotOccur = new List<string>() { },
            }.Init();

            ReadyModel(cfg.ModelState);
            ExecSampleReport(cfg, justDoc);
            Assert.IsTrue(cfg.Eval());

        }

        [TestMethod]
        public void NameSuffixesTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A013_NameSuffixesTest.docx",
                Title = "Name Suffixes Test",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.GenerationPrefix),
                    nameof(G2RSettings.IncludeBackRefs),
                    nameof(G2RSettings.Placeholders)
                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.OmitFocusSpouses),
                    nameof(G2RSettings.Focus),
                },
                MustOccur = new List<string>() { @"Baudoiun IV", },
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());

        }

        [TestMethod]
        public void EmptyEventsTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A014_EmptyEvents.docx",
                Title = "Empty Events Test",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.OmitFocusSpouses),
                    nameof(G2RSettings.GenerationPrefix),
                    nameof(G2RSettings.IncludeBackRefs),
                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.Focus),
                    nameof(G2RSettings.Placeholders)
                },
                MustNotOccur = new List<string>() { @"She and she died", "She  " },
                CheckPlainText = true,
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());

        }

        [TestMethod]
        public void EmptyEventsTest2()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A015_EmptyEvents2.docx",
                Title = "Empty Events Test 2",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.OmitFocusSpouses),
                    nameof(G2RSettings.GenerationPrefix),
                    nameof(G2RSettings.IncludeBackRefs),
                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.Focus),
                    nameof(G2RSettings.Placeholders)
                },
                // regex is too general, it matches e.g. "She died..."
                //RegexesToAssertFalse = new List<string>() { @"[A-Z][a-z]+\sdied" },
                // there are about 40 specific cases in this test output, 
                // we just need to pick one
                MustNotOccur = new List<string>() { @"Clere died" },
                CheckPlainText = true,
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());

        }


        [TestMethod]
        public void GenerationSpanningPlaceholdersTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A016_GenerationSpanningPlaceholdersTest.docx",
                Title = "Generation Spanning Placeholders Test",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.OmitFocusSpouses),
                    nameof(G2RSettings.GenerationPrefix),
                    nameof(G2RSettings.IncludeBackRefs),
                    nameof(G2RSettings.Placeholders),
                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.Focus),
                },
                MustNotOccur = new List<string>() { @"09-249" },
                MustOccur = new List<string>() { @"08-249" },
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());

        }


        [TestMethod]
        public void SpecialParentheticalWordsTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A017_SpecialParentheticalWordsTest.docx",
                Title = "Special Parenthetical Words Test",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.OmitFocusSpouses),
                    nameof(G2RSettings.GenerationPrefix),
                    nameof(G2RSettings.IncludeBackRefs),
                    nameof(G2RSettings.Placeholders),
                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.Focus),
                },
                MustNotOccur = new List<string>() { @"(Now " },
                MustOccur = new List<string>() { @"(now " },
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());

        }


        [TestMethod]
        public void CitationPlaceholdersTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A018_CitationPlaceholdersTest.docx",
                Title = "Citation Placeholders Test",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.CitationPlaceholders),
                    nameof(G2RSettings.GenerationPrefix),
                    nameof(G2RSettings.IncludeBackRefs),
                    //nameof(G2RSettings.DebuggingOutput),
                    nameof(G2RSettings.GenerationHeadings),

                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.Focus),
                    nameof(G2RSettings.Placeholders),
                },
                //MustNotOccur = new List<string>() { @"(Now " },
                // see 17-112896
                MustOccur = new List<string>() { "***Citation needed*** for: Birth ", "***Citation needed*** for: Marriage " },
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());

        }


        [TestMethod]
        public void CitationPlaceholdersDeferTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A019_CitationPlaceholdersDeferTest.docx",
                Title = "Citation Placeholders Defer Test",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.CitationPlaceholders),
                    nameof(G2RSettings.GenerationPrefix),
                    nameof(G2RSettings.IncludeBackRefs),
                    //nameof(G2RSettings.DebuggingOutput),
                    nameof(G2RSettings.GenerationHeadings),
                    nameof(G2RSettings.DeferConsecutiveRepeats),
                    nameof(G2RSettings.UseSeeNote),

                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.Focus),
                    nameof(G2RSettings.Placeholders),
                },
                //MustNotOccur = new List<string>() { @"(Now " },
                // see 17-112896
                // note this test is inadequate, as it it did not capture the condition
                // of missing cites due to accretion onto non-emitted marriage
                MustOccur = new List<string>() { "***Citation needed*** for: Death of Alice Boleyn" },
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());

        }

        //***Citation needed*** for: Marriage of Charles Chauncey, Sarah Burr
        [TestMethod]
        public void UnseenCitationsTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A020_UnseenCitationsTest.docx",
                Title = "Unseen Citations Test",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.CitationPlaceholders),
                    nameof(G2RSettings.GenerationPrefix),
                    nameof(G2RSettings.IncludeBackRefs),
                    nameof(G2RSettings.DebuggingOutput),
                    nameof(G2RSettings.GenerationHeadings),
                    nameof(G2RSettings.DeferConsecutiveRepeats),
                    nameof(G2RSettings.UseSeeNote),

                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.Focus),
                    nameof(G2RSettings.Placeholders),
                },
                //MustNotOccur = new List<string>() { @"(Now " },
                // see 17-112896
                // note this test is inadequate, as it it did not capture the condition
                // of missing cites due to accretion onto non-emitted marriage
                MustNotOccur = new List<string>() { "***Citation needed*** for: Marriage of Charles Chauncey, Sarah Burr" },
            }.Init();

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());

        }


        [TestMethod]
        public void DoubleParensTest()
        {
            ReportConfig cfg = new ReportConfig()
            {
                Settings = _settings,
                ModelState = ModelState.AncestorReady,
                OutputPath = OutputPath,
                FileName = "A021_DoubleParensTest.docx",
                Title = "Double Parens Test",
                FlagsOn = new List<string>()
                {
                    nameof(G2RSettings.CitationPlaceholders),
                    nameof(G2RSettings.GenerationPrefix),
                    nameof(G2RSettings.IncludeBackRefs),
                    //nameof(G2RSettings.DebuggingOutput),
                    nameof(G2RSettings.GenerationHeadings),
                    nameof(G2RSettings.DeferConsecutiveRepeats),
                    nameof(G2RSettings.UseSeeNote),
                    nameof(G2RSettings.HandleUnknownNames),
                },
                FlagsOff = new List<string>()
                {
                    nameof(G2RSettings.Focus),
                    nameof(G2RSettings.Placeholders),
                },
                //MustNotOccur = new List<string>() { @"(Now " },
                // see 17-112896
                // note this test is inadequate, as it it did not capture the condition
                // of missing cites due to accretion onto non-emitted marriage
                MustNotOccur = new List<string>() { "((" },
            }.Init();
            cfg.Settings.UnknownInSource = "_";
            cfg.Settings.UnknownInReport = "(Unknown)";

            ReadyModel(cfg.ModelState);
            _settings.Generations = 99;
            ExecSampleReport(cfg);
            Assert.IsTrue(cfg.Eval());

        }


        [TestCleanup]
        public void LogResults()
        {
            _logger.Log(_rrm?.Reporter?.GetStatsSummary());
        }
    }


    public enum ModelState
    {
        Unknown,
        AncestorReady,
        DescendantReady
    }
}
