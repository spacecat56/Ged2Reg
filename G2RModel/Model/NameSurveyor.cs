using System.Collections.Generic;
using Ged2Reg.Model;

namespace G2RModel.Model
{
    public static class NameSurveyor
    {
        private static HashSet<string> _knownNames;

        public static void Survey(IEnumerable<GedcomIndividual> individuals, bool reset = true)
        {
            if (reset || _knownNames == null) _knownNames = new HashSet<string>();

            foreach (GedcomIndividual individual in individuals)
            {
                if (!_knownNames.Contains(individual.Surname))
                    _knownNames.Add(individual.Surname);
                string fno = FirstNameOnly(individual.GivenName);
                if (fno != null && !_knownNames.Contains(fno))
                    _knownNames.Add(fno);
            }
        }

        private static string FirstNameOnly(string s)
        {
            s = s?.Trim();
            if (string.IsNullOrEmpty(s)) return s;
            int ix = s.IndexOf(' ');
            return ix < 0 ? s : s.Substring(0, ix);
        }

        public static bool IsKnown(string nameQ)
        {
            return _knownNames.Contains(nameQ);
        }
    }
}
