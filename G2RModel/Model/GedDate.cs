using System.Text.RegularExpressions;

namespace Ged2Reg.Model
{
    public static class GedDate
    {
        private static readonly Regex _rexYear = new Regex("(?<year>\\d{4})");
        private static readonly Regex _rexYear6 = new Regex("(?<year>\\d{4}([/]\\d)?)");
        private static readonly Regex _rexYearOnly = new Regex(@"\A(?<year>\d{1,4})(?:\s*(AD|BC|BCE|CE))?\z");
        private static readonly Regex _rexYearEra = new Regex(@"(?<year>\d{1,4})(?:\s*(AD|BC|BCE|CE))");
        private static readonly Regex _rexMonYearEra = new Regex(@"(?:[A-Z]+)\s+(?<year>\d{1,4})(?:\s*(AD|BC|BCE|CE))?");
        public static string ExtractYear(string dat, bool allow6 = false, string defaultVal = "")
        {
            string rv = defaultVal;
            if (string.IsNullOrEmpty(dat))
                return rv;

            Match m = (allow6 ? _rexYear6 : _rexYear).Match(dat);
            if (!m.Success)
                m = _rexYearOnly.Match(dat);
            if (!m.Success)
                m = _rexYearEra.Match(dat);
            if (!m.Success)
                m = _rexMonYearEra.Match(dat);

            if (m.Success)
                rv = m.Groups["year"].Value;

            return rv;
        }
    }
}
