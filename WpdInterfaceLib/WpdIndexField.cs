// WpdIndexField.cs
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
    public abstract class WpdIndexField : WpdFieldBase
    {
        public static string UpdateFieldPrompt { get; set; } = "Right-click and Update (this) field to generate the index";
        public int Columns { get; set; }
        public string IndexName { get; set; }
        public string EntryPageSeparator { get; set; }
        public string Heading { get; set; } = "Index";
        public bool SingleIndexOnly { get; set; }

        protected WpdIndexField(IWpdDocument document) : base(document) { }
    }
}