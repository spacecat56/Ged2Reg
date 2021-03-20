using G2RModel.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace G2RModelTest.Model
{
    [TestClass]
    public class GenealogicalNameFormatterTests
    {

        class NameCase
        {
            public string Name { get; set; }
            public string Givn { get; set; }
            public string Surn { get; set; }
            public string GivenNames { get; set; }
            public string Surname { get; set; }
            public GenealogicalNameFormatter FormattedName { get; set; }
        }


        [TestMethod]
        public void SimpleNameShiftTest()
        {
            GenealogicalNameFormatter.SetPolicies(true, true, "_", "_____");

            NameCase[] cases = new[]
            {
                new NameCase() {Name = "Robert /Jones/", Givn = null, Surn = null, GivenNames = "Robert", Surname = "Jones"},
                new NameCase() {Name = "Robert E /Jones/", Givn = null, Surn = null, GivenNames = "Robert E", Surname = "Jones"},
                new NameCase() {Name = "Robert E. /Jones/", Givn = null, Surn = null, GivenNames = "Robert E.", Surname = "Jones"},
                new NameCase() {Name = "ROBERT E /JONES/", Givn = null, Surn = null, GivenNames = "Robert E", Surname = "Jones"},
                new NameCase() {Name = "ROBERT E. /JONES/", Givn = null, Surn = null, GivenNames = "Robert E.", Surname = "Jones"},
                new NameCase() {Name = "ROBERT E.N. /JONES/", Givn = null, Surn = null, GivenNames = "Robert E.N.", Surname = "Jones"},
                new NameCase() {Name = "ROBERT E. N. /JONES/", Givn = null, Surn = null, GivenNames = "Robert E. N.", Surname = "Jones"},
                new NameCase() {Name = "_ /JONES/", Givn = null, Surn = null, GivenNames = "_____", Surname = "Jones"},
            };

            ExecTests(cases);
        }

        private void ExecTests(NameCase[] cases)
        {
            foreach (NameCase nc in cases)
            {
                var name = GenealogicalNameFormatter.Reformat(nc.Name, nc.Givn, nc.Surn);
                Assert.AreEqual(nc.GivenNames, name.GivenNames, $"case: {nc.Name}");
                Assert.AreEqual(nc.Surname, name.Surname, $"case: {nc.Name}");
                nc.FormattedName = name;
            }
        }

        [TestMethod]
        public void SimpleNameUnshiftTest()
        {
            GenealogicalNameFormatter.SetPolicies(false, true, "_", "_____");

            NameCase[] cases = new[]
            {
                new NameCase() {Name = "Robert /Jones/", Givn = null, Surn = null, GivenNames = "Robert", Surname = "Jones"},
                new NameCase() {Name = "Robert E /Jones/", Givn = null, Surn = null, GivenNames = "Robert E", Surname = "Jones"},
                new NameCase() {Name = "Robert E. /Jones/", Givn = null, Surn = null, GivenNames = "Robert E.", Surname = "Jones"},
                new NameCase() {Name = "ROBERT E /JONES/", Givn = null, Surn = null, GivenNames = "ROBERT E", Surname = "JONES"},
                new NameCase() {Name = "ROBERT E. /JONES/", Givn = null, Surn = null, GivenNames = "ROBERT E.", Surname = "JONES"},
                new NameCase() {Name = "_ /JONES/", Givn = null, Surn = null, GivenNames = "_____", Surname = "JONES"},
            };
            ExecTests(cases);
        }

        [TestMethod]
        public void CompoundNameShiftTest()
        {
            GenealogicalNameFormatter.SetPolicies(true, true, "_", "_____");

            NameCase[] cases = new[]
            {
                new NameCase() {Name = "Robert /de Jones/", Givn = null, Surn = null, GivenNames = "Robert", Surname = "de Jones"},
                new NameCase() {Name = "Robert E /de Jones/", Givn = null, Surn = null, GivenNames = "Robert E", Surname = "de Jones"},
                new NameCase() {Name = "Robert E. /de la Jones/", Givn = null, Surn = null, GivenNames = "Robert E.", Surname = "de la Jones"},
                new NameCase() {Name = "ROBERT E /DE JONES/", Givn = null, Surn = null, GivenNames = "Robert E", Surname = "de Jones"},
                new NameCase() {Name = "ROBERT E /DE LA JONES/", Givn = null, Surn = null, GivenNames = "Robert E", Surname = "de la Jones"},
                new NameCase() {Name = "ROBERT E /ST JONES/", Givn = null, Surn = null, GivenNames = "Robert E", Surname = "St Jones"},
                new NameCase() {Name = "ROBERT E /ST. JONES/", Givn = null, Surn = null, GivenNames = "Robert E", Surname = "St. Jones"},
                new NameCase() {Name = "ROBERT E /SAINT JONES/", Givn = null, Surn = null, GivenNames = "Robert E", Surname = "Saint Jones"},
                new NameCase() {Name = "ROBERT E. /DES JONES/", Givn = null, Surn = null, GivenNames = "Robert E.", Surname = "des Jones"},
                new NameCase() {Name = "ROBERT E. /A JONES/", Givn = null, Surn = null, GivenNames = "Robert E.", Surname = "a Jones"},
                new NameCase() {Name = "_ /DE JONES/", Givn = null, Surn = null, GivenNames = "_____", Surname = "de Jones"},
            };
            ExecTests(cases);
        }

        [TestMethod]
        public void CompoundNameUnshiftTest()
        {
            GenealogicalNameFormatter.SetPolicies(false, true, "_", "_____");

            NameCase[] cases = new[]
            {
                new NameCase() {Name = "Robert /de Jones/", Givn = null, Surn = null, GivenNames = "Robert", Surname = "de Jones"},
                new NameCase() {Name = "Robert E /de Jones/", Givn = null, Surn = null, GivenNames = "Robert E", Surname = "de Jones"},
                new NameCase() {Name = "Robert E. /de la Jones/", Givn = null, Surn = null, GivenNames = "Robert E.", Surname = "de la Jones"},
                new NameCase() {Name = "ROBERT E /DE JONES/", Givn = null, Surn = null, GivenNames = "ROBERT E", Surname = "DE JONES"},
                new NameCase() {Name = "ROBERT E /DE LA JONES/", Givn = null, Surn = null, GivenNames = "ROBERT E", Surname = "DE LA JONES"},
                new NameCase() {Name = "ROBERT E /ST JONES/", Givn = null, Surn = null, GivenNames = "ROBERT E", Surname = "ST JONES"},
                new NameCase() {Name = "ROBERT E /ST. JONES/", Givn = null, Surn = null, GivenNames = "ROBERT E", Surname = "ST. JONES"},
                new NameCase() {Name = "ROBERT E /SAINT JONES/", Givn = null, Surn = null, GivenNames = "ROBERT E", Surname = "SAINT JONES"},
                new NameCase() {Name = "ROBERT E. /DES JONES/", Givn = null, Surn = null, GivenNames = "ROBERT E.", Surname = "DES JONES"},
                new NameCase() {Name = "_ /DE JONES/", Givn = null, Surn = null, GivenNames = "_____", Surname = "DE JONES"},
            };
            ExecTests(cases);
        }

        [TestMethod]
        public void SubnameNameShiftTest()
        {
            GenealogicalNameFormatter.SetPolicies(true, true, "_", "_____");
            // if provided, the pre-split name parts have priority
            NameCase[] cases = new[]
            {
                new NameCase() {Name = "Robert /de Jones/", Givn = "Bobby", Surn = "Jonesy", GivenNames = "Bobby", Surname = "Jonesy"},
                new NameCase() {Name = "ROBERT E /DE JONES/", Givn = "BOBBY", Surn = "JONESY", GivenNames = "Bobby", Surname = "Jonesy"},
                new NameCase() {Name = "ROBERT E /DE JONES/", Givn = "BOBBY", Surn = "DE JONESY", GivenNames = "Bobby", Surname = "de Jonesy"},
                new NameCase() {Name = "ROBERT E /DE JONES/", Givn = "BOBBY", Surn = "de JONESY", GivenNames = "Bobby", Surname = "de JONESY"}, // twist here
            };
            ExecTests(cases);
        }

        [TestMethod]
        public void SurnameOnlyTest()
        {
            GenealogicalNameFormatter.SetPolicies(true, true, "_", "_____");
            // if provided, the pre-split name parts have priority
            NameCase[] cases = new[]
            {
                new NameCase() {Name = "/INGLETRUDE/", Givn = null, Surn = null, GivenNames = "_____", Surname = "Ingletrude"},
            };
            ExecTests(cases);
        }

        [TestMethod]
        public void McNamesTest()
        {
            GenealogicalNameFormatter.SetPolicies(true, true, "_", "_____");

            // let's first make sure the classification is correct
            var gnf = GenealogicalNameFormatter.Reformat("Robert /MacNeary/", null, null);
            Assert.IsTrue(gnf.IsMacName);
            gnf = GenealogicalNameFormatter.Reformat("Robert /Neary/", null, null);
            Assert.IsFalse(gnf.IsMacName);

            NameCase[] cases = new[]
            {
                new NameCase() {Name = "Robert E /McJones/", Givn = null, Surn = null, GivenNames = "Robert E", Surname = "McJones"},
                new NameCase() {Name = "Robert E. /McJones/", Givn = null, Surn = null, GivenNames = "Robert E.", Surname = "McJones"},
                new NameCase() {Name = "ROBERT E /MCJONES/", Givn = null, Surn = null, GivenNames = "Robert E", Surname = "McJones"},
                new NameCase() {Name = "ROBERT E /MACJONES/", Givn = null, Surn = null, GivenNames = "Robert E", Surname = "MacJones"},
                new NameCase() {Name = "ROBERT E. /MCJONES/", Givn = null, Surn = null, GivenNames = "Robert E.", Surname = "McJones"},
                new NameCase() {Name = "ROBERT E. /McJONES/", Givn = null, Surn = null, GivenNames = "Robert E.", Surname = "McJones"},
                new NameCase() {Name = "ROBERT E. /MacJONES/", Givn = null, Surn = null, GivenNames = "Robert E.", Surname = "MacJones"},
                new NameCase() {Name = "ROBERT E. /Mc JONES/", Givn = null, Surn = null, GivenNames = "Robert E.", Surname = "Mc Jones"},
                new NameCase() {Name = "ROBERT E. /Mac JONES/", Givn = null, Surn = null, GivenNames = "Robert E.", Surname = "Mac Jones"},
                new NameCase() {Name = "JOHN /MACKEY/", Givn = null, Surn = null, GivenNames = "John", Surname = "Mackey"},
                new NameCase() {Name = "JOHN /MACK/", Givn = null, Surn = null, GivenNames = "John", Surname = "Mack"},
            };
            ExecTests(cases);
        }


        [TestMethod]
        public void NullNamesTest()
        {
            GenealogicalNameFormatter.SetPolicies(true, true, "_", "_____");

            NameCase[] cases = new[]
            {
                new NameCase() {Name = null, Givn = null, Surn = null, GivenNames = "_____", Surname = null},
            };
            ExecTests(cases);
            Assert.IsTrue(cases[0].FormattedName.UnknownGivenName);
            // hmm.  are these other conditions needed?
            //Assert.IsTrue(cases[0].FormattedName.NoSurname);
            //Assert.IsTrue(cases[0].FormattedName.UnknownName);
        }
    }
}
