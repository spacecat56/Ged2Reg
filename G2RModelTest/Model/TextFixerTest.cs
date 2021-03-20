﻿using G2RModel.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace G2RModelTest.Model
{
    [TestClass]
    public class TextFixerTest
    {
        private TextFixer _nameFixer = new TextFixer()
        {
            FinderText = @"(?-i)(?<hit>([(]((Mrs.)|(\d[a-z]{2})).*?[)]))",
            Fixer = "_____"
        }.Init();

        private TextFixer _spaceFixer = new TextFixer()
        {
            FinderText = @"(?i)(?<part1>^.*?\d{4})(?<part2>[a-z]{2,}.*$)",
            Fixer = "${part1} ${part2}"
        }.Init();
        //\d{4}[a-z]{2,}

        [TestMethod]
        public void SampleNameFixerTests()
        {
            string[] nameExamples = new[]
            {
                "(Mrs. Stephen Rhodes)",
                "(3rd wife of Washington Mackey)",
                "(Mrs. Orsmar Hollister Mackey)",
                "(2nd Mrs. Aaron Mackey)",
            };

            string expected = "_____";

            foreach (string example in nameExamples)
            {
                Assert.AreEqual(expected, _nameFixer.Exec(example));
            }
        }

        [TestMethod]
        public void InvalidRegexTest()
        {
            TextFixer tf = new TextFixer()
            {
                FinderText = @"({this is not valid",
                Fixer = "no matter"
            };
            TextFixer tfNull = tf.Init();

            Assert.IsNull(tfNull);
            Assert.IsFalse(tf.IsValid);
            Assert.AreEqual("anything", tf.Exec("anything"));
            Assert.IsNull(new TextFixer().Init());
        }

        [TestMethod]
        public void SampleNospaceFixerTests()
        {
            string[] example = new[]
            {
                "1830and",
                "1825given",
                "1822she",
                "therefore b. bet. 1821-1825given the spacing ",
            };
            string[] expected = new[]
            {
                "1830 and",
                "1825 given",
                "1822 she",
                "therefore b. bet. 1821-1825 given the spacing ",
            };

            for (int i = 0; i < example.Length; i++)
            {
                Assert.AreEqual(expected[i], _spaceFixer.Exec(example[i]));
            }
        }
    }
}
