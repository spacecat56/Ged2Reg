using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Ged2Reg.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace G2RModelTest.Model
{
    public class ReportConfig : ICloneable
    {
        private static Dictionary<string, PropertyInfo> Props;

        static ReportConfig()
        {
            PropertyInfo[] ps = typeof(G2RSettings).GetProperties();
            Props = ps.ToDictionary(p => p.Name, p => p);
        }

        public G2RSettings Settings { get; set; }
        public string Title { get; set; }
        public string OutputPath { get; set; }
        public string FileName { get; set; }
        public ModelState ModelState { get; set; }
        public List<string> FlagsOn { get; set; }
        public List<string> FlagsOff { get; set; }
        public List<string> MustOccur { get; set; }
        public List<string> MustNotOccur { get; set; }
        public List<string> RegexesToAssertTrue { get; set; }
        public List<string> RegexesToAssertFalse { get; set; }
        public bool RestrictRexToMainDoc { get; set; }
        public int MinSize { get; set; } = 1024;

        public ReportConfig Init(bool conform = true, bool asRegister = true)
        {
            Settings.OutFile = Path.Combine(OutputPath, FileName);
            Settings.Title = Title;

            if (conform) Settings.ConformToRegister(asRegister);

            foreach (string fOn in FlagsOn ??= new List<string>())
            {
                Props[fOn].SetValue(Settings, true);
            }

            foreach (string fOff in FlagsOff ??= new List<string>())
            {
                Props[fOff].SetValue(Settings, false);
            }

            return this;
        }

        public bool Eval()
        {
            Assert.IsTrue(File.Exists(Settings.OutFile));
            DocFile doc = new DocFile().Init(Settings.OutFile);
            //string txt = File.ReadAllText(Settings.OutFile);
            Assert.IsTrue(doc.FullText.Length >= MinSize);
            foreach (string s in MustOccur ?? new List<string>())
            {
                Assert.IsTrue(doc.FullText.IndexOf(s) >= 0, $"Expected {s} not found in {FileName}");
            }
            foreach (string s in MustNotOccur ?? new List<string>())
            {
                Assert.IsTrue(doc.FullText.IndexOf(s) < 0, $"Unexpected {s} was found in {FileName}");
            }

            string rexTxt = RestrictRexToMainDoc ? doc.MainText : doc.FullText;

            foreach (string rex in RegexesToAssertTrue ??= new List<string>())
            {
                Assert.IsTrue(Regex.IsMatch(rexTxt, rex), $"Failed asserting true: '{rex}'");
            }

            foreach (string rex in RegexesToAssertFalse ??= new List<string>())
            {
                Assert.IsFalse(Regex.IsMatch(rexTxt, rex), $"Failed asserting false: '{rex}'");
            }

            return true;
        }

        #region Implementation of ICloneable

        public object Clone()
        {
            ReportConfig other = new ReportConfig()
            {
                FileName = FileName,
                FlagsOff = FlagsOff ?? new List<string>(),
                FlagsOn = FlagsOn ?? new List<string>(),
                MinSize = MinSize,
                ModelState = ModelState,
                MustNotOccur = MustNotOccur ?? new List<string>(),
                MustOccur = MustOccur ?? new List<string>(),
                OutputPath = OutputPath,
                RegexesToAssertFalse = RegexesToAssertFalse ?? new List<string>(),
                RegexesToAssertTrue = RegexesToAssertTrue ?? new List<string>(),
                RestrictRexToMainDoc = RestrictRexToMainDoc,
                Settings = Settings,
                Title = Title
            };
            return other;
        }

        #endregion
    }
}