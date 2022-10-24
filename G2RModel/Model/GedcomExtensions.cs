// GedcomExtensions.cs
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

using SimpleGedcomLib;

namespace G2RModel
{
    public static class GedcomExtensions
    {
        public static string ReAssemble(this Tag t)
        {
            string id = t.Id == null ? null : $" {t.Id}";
            string content = t.Content == null ? null : $" {t.Content}";
            return $"{t.Level}{id} {t.Code}{content}";
        }

        public static string GetEditedCitationText(this Tag t)
        {
            if (t?.Code != TagCode.SOUR)
                return null;
            Tag edCite = t.GetFirstDescendant(TagCode._FOOT);
            string rv = edCite?.FullText();
            return rv;
        }
    }
}