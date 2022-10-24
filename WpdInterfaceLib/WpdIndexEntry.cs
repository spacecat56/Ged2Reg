// WpdIndexEntry.cs
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

namespace WpdInterfaceLib
{
    public abstract class WpdIndexEntry : WpdFieldBase
    {
        public string IndexValue { get; set; }
        public string IndexName { get; set; }
        public string SeeInstead { get; set; }


        protected WpdIndexEntry(IWpdDocument document) : base(document) { }
    }
}