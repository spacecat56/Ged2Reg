// ReportContext.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ged2Reg.Model
{
    public class ReportContext
    {
        private static ReportContext _theInstance;
        public static ReportContext Instance => _theInstance;

        public static ReportContext Init(G2RSettings settings)
        {
            return _theInstance = new ReportContext() {Settings = settings};
        }

        public G2RSettings Settings { get; private set; }

        //public int LastPersonNumber { get; set; }

        //public int CurrentGeneration { get; set; }

        //public Dictionary<string, string> MappedNames { get; } = new Dictionary<string, string>();

        public RegisterReportModel Model { get; set; }


    }
}
