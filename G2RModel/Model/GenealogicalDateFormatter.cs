using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ged2Reg.Model
{
    public class GenealogicalDateFormatter
    {
        public static int ParseYear(string y, int defalt = 9999)
        {
            if (!int.TryParse(y, out int rv))
                rv = defalt;
            if (rv == 0)
                rv = defalt;
            return rv;
        }

        //private ContentReformatter _crfBefore;
        //private ContentReformatter _crfAfter;
        //private ContentReformatter _crfAbout;
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

        public GenealogicalDateFormatter()
        {
            //_crfBefore = ReportContext.Instance.Settings.DateRules.Find(i => i.Role == PatternRole.DateBefore);
            //_crfAfter = ReportContext.Instance.Settings.DateRules.Find(i => i.Role == PatternRole.DateAfter);
            //_crfAbout = ReportContext.Instance.Settings.DateRules.Find(i => i.Role == PatternRole.DateAbout);
            _crfAboutBeforeAfter = ReportContext.Instance.Settings.DateRules.Find(i => i.Role == PatternRole.DateAboutBeforeAfter);
            _crfBetween = ReportContext.Instance.Settings.DateRules.Find(i => i.Role == PatternRole.DateBetween);
            _crfDate = ReportContext.Instance.Settings.DateRules.Find(i => i.Role == PatternRole.Date);
        }

        public string Reformat(string d)
        {
            if (string.IsNullOrEmpty(d)) return "";

            Match m = _crfBetween?.Extractor?.Match(d);
            if (m?.Success ?? false)
            {
                return m.Result(_crfBetween.Emitter);
            }

            m = _crfAboutBeforeAfter?.Extractor?.Match(d);
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

            return ReformatDate(d);

        }

        private string ReformatDate(string d, bool applyPreposistion = true)
        {
            if (string.IsNullOrEmpty(d)) return "";
            Match m = _crfDate?.Extractor?.Match(d);
            if (!(m?.Success ?? false)) return "";

            string prep = applyPreposistion ? (string.IsNullOrEmpty(m.Groups["day"]?.Value) ? "in " : "on ") : null;

            string op = m.Result(_crfDate.Emitter);
            string mon = m.Groups["month"].Value;
            if (!string.IsNullOrEmpty(mon))
            {
                if (MonthNames.TryGetValue(mon.ToLower(CultureInfo.InvariantCulture), out var fullmon))
                {
                    mon = fullmon;
                }
            }

            return $"{prep}{string.Format(op, mon).Trim()}";
        }
    }
}
