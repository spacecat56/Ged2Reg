using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ged2Reg.Model
{
    public static class GedDate
    {
        private static readonly Regex _rexYear = new Regex("\\d{4}");
        private static readonly Regex _rexYear6 = new Regex("\\d{4}([/]\\d)?");
        public static string ExtractYear(string dat, bool allow6 = false, string defaultVal = "")
        {
            string rv = defaultVal;
            if (string.IsNullOrEmpty(dat))
                return rv;

            Match m = (allow6 ? _rexYear6 : _rexYear).Match(dat);
            if (m.Success)
                rv = m.Value;

            return rv;
        }
    }
}
