// CitationFormatter.cs
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommonClassesLib;
using G2RModel;
using SimpleGedcomLib;

namespace Ged2Reg.Model
{
    public class CitationResult
    {
        public static bool DetectHyperlinksInTextPieces { get; set; } = true;


        public static Regex UrlRex = new Regex(@"(?i)\b(?<root>https?://.*?[.][a-z]+)/\S+\b");


        public string Text => Pieces.Count == 1 && Pieces[0].PieceType == PieceType.Text
            ? Pieces[0].Text
            : null;

        public List<CitationResultPiece> Pieces { get; set; } = new List<CitationResultPiece>();

        public string SourceId { get; set; }
        public bool IsSimpleDetail { get; set; }
        public string AltText { get; set; }

        public void AppendText(string t)
        {
            if (!DetectHyperlinksInTextPieces || string.IsNullOrEmpty(t))
            {
                AppendTextPiece(t);
                return;
            }
            MatchCollection matches = UrlRex.Matches(t);
            if (matches.Count == 0)
            {
                AppendTextPiece(t);
                return;
            }

            int nextCharToEmit = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                Match m = matches[i];
                if (nextCharToEmit < m.Index)
                    AppendTextPiece(t.Substring(nextCharToEmit, m.Index - nextCharToEmit));
                AppendLink(m.Value);
                nextCharToEmit = m.Index + m.Length;
            }
            if (nextCharToEmit < t.Length)
                AppendTextPiece(t.Substring(nextCharToEmit));
        }

        public void AppendLink(string t)
        {
            Pieces.Add(new CitationResultPiece() { PieceType = PieceType.Hyperlink, Text = t });
        }

