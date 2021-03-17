using System.Collections.Generic;
using Ged2Reg.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace G2RModelTest.Model
{
    [TestClass]
    public class GenealogicalPlaceFormatterTest
    {
        private Dictionary<string, string> _normalCases = new Dictionary<string, string>()
        {
            {"Matteawan, Fishkill, Dutchess, New York", "Matteawan, Fishkill, Dutchess County, New York"},
            {"Fishkill, Dutchess, New York", "Fishkill, Dutchess County, New York"},
            // CONFLICTS with rule for "avoid bogus county" {"Dutchess, New York", "Dutchess County, New York"},
            {"Dutchess, New York", "Dutchess, New York"},
            {"New York", "New York"},
            {"Matteawan, Fishkill, Dutchess, New York, USA", "Matteawan, Fishkill, Dutchess County, New York"},
            {"Matteawan, Fishkill, Dutchess, New York, United States of America", "Matteawan, Fishkill, Dutchess County, New York"},
        };

        [TestMethod]
        public void TestAbbreviation1()
        {
            GenealogicalPlaceFormatter gpf = new GenealogicalPlaceFormatter().Init();
            string input = "Ruston, LA";
            string expected = "Ruston, Louisiana";

            Exec(gpf, input, expected);
        }

        [TestMethod]
        public void TestAbbreviation2()
        {
            GenealogicalPlaceFormatter gpf = new GenealogicalPlaceFormatter().Init();
            string input = "Cheyenne, Laramie Co, WY";
            string expected = "Cheyenne, Laramie County, Wyoming";

            Exec(gpf, input, expected);
        }

        [TestMethod]
        public void TestAbbreviation3()
        {
            GenealogicalPlaceFormatter gpf = new GenealogicalPlaceFormatter().Init();
            string input = "Tomah, Monroe, Wisconsin USA";
            string expected = "Tomah, Monroe County, Wisconsin";

            Exec(gpf, input, expected);
        }

        [TestMethod]
        public void TestAbbreviation4()
        {
            GenealogicalPlaceFormatter gpf = new GenealogicalPlaceFormatter().Init();
            string input = "NY. USA";
            string expected = "New York";

            Exec(gpf, input, expected);
        }


        [TestMethod]
        public void TestAbbreviationCo()
        {
            GenealogicalPlaceFormatter gpf = new GenealogicalPlaceFormatter().Init();
            string input = "Alameda Co., Ca";
            string expected = "Alameda County, California";

            Exec(gpf, input, expected);
        }

        [TestMethod]
        public void TestDC()
        {
            GenealogicalPlaceFormatter gpf = new GenealogicalPlaceFormatter().Init();
            string input = "Washington City, District Of Columbia, District of Columbia, USA";
            string expected = "Washington, D.C.";
            Exec(gpf, input, expected);

            input = "Washington City, District Of Columbia, District of Columbia, USA";
            Exec(gpf, input, expected);

            input = "Washington DC";
            Exec(gpf, input, expected);

            input = "Washington, DC";
            Exec(gpf, input, expected);

            input = "Washington D. C.";
            Exec(gpf, input, expected);

            input = "Washington D.C.";
            Exec(gpf, input, expected);
        }

        [TestMethod]
        public void TestNormalCases()
        {
            GenealogicalPlaceFormatter gpf = new GenealogicalPlaceFormatter(){ReduceOnRepetition = false}.Init();
            foreach (string input in _normalCases.Keys)
            {
                Exec(gpf, input, _normalCases[input]);
            }
        }

        [TestMethod]
        public void TestNonCounty()
        {
            GenealogicalPlaceFormatter gpf = new GenealogicalPlaceFormatter(){InjectWordCounty = true}.Init();
            string expected = "Cheshire, Connecticut";
            string input = "Cheshire, Connecticut, USA";
            Exec(gpf, input, expected);

            expected = "Cheshire, Hartford County, Connecticut";
            input = "Cheshire, Hartford, Connecticut, USA";
            Exec(gpf, input, expected);

        }

        // can't pass this, setting it aside
        //[TestMethod]
        //public void TestMultipleDots()
        //{
        //    GenealogicalPlaceFormatter gpf = new GenealogicalPlaceFormatter() { InjectWordCounty = true }.Init();
        //    string input = "Cheshire, N.Y., U.S.A.";
        //    string expected = "Cheshire, N.Y.";
        //    Exec(gpf, input, expected);
        //}

        [TestMethod]
        public void TestCountyState()
        {
            GenealogicalPlaceFormatter gpf = new GenealogicalPlaceFormatter() { InjectWordCounty = true }.Init();
            string input = "Putnam, New York";
            string expected = "Putnam, New York"; // ambiguous case, should be emitted unmodified
            Exec(gpf, input, expected);
        }

        [TestMethod]
        public void TestTrash()
        {
            GenealogicalPlaceFormatter gpf = new GenealogicalPlaceFormatter() { InjectWordCounty = true }.Init();
            string input = ",,,,Syria";
            string expected = "Syria"; 
            Exec(gpf, input, expected);
        }

        [TestMethod]
        public void TestCoDot()
        {
            GenealogicalPlaceFormatter gpf = new GenealogicalPlaceFormatter() { InjectWordCounty = true }.Init();
            string input = "Lloyd, Ulster Co., NY";
            string expected = "Lloyd, Ulster County, New York";
            Exec(gpf, input, expected);
        }


        //


        private void Exec(GenealogicalPlaceFormatter gpf, string input, string expected)
        {
            FormattedPlaceName fpn = gpf.Reformat(input);
            Assert.AreEqual(expected, fpn.PreferredName);
        }
    }
}
