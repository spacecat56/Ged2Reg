using Microsoft.VisualStudio.TestTools.UnitTesting;
using G2RModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G2RModel.Model.Tests
{
    [TestClass()]
    public class GenerationNumberMapperTests
    {
        [TestMethod()]
        public void InitTestB()
        {
            GenerationNumberMapper gm 
                = GenerationNumberMapper.Instance.Init(99, "B");

            string result = gm.GenerationNumberFor(2);
            Assert.AreEqual("A", result);

            result = gm.GenerationNumberFor(10);
            Assert.AreEqual("8", result);

        }

        [TestMethod()]
        public void InitTestNull()
        {
            GenerationNumberMapper gm
                = GenerationNumberMapper.Instance.Init(99, null);

            string result = gm.GenerationNumberFor(2);
            Assert.AreEqual("2", result);

            result = gm.GenerationNumberFor(10);
            Assert.AreEqual("10", result);
        }

        [TestMethod()]
        public void InitTest0()
        {
            GenerationNumberMapper gm
                = GenerationNumberMapper.Instance.Init(99, "0");

            string result = gm.GenerationNumberFor(1);
            Assert.AreEqual("1", result);

        }
    }
}