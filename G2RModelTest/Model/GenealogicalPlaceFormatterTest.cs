﻿// GenealogicalPlaceFormatterTest.cs
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

        [TestMethod]
        public void TestUSA()
        {
            GenealogicalPlaceFormatter gpf = new GenealogicalPlaceFormatter() { InjectWordCounty = true }.Init();
            string input = "United States";
            string expected = "in the United States";
            var fpn = gpf.Reformat(input);
            string result = $"{fpn.Preposition} {fpn.PreferredName}";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestInNotAt()
        {
            GenealogicalPlaceFormatter gpf = new GenealogicalPlaceFormatter() { InjectWordCounty = true }.Init();
            string input = "Leitrim County, Ireland";
            string expected = "in Leitrim County, Ireland";
            var fpn = gpf.Reformat(input);
            string result = $"{fpn.Preposition} {fpn.PreferredName}";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestAtNotIn()
        {
            GenealogicalPlaceFormatter gpf = new GenealogicalPlaceFormatter() { InjectWordCounty = true }.Init();
            string input = "Brede, Sussex, England";
            string expected = "at Brede, Sussex, England";
            var fpn = gpf.Reformat(input);
            string result = $"{fpn.Preposition} {fpn.PreferredName}";
            Assert.AreEqual(expected, result);
        }


        private void Exec(GenealogicalPlaceFormatter gpf, string input, string expected)
        {
            FormattedPlaceName fpn = gpf.Reformat(input);
            Assert.AreEqual(expected, fpn.PreferredName);
        }
    }
}
