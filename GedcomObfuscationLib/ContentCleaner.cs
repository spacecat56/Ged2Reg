﻿// ContentCleaner.cs
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

using System.Text.RegularExpressions;

namespace GedcomObfuscationLib
{
    public class ContentCleaner
    {
        Regex _urlRex = new Regex(@"(?i)\b(?<root>https?://.*?[.][a-z]+)/\S+\b");
        public int TextsChanged { get; set; }
        public string PruneURLs(string text)
        {
            string rv = _urlRex.Replace(text, "${root}");
            if (rv != text)
                TextsChanged++;
            return rv;
        }
    }
}