        private void AppendTextPiece(string t)
        {
            Pieces.Add(new CitationResultPiece() {PieceType = PieceType.Text, Text = t});
        }
    }

    public enum PieceType
    {
        Text,
        Hyperlink
    }
    public class CitationResultPiece
    {
        public PieceType PieceType { get; internal set; }
        public string Text { get; set; }
        public string SourceId { get; set; }    

    }


    public class CitationFormatter
    {
        public static bool PreferEditedCitation { get; set; }

        /// <summary>
        /// FTM has a quirky way of emitting a simple citation, prefxing it with
        /// "Details: ", which is pretty ugly in the output.  we will strip that
        /// off when we see it... but leave the option public so it could be suppressed
        /// </summary>
        public static bool StripLeadingWordDetails { get; set; } = true;
        private const string WordDetails = "Details: ";


        public TextCleanerContext OperationContext { get; set; } = TextCleanerContext.FullCitation;
        public List<CitationPart> Parts { get; set; }

        public CitationResult Apply(CitationView cv)
        {
            CitationResult rv = new CitationResult();
            SourceView sv = cv?.TheSourceView;

            if (PreferEditedCitation)
            {
                string edText = cv?.SourceTag?.GetEditedCitationText();
                if (!string.IsNullOrEmpty(edText))
                {
                    rv.AppendText(edText);
                    rv.AltText = TextCleaner.TitleCleaner.Exec(sv.Title, TextCleanerContext.SeeNote) ?? "";
                    return rv;
                }
            }

            if (sv == null)
            {
                // citation with no source reference is just a piece of text
                rv.IsSimpleDetail = true;
                string simpleText = cv?.SourceTag?.Content;
                if (!string.IsNullOrEmpty(simpleText))
                {
                    if (StripLeadingWordDetails && simpleText.StartsWith(WordDetails) 
                    && simpleText.Length > WordDetails.Length)
                        simpleText = simpleText.Substring(WordDetails.Length);
                    string ext = simpleText.EndsWith(".") ? string.Empty : ".";
                    rv.AppendText($"{simpleText}{ext}");
                }
                return rv;
            }


            rv.SourceId = sv.Id;

            StringBuilder sb = new StringBuilder();
            string pendingAnotherPiece = null;
            foreach (CitationPart part in Parts)
            {
                switch (part.Name)
                {
                    case CitationPartName.None:
                        break;
                    case CitationPartName.Source_AUTH:
                        pendingAnotherPiece = part.Apply(sb, sv.Author, pendingAnotherPiece);
                        break;
                    case CitationPartName.Source_TITL:
                        pendingAnotherPiece = part.Apply(sb, TextCleaner.TitleCleaner.Exec(sv.Title, OperationContext), pendingAnotherPiece);
                        break;
                    case CitationPartName.Source_PUBL:
                        pendingAnotherPiece = part.Apply(sb, sv.Publisher, pendingAnotherPiece);
                        break;
                    case CitationPartName.Source_REPO:
                        pendingAnotherPiece = part.Apply(sb, sv.Repository, pendingAnotherPiece);
                        break;
                    case CitationPartName.Source_NOTE:
                        pendingAnotherPiece = part.Apply(sb, sv.ReferenceNote, pendingAnotherPiece);
                        break;
                    case CitationPartName.Source_TEXT:
                        pendingAnotherPiece = part.Apply(sb, sv.SourceTag.GetChild(TagCode.TEXT)?.FullText(), pendingAnotherPiece);
                        break;
                    case CitationPartName.Source_ABBR:
                        pendingAnotherPiece = part.Apply(sb, sv.SourceTag.GetChild(TagCode.ABBR)?.Content, pendingAnotherPiece);
                        break;
                    case CitationPartName.Citation_PAGE:
                        pendingAnotherPiece = part.Apply(sb, cv.Detail, pendingAnotherPiece);
                        break;
                    case CitationPartName.Citation_DATA_TEXT:
                        pendingAnotherPiece = part.Apply(sb, cv.Text, pendingAnotherPiece);
                        break;
                    case CitationPartName.Citation_DATA_DATE:
                        pendingAnotherPiece = part.Apply(sb, cv.SourceTag.GetChild(TagCode.DATA)?.GetChild(TagCode.DATE)?.Content, pendingAnotherPiece);
                        break;
                    case CitationPartName.Citation_URL:
                        string url = cv.URL?.Trim();
                        if (string.IsNullOrEmpty(url))
                            break;
                        sb.Append(pendingAnotherPiece);
                        pendingAnotherPiece = null;
                        if (!string.IsNullOrEmpty(part.Prefix))
                        {
                            sb.Append(part.Prefix);
                        } 
                        if (sb.Length > 0)
                        {
                            rv.AppendText(sb.ToString());
                            sb.Clear();
                        }
                        rv.AppendLink(url);
                        if (!string.IsNullOrEmpty(part.Suffix))
                        {
                            sb.Append(part.Suffix);
                        }
                        break;
                    case CitationPartName.LiteralOnly:
                        sb.Append(string.Format(part.FormatString, ""));
                        pendingAnotherPiece = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            sb.Append(pendingAnotherPiece); // ugh.  'must be' a '.' supplied by a nullvalue at the end, otherwise, not working
            if (sb.Length > 0)
                rv.AppendText(sb.ToString());

            return rv;
        }

        //public CitationFormatter AutoSequence()
        //{
        //    for (int i = 0; i < Parts.Count; i++)
        //    {
        //        Parts[i].Sequence = i + 1;
        //    }

        //    return this;
        //}
    }

    public class CitationPart
    {
        private const string Splitter = "@^@";
        public int Sequence { get; set; }
        public CitationPartName Name { get; set; } = CitationPartName.None;
        public string FormatString { get; set; }
        public string NullValue { get; set; }
        public bool IsActive { get; set; } = true;
        public string TrailingSeparator { get; set; }

        public bool TrimTrailingPunctuation { get; set; } = true;
        public string ChainableFormatString { get; set; }
        public bool ChainEvaluated { get; set; }

        private string _prefix;
        private string _suffix;
        internal string Prefix => _prefix ?? SplitFormat();
        internal string Suffix => _suffix; // note dependency on call sequence

        private string SplitFormat()
        {
            string breakingUpIsHardToDo = string.Format(FormatString, Splitter);
            if (Splitter == breakingUpIsHardToDo)
            {
                return _prefix = _suffix = "";
            }

            int ix = breakingUpIsHardToDo.IndexOf(Splitter, StringComparison.CurrentCulture);
            _prefix = ix == 0 ? "" : breakingUpIsHardToDo.Substring(0, ix);
            int ix2 = ix + Splitter.Length;
            _suffix = ix2 >= breakingUpIsHardToDo.Length ? "" : breakingUpIsHardToDo.Substring(ix2);

            //if ((_suffix ?? "").Contains("."))
            //    ChainableFormatString = FormatString; // assume it is a sentence-ending.  WEAK!
            //else
                ChainableFormatString = FormatString.Substring(0, FormatString.LastIndexOf(_suffix, StringComparison.Ordinal));

            ChainEvaluated = true;

            return _prefix;
        }

        public string Apply (string val)
        {
            if (!IsActive) return null;
            if (string.IsNullOrEmpty(val)) return NullValue;
            return string.Format(FormatString ?? "{0}", Clean(val));
        }

        //public string Apply(StringBuilder sb, string val, string pending)
        //{
        //    if (!IsActive) return null;
        //    if (string.IsNullOrEmpty(val = Clean(val))) return NullValue;
        //    if (!ChainEvaluated)
        //        SplitFormat();
        //    sb.Append(pending).Append(
        //        string.Format(ChainableFormatString ?? FormatString ?? "{0}", val));
        //    return Suffix;
        //}

        public string Apply(StringBuilder sb, string val, string pending)
        {
            if (!IsActive) return pending;
            if (string.IsNullOrEmpty(val = Clean(val))) return pending;
            //if (!ChainEvaluated)
            //    SplitFormat();
            sb.Append(pending).Append(
                string.Format(ChainableFormatString ?? FormatString ?? "{0}", val));
            return TrailingSeparator;
        }

        private static char[] PunctChars = {'.', ',', ':', ';'};
        private string Clean(string s)
        {

            s = s?.Trim();
            if (string.IsNullOrEmpty(s) || s.Length < 2 || !TrimTrailingPunctuation) 
                return s;
            if (!PunctChars.Contains(s[s.Length - 1])) 
                return s;
            return s.Substring(0, s.Length - 1).Trim();
        }
    }

    public class ListOfCitationParts : SortableBindingList<CitationPart>
    {
        public ListOfCitationParts AutoSequence()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Sequence = i + 1;
            }

            return this;
        }
    }
}
