// GenealogicalDateFormatter.cs
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
using System.Globalization;
using System.Text.RegularExpressions;

namespace Ged2Reg.Model
{

    public class ContentReformatter
    {
        public PatternRole Role { get; set; }
        public string RecognizerPattern { get; set; }
        public string Emitter { get; set; }
        public string ShortEmitter { get; set; }

        private Regex _extractor;
        public Regex Extractor => _extractor ?? (_extractor = new Regex(RecognizerPattern));

    }

    public class GenealogicalDateFormatter
    {
        private static GenealogicalDateFormatter _instance;

        public static GenealogicalDateFormatter Instance
        {
            get => _instance ??= new GenealogicalDateFormatter();
            set => _instance = value;
        }

        public static int ParseYear(string y, int defalt = 9999)
        {
            if (!int.TryParse(y, out int rv))
                rv = defalt;
            if (rv == 0)
                rv = defalt;
            return rv;
        }

        private ContentReformatter _crfAboutBeforeAfter;
        private ContentReformatter _crfBetween;
        private ContentReformatter _crfDate;
        private Dictionary<string, string> MonthNames =new Dictionary<string, string>()
        {
            {"jan", "January"},
            {"feb", "February"},
            {"mar", "March"},
            {"apr", "April"},
            {"may", "May"},
            {"jun", "June"},
            {"jul", "July"},
            {"aug", "August"},
            {"sep", "September"},
            {"oct", "October"},
            {"nov", "November"},
            {"dec", "December"},
        };

        private static Regex LongMonthNamesRex = new Regex(@"(?i)(?<month>January|February|March|April|June|July|August|September|October|November|December)");

        public GenealogicalDateFormatter()
        {
            _crfAboutBeforeAfter = ReportContext.Instance.Settings.DateRules.Find(i => i.Role == PatternRole.DateAboutBeforeAfter);
            _crfBetween = ReportContext.Instance.Settings.DateRules.Find(i => i.Role == PatternRole.DateBetween);
            _crfDate = ReportContext.Instance.Settings.DateRules.Find(i => i.Role == PatternRole.Date);
        }

        public string Reformat(string d, bool returnUnrecognizableInput = false)
        {
            string dParm = (d ?? "").Trim();
            if (string.IsNullOrEmpty(dParm)) return "";

            // "somehow, sometimes" we get full-length month 
            // names in the input.  truncate them to make the 
            // rest of this processing word:
            Match lm = LongMonthNamesRex.Match(dParm);
            if (lm.Success)
            {
                d = dParm = dParm.Replace(lm.Value, lm.Value.Substring(0, 3));
            }

            Match m = _crfBetween?.Extractor?.Match(dParm);
            if (m?.Success ?? false)
            {
                return m.Result(_crfBetween.Emitter);
            }

            m = _crfAboutBeforeAfter?.Extractor?.Match(dParm);
            if (m?.Success ?? false)
            {
                string op = m.Result(_crfAboutBeforeAfter.Emitter);
                string dd = m.Groups["date"].Value;
                string ip = ReformatDate(dd, false);
                string prep = "about";
                if (!string.IsNullOrEmpty(m.Groups["after"].Value))
                    prep = "after";
                else if (!string.IsNullOrEmpty(m.Groups["before"].Value))
                    prep = "before";
                string rv = String.Format(op, prep, ip);
                return rv;
            }

            return ReformatDate(d, returnUnrecognizable: returnUnrecognizableInput);
        }

        private string ReformatDate(string d, bool applyPreposistion = true, bool returnUnrecognizable = false)
        {
            if (string.IsNullOrEmpty(d)) return "";
            Match m = _crfDate?.Extractor?.Match(d.ToUpper());
            if (!(m?.Success ?? false)) return returnUnrecognizable ? d : "";

            string prep = applyPreposistion ? (string.IsNullOrEmpty(m.Groups["day"].Value) ? "in " : "on ") : null;

            string op = m.Result(_crfDate.Emitter);
            string mon = m.Groups["month"].Value;
            if (!string.IsNullOrEmpty(mon))
            {
                if (MonthNames.TryGetValue(mon.ToLower(CultureInfo.InvariantCulture), out var fullmon))
                {
                    mon = fullmon;
                }
            }

            return $"{prep}{string.Format(op, mon).Trim()}".Replace("  ", " ");
        }
    }
}
