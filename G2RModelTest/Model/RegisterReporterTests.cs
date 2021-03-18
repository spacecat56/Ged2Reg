using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        public string OutputPath = @"c:\test\ged2reg";
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
            _settings = new G2RSettings().Init().ApplyVersionUpdates();
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
            ExecSampleReport(cfg);
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
