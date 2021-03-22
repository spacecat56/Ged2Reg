﻿using System;
using System.Text;
using System.Text.RegularExpressions;
using G2RModel.Model;
using SimpleGedcomLib;

namespace Ged2Reg.Model
{
    public class FormattedEvent
    {
        public static bool IncludeFactDescription { get; set; } = true;
        public static bool EditFactDescription { get; set; }
        public static bool PlaceBeforeDate { get; set; }
        public static TextFixer DescriptionFixer { get; set; }
        /// <summary>
        /// Regex picks out the first word in a parenthetical clause, iff
        /// it starts with one uppercase letter, continues with 0 or more lower
        /// case letters, and is NOT followed by another word starting with an
        /// upper case letter 
        /// </summary>
        public static Regex Word1Regex { get;  } = new Regex(@"(?-i)^[(](?<word1>[A-Z][a-z]*)[ ,][^A-Z]");

        public string EventString { get; set; }
        public string PlaceIndexEntry { get; set; }
        public int PlaceIndexIndex { get; set; }
        public TagCode EventTagCode { get; set; }

        public ReportEntryBase Owner { get; set; }

        private static char[] _splitSpace = { ' ' };

        public static FormattedEvent ConditionalEvent(string ev, string dayt, string place, string detail = null, bool omitDate = false)
        {
            return new FormattedEvent().Init(ev, dayt, place, detail, omitDate);
        }

        public FormattedEvent Init(string ev, string dayt, string place, string detail = null, bool omitDate = false)
        {
            void ApplyNonEmptyPlace(StringBuilder sb1)
            {
                if (!string.IsNullOrEmpty(place))
                {
                    FormattedPlaceName fpn = GenealogicalPlaceFormatter.Instance.Reformat(place);

                    sb1.Append($" {fpn.Preposition} ").Append(fpn.PreferredName);
                    PlaceIndexIndex = sb1.Length;
                    PlaceIndexEntry = fpn.IndexEntry;
                }
            }

            void ApplyNonEmptyDate(StringBuilder stringBuilder)
            {
                if (omitDate || string.IsNullOrEmpty(dayt?.Trim()))
                    return;
                // here, anything that is SUPPOSED TO BE a date will be 
                // output, EVEN IF we can't parse and reformat it
                string d = GenealogicalDateFormatter.Instance.Reformat(dayt);
                if (string.IsNullOrEmpty(d)) 
                    d = dayt.Trim();
                stringBuilder.Append(' ').Append(d);
            }

            if (string.IsNullOrEmpty(dayt) && string.IsNullOrEmpty(place))
                return null;

            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(ev))
                sb.Append(" ").Append(ev);

            if (PlaceBeforeDate)
            {
                ApplyNonEmptyPlace(sb);
                ApplyNonEmptyDate(sb);
            }
            else
            {
                ApplyNonEmptyDate(sb);
                ApplyNonEmptyPlace(sb);
            }

            if (IncludeFactDescription && !string.IsNullOrEmpty(detail))
            {
                string det = OptimizeEventDetail(detail);
                if (DescriptionFixer != null)
                    det = DescriptionFixer.Exec(det);
                // we ignore anything of length 1 or shorter...
                // allowing a "Y" to be entered in order to be sure
                // the event is on record even if nothing is known
                // this can be needed in the struggle between e.g. 
                // FTM and Ancestry.com over whether a person is
                // "living" or not (ancestry is pretty dimmwitted about it)
                if ((det??string.Empty).Length > 1)
                    sb.Append(" ").Append(det);
            }

            string rv = sb.ToString().Trim();
            if (rv.Equals(ev?.Trim() ?? "")) return null;
            EventString = sb.ToString();
            return this;
        }

        internal static string OptimizeEventDetail(string detail)
        {
            detail = detail?.Trim();
            if (string.IsNullOrEmpty(detail) || detail.Length < 2)
                return null;

            bool wrappedAlready = detail.StartsWith('(') && detail.EndsWith(')');

            // don't end with a period
            if (!wrappedAlready && detail.EndsWith("."))
                detail = detail.Substring(0, detail.Length - 1);
            else if (detail.EndsWith(".)"))
                detail = $"{detail.Substring(0, detail.Length - 2)})";

            if (!EditFactDescription)
            {
                detail = wrappedAlready ? detail : $"({detail})";

                // even "unedited: we REALLY want to get rid of intrusive leading uppercase letters
                Match m = Word1Regex.Match(detail);
                if (m.Success)
                {
                    if (!NameSurveyor.IsKnown(m.Groups["word1"].Value))
                    {
                        // relying on the regex being pinned to the START of the string
                        // and being wrapped in ()s simplifies this a lot
                        detail = $"({char.ToLower(detail[1])}{detail.Substring(2)}";
                    }
                }

                return detail;
            }

            string[] ss = detail.Split(_splitSpace, StringSplitOptions.RemoveEmptyEntries);

            // don't start with a capital letter
            // unless it is apparently part of a name (next word also caps)
            // or an abbreviated name (one letter)
            if (EvalForInitialLowercase(ss[0], ss.Length > 1 ? ss[1] : null))
                ss[0] = ss[0].Substring(0, 1).ToLower() + ss[0].Substring(1);


            // don't end sentences in the body of this text, make them ; clauses
            // single letter with a . do not change.  The word after any . -> ; change
            // is also a candidate to lowercase
            for (int i = 1; i < ss.Length; i++)
            {
                if (!ss[i].EndsWith(".")) continue;
                if (ss[i].Length < 3) continue;
                // trying to not be fooled by abbreviations like "St."
                if (LanguageElementConstants.Abbreviations.Contains(ss[i])) continue;
                //if (LanguageElementConstants.Abbreviations.Contains(ss[i].ToLower())) continue;
                //if (LanguageElementConstants.Abbreviations.Contains(ss[i].ToUpper())) continue;
                // try to recognize initials like L.I.  or W.H.
                if (ss[i].IndexOf('.') < ss[i].LastIndexOf('.')) continue;
                // remove the '.';
                ss[i] = ss[i].Substring(0, ss[i].Length - 1) + ";";
                if (i + 2 >= ss.Length) continue; // need two more words to evaluate for lc
                if (!EvalForInitialLowercase(ss[i + 1], ss[i + 2])) continue;
                ss[i + 1] = ss[i + 1].Substring(0, 1).ToLower() + ss[i + 1].Substring(1);
            }

            // reassemble, wrapped in ()s
            StringBuilder sb = new StringBuilder(detail.Length + 2);
            if (!wrappedAlready)
                sb.Append('(');
            for (int i = 0; i < ss.Length - 1; i++)
            {
                sb.Append(ss[i]).Append(' ');
            }
            sb.Append(ss[ss.Length - 1]);

            if (!wrappedAlready)
                sb.Append(')');

            return sb.ToString();
        }

        public static bool EvalForInitialLowercase(string x1, string x2)
        {
            if (NameConstants.CommonGivenNames.Contains(x1.ToUpper())) return false;
            if (NameConstants.CommonSurnames.Contains(x1.ToUpper())) return false;

            if (LanguageElementConstants.CommonPrepositions.Contains(x1.ToLower())) return true;
            if (LanguageElementConstants.Determiners.Contains(x1.ToLower())) return true;
            if (LanguageElementConstants.Pronouns.Contains(x1.ToLower())) return true;

            bool initialLowercase = x1.Length > 1 && char.IsUpper(x1.ToCharArray()[0]);
            initialLowercase = initialLowercase && (x1.Length > 2 || (x1.Length == 2 && x1.ToCharArray()[1] != '.'));
            initialLowercase = initialLowercase && (x2 == null || !char.IsUpper(x2.ToCharArray()[0]));
            return initialLowercase;
        }
    }
}
