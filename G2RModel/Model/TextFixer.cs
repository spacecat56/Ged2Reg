// TextFixer.cs
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
using System.Text.RegularExpressions;

namespace G2RModel.Model
{
    public class TextFixer
    {
        public Regex Finder { get; private set; }
        public string Fixer { get; set; }
        public string FinderText { get; set; }
        public bool IsValid { get; set; }

        public TextFixer Init()
        {
            if (string.IsNullOrEmpty(FinderText) || string.IsNullOrEmpty(Fixer))
            {
                IsValid = false;
                return null;
            }
            try
            {
                Finder = new Regex(FinderText);
                var dummy = Finder.Match("success is irrelevant");
                //var g = dummy.Groups["hit"];
                IsValid = true;
                return this;
            }
            catch (Exception ex)
            {
                IsValid = false;
                return null;
            }
        }

        public string Exec(string victim)
        {
            if (!IsValid && Init() == null)
                return victim;

            if (string.IsNullOrEmpty(victim))
                return victim;

            try
            {
                Match m = Finder.Match(victim);
                return !m.Success ? victim : m.Result(Fixer);
            }
            catch
            {
                return victim;
            }
        }

    }
}
