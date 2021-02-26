using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ged2Reg.Model
{
    public class GenealogicalPlaceFormatter
    {
        internal static readonly FormattedPlaceName EmptyPlace = new FormattedPlaceName(){LongName = ""};
        public bool InjectWordCounty { get; set; } = true;
        public bool DropUSA { get; set; } = true;
        public bool ReduceOnRepetition { get; set; } = true;
        //public string PlaceNamePattern { get; set; }

        public HashSet<string> Counties { get; set; }
        public HashSet<string> ForeignCounties { get; set; }
        public HashSet<string> States { get; set; }
        public Dictionary<string, string> StateAbbreviations { get; set; }
        public HashSet<string> Countries { get; set; }


        private StringEqualComparer _stringEqual = new StringEqualComparer();
        private char[] _splitter = new[] { ',' };
        private char[] _splitterAlt = new[] { '.' };


        public HashSet<string> NamesForUsa { get; set; } = new HashSet<string>()
        {
            "United States of America",
            "United States",
            "USA",
        };

        public HashSet<string> CountriesWithCounties { get; set; } = new HashSet<string>()
        {
            "USA",
            "United Kingdom",
            "England",
            "Ireland"
        };

        public string NameOfUsa { get; set; } = "USA";

        Dictionary<string, string> _previouslyUsedNames = new Dictionary<string, string>();

        public GenealogicalPlaceFormatter Init()
        {
            Countries = new HashSet<string>(NameConstants.CountryLevelNames);
            States = new HashSet<string>(NameConstants.StateLevelNames);
            StateAbbreviations = new Dictionary<string, string>(NameConstants.StateAbbreviations);
            Counties = new HashSet<string>(NameConstants.CountyLevelNames);
            ForeignCounties = new HashSet<string>(NameConstants.NonUsaCountyNames);

            return this;
        }

        /// <summary>
        /// Can be used generation-by-generation to allow the output of each fully
        /// qualified place name once per generation.
        /// </summary>
        public void Reset()
        {
            _previouslyUsedNames.Clear();
        }

        public FormattedPlaceName Reformat(string p)
        {
            FormattedPlaceName rv = ReformatInternal(p);
            if ((rv.LongName?.IndexOf(", ,") ?? 0) > 0)
                Debug.WriteLine($"Unexpected result in place name: '{rv.LongName}'");
            return rv;
        }

        internal FormattedPlaceName ReformatInternal(string p)
        {
            p = DeTrash(p);

            if (string.IsNullOrEmpty(p)) return EmptyPlace;

            FormattedPlaceName rv = new FormattedPlaceName(){UnformattedName = p};
            CanonicalPlace cp = new CanonicalPlace();

            bool isDC = DetectDC(p, cp);
            if (isDC)
            {
                cp.Emit(DropUSA, InjectWordCounty, NameOfUsa, rv);
                return rv;
            }

            string[] pps = p.Split(_splitter, StringSplitOptions.RemoveEmptyEntries);
            if (pps.Length==1 && p.Contains("."))
                pps = p.Split(_splitterAlt, StringSplitOptions.RemoveEmptyEntries);
            if (pps.Length < 2)
                return rv;

            for (int i = 0; i < pps.Length; i++)
            {
                pps[i] = pps[i].Trim();
                if (pps[i].Length == 0)
                    pps[i] = null;
            }

            pps = (from pp in pps where pp != null select pp).ToArray();

            bool isUsa = DetectDC(p, cp) || DetectAndProcessUsaPlace(pps, cp);
            if (!isUsa)
                ProcessNonUsaPlace(pps, cp);

            string[] rva = cp.Emit(DropUSA, InjectWordCounty, NameOfUsa, rv);
            if (!ReduceOnRepetition)
                return rv;
            if (_previouslyUsedNames.ContainsKey(rva[0])) 
            {
                rv.PreferShort = !cp.IsAmbiguous;
                return rv;
            }
            _previouslyUsedNames.Add(rva[0], rva[1]);
            return rv;
        }

        private string DeTrash(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            while (s.Contains(",,"))
                s = s.Replace(",,", ",");
            while (s.Length > 1 && s.StartsWith(','))
                s = s.Substring(1);
            return s;
        }

        Regex _rexDC = new Regex(@"(?i)(District of Columbia)|(Washington[,]?\s?D[.]?\s?C[.]?)");

        private bool DetectDC(string placeName, CanonicalPlace cp)
        {
            if (!_rexDC.IsMatch(placeName)) return false;
            cp.IsUsaPlace = true;
            cp.OutputOverride = "Washington, D.C.";
            return true;
        }

        private void ProcessNonUsaPlace(string[] pps, CanonicalPlace cp)
        {
            int ix = pps.Length - 1;
            bool hasCountry = Countries.Contains(pps[ix], _stringEqual);
            bool hasCounties = hasCountry && CountriesWithCounties.Contains(pps[ix], _stringEqual);
            if (hasCountry)
            {
                cp.Country = pps[ix];
                if (--ix < 0) return;
            }

            if (hasCounties && ForeignCounties.Contains(pps[ix], _stringEqual))
            {
                cp.County = pps[ix];
                if (--ix < 0) return;
            }

            for (int i = 0; i <= ix; i++)
                cp.Locality.Add(pps[i]);
        }

        private bool LooksLikeStateName(string stateMaybe)
        {
            if (States.Contains(stateMaybe, _stringEqual))
                return true;

            string checkAbbrev = (stateMaybe ?? "").Trim().Replace(".", "").ToUpper();
            if (checkAbbrev.Length == 2)
            {
                if (StateAbbreviations.ContainsKey(checkAbbrev))
                    return true;
            }

            return false;
        }

        private Regex _rexCounty = new Regex(@"(?i)(?<countyName>.*?)\s+((county)|(co[.]?))\s*$");

        private string IsCountyName(string maybe, bool mustBeExplicit)
        {
            if (string.IsNullOrEmpty(maybe)) return null;

            if (mustBeExplicit)
            {
                Match m = _rexCounty.Match(maybe);
                if (!m.Success)
                    return null;
                return m.Groups["countyName"].Value;
            }

            maybe = maybe.ToUpper().Replace("COUNTY", "").Trim();

            if (maybe.Length > 3 && maybe.EndsWith(" CO"))
                maybe = maybe.Substring(0, maybe.LastIndexOf(" CO"));

            char[] mchars = maybe.Trim().ToLower().ToCharArray();

            mchars[0] = char.ToUpper(mchars[0]);
            for (int i = 1; i < mchars.Length; i++)
            {
                if (mchars[i-1] == ' ') 
                    mchars[i] = char.ToUpper(mchars[i]);
            }

            maybe = new string(mchars);

            return Counties.Contains(maybe, _stringEqual)
                ? maybe
                : null;
        }

        private bool DetectAndProcessUsaPlace(string[] pps, CanonicalPlace cp)
        {
            int ix = pps.Length - 1;
            bool isUsa = NamesForUsa.Contains(pps[ix], _stringEqual);
            bool isImplicitUsa = !isUsa && LooksLikeStateName(pps[ix]);
            bool isCattedUsa = false;
            string implicated = null;
            if (!isUsa && !isImplicitUsa)
            {
                foreach (string usa in NamesForUsa)
                {
                    if (!pps[ix].EndsWith(usa, true, System.Globalization.CultureInfo.CurrentCulture))
                        continue;
                    implicated = usa;
                    isCattedUsa = true;
                    break;
                }
            }
            if (!(isUsa || isImplicitUsa || isCattedUsa))
                return false;

            cp.IsUsaPlace = true;

            if (isCattedUsa)
                pps[ix] = pps[ix].Replace(implicated, "").Trim();

            if (!DropUSA)
                cp.Country = NameOfUsa;
            if (isUsa)
                ix--; // explicit, so, consume it

            if (ix < 0) return true;

            string checkAbbrev = (pps[ix] ?? "").Trim().Replace(".", "").ToUpper();
            if (checkAbbrev.Length == 2)
            {
                if (StateAbbreviations.ContainsKey(checkAbbrev))
                    pps[ix] = StateAbbreviations[checkAbbrev];
            }

            if (States.Contains(pps[ix], _stringEqual))
            {
                cp.State = pps[ix];
                if (--ix < 0) return true;
            }

            string maybeCounty = IsCountyName(pps[ix], ix==0);
            if (!string.IsNullOrEmpty(maybeCounty))
            {
                cp.County = maybeCounty;
                if (--ix < 0) return true;
            }
            else
            {
                cp.IsAmbiguous = ix == 0;
            }

            cp.City = pps[ix];

            for (int i = 0; i < ix; i++)
                cp.Locality.Add(pps[i]);

            return true;
        }
    }

    public class FormattedPlaceName
    {
        public string UnformattedName { get; set; }
        public string LongName { get; set; }
        public string ShortName { get; set; }
        public bool PreferShort { get; set; }
        public string IndexEntry { get; set; }
        public string PreferredName => PreferShort ? ShortName : LongName ?? UnformattedName;
    }

    public class StringEqualComparer : IEqualityComparer<string>
    {
        #region Implementation of IEqualityComparer<in string>

        public bool Equals(string x, string y)
        {
            if (x == null) return y==null;
            return x.Equals(y, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }

        #endregion
    }
}
