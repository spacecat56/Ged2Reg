using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ged2Reg.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ged2Reg.Model.Tests
{
    [TestClass()]
    public class GenealogicalDateFormatterTests
    {
        //[TestMethod()]
        //public void ParseYearTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void GenealogicalDateFormatterTest()
        //{
        //    Assert.Fail();
        //}

        [TestMethod()]
        public void ReformatTest()
        {
            G2RSettings settings = new G2RSettings().Init();
            ReportContext rc = ReportContext.Init(settings);
            //ContentReformatter crfDateBeforeAfter =
            //    settings.DateRules.Find(crf => crf.Role == PatternRole.DateAboutBeforeAfter);
            GenealogicalDateFormatter gdf = new GenealogicalDateFormatter();

            string[][] cases =
            {
                new []{"BEF 33 CE", "before 33 CE"},
                new []{"AFT 2 AD", "after 2 AD"},
                new []{"ABT 323 BC", "about 323 BC"},
                new []{"03 MAR 79 BC", "on 03 March 79 BC"},
                new []{"13 DEC 1844", "on 13 December 1844"},
                new []{"ABT 01 MAR 1665/6", "about 01 March 1665/6"},
                new []{"BEF JUL 1776", "before July 1776"},
                new []{"BET 1776 AND 1781", "between 1776 and 1781"},
                //Abt 400 BC failing 'in situ'
                new []{ "ABT 400 BC", "about 400 BC"},
                new []{ "Abt 400 BC", "about 400 BC"},
            };

            foreach (string[] cayse in cases)
            {
                string result = gdf.Reformat(cayse[0]);
                Assert.AreEqual(cayse[1],result);
            }

        }

        [TestMethod()]
        public void LongMonthNameTest()
        {
            G2RSettings settings = new G2RSettings().Init();
            ReportContext rc = ReportContext.Init(settings);
            GenealogicalDateFormatter gdf = new GenealogicalDateFormatter();
            string input = "7 April 1823";
            string expected = "on 7 April 1823";
            string result = gdf.Reformat(input);
            Assert.AreEqual(expected, result);
        }

        [TestMethod()]
        public void UntrimmedTest()
        {
            G2RSettings settings = new G2RSettings().Init();
            ReportContext rc = ReportContext.Init(settings);
            GenealogicalDateFormatter gdf = new GenealogicalDateFormatter();
            string input = "10 September 1910 ";
            string expected = "on 10 September 1910";
            string result = gdf.Reformat(input);
            Assert.AreEqual(expected, result);
        }

        // 

        [TestMethod()]
        public void NoSpaceTest()
        {
            G2RSettings settings = new G2RSettings().Init();
            ReportContext rc = ReportContext.Init(settings);
            GenealogicalDateFormatter gdf = new GenealogicalDateFormatter();
            string input = "31Jul 1880";
            string expected = "on 31 July 1880";
            string result = gdf.Reformat(input);
            Assert.AreEqual(expected, result);
        }

    }

}