// OoxBuilders.cs
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

using DocumentFormat.OpenXml.Wordprocessing;
using WpdInterfaceLib;

namespace DocxAdapterLib
{
    public static class OoxBuilders
    {
        public static RunProperties BuildRunProperties(Formatting fmt)
        {
            var rv = new RunProperties();
            if (fmt == null)
                return rv;

            if (!string.IsNullOrEmpty(fmt.CharacterStyleName))
                rv.AppendChild(new RunStyle() { Val = fmt.CharacterStyleName });

            if (fmt.Bold ?? false)
                rv.AppendChild(new Bold());

            if (fmt.Italic ?? false)
                rv.AppendChild(new Italic());

            // todo: more properties other than style?

            return rv;
        }
    }
}
