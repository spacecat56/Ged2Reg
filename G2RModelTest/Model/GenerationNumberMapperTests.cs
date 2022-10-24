// GenerationNumberMapperTests.cs
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

using G2RModel.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace G2RModelTest.Model
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