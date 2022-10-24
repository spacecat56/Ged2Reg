// AbbreviationManager.cs
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

namespace G2RModel.Model
{
    public static class AbbreviationManager
    {
        public static bool AbbreviationsForChildEvents { get; set; }

        public static string TextFor(TagCode tc, bool isChild = false, string prefix = null)
        {
            bool abbr = isChild && AbbreviationsForChildEvents;
            switch (tc)
            {
                case TagCode.BIRT:
                    return abbr ? "b." : $"{prefix}born";
                case TagCode.BAPM:
                case TagCode.CHR:
                    return abbr ? "bp." : $"{prefix}baptized";
                case TagCode.MARR:
                    return abbr ? "m." : $"{prefix}married";
                case TagCode.DIV:
                    return abbr ? "div." : $"{prefix}divorced";
                case TagCode.DEAT:
                    return abbr ? "d." : $"{prefix}died";
                case TagCode.BURI:
                    return abbr ? "bd." : $"{prefix}buried";
                default:
                    return tc.Map().ToString();
            }
        }

    }
}
