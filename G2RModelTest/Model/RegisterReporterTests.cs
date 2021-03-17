using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocxAdapterLib;
using G2RModel.Model;
using Ged2Reg.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        enum ModelState
        {
            Unknown,
            AncestorReady,
            DescendantReady
        }

        private string _outputPath = @"c:\test\ged2reg";

        G2RSettings _settings;
        ReportContext _rc;
        private RegisterReportModel _rrm;
        private TestLogger _logger;
        private AsyncActionDelegates _aad;

        private ModelState _currentModelState;

        [TestInitialize]
        public void InitModel()
        {
            _settings = new G2RSettings().Init();
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
            Directory.CreateDirectory(_outputPath);
        }

        private bool ReadyModel(ModelState needed)
        {
            if (_currentModelState == needed)
                return true;
            _settings.AncestorsReport = needed == ModelState.AncestorReady;
            _rrm.ConfigureSampleFile(RegisterReportModel.PathToFileResource("sample.ged"));
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
            _settings.OutFile = Path.Combine(_outputPath, "DescendantTest-01.docx");
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

        [TestMethod]
        public void DescendantTest2()
        {
            _settings.ConformToRegister();
            _settings.UseSeeNote = true;
            _settings.UseHostName = true;
            ReadyModel(ModelState.DescendantReady);
            _settings.OutFile = Path.Combine(_outputPath, "DescendantTest-02.docx");
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
            _settings.OutFile = Path.Combine(_outputPath, "AncestorsTest-01.docx");
            ExecSampleReport();
            Assert.IsTrue(File.Exists(_settings.OutFile));
        }
    }
}
