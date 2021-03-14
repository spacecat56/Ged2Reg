using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Ged2Reg.Model;
using SimpleGedcomLib;

namespace GedcomObfuscationLib
{
    public class GedcomObfuscator
    {
        public static int OutputLineLength { get; set; } = 80;
        public string FileName { get; set; }
        public Exception LastException { get; set; }
        public string FileNameOut { get; set; }
        public int Fence { get; set; } = 1750;
        public int NamesToRecheck { get; set; } = 10;

        public StringBuilder ResultsText { get;  } = new StringBuilder();
        public int CountIndividuals { get; set; }
        public int CountImmune { get; set; }
        public int CountNoId { get; set; }
        public int CountSurnMapped { get; set; }
        public int CountGivenMapped { get; set; }
        public int CountSurnUnMapped { get; set; }
        public int CountGivenUnMapped { get; set; }
        public int CountDropObje { get; set; }
        public int CountDropRepo { get; set; }
        public int CountDropUnk { get; set; }

        public List<NamesPools.NameCounter> Residuals { get; set; }

        private GedcomFile _file;
        private NamesPools _namePool;
        private ContentCleaner _cleaner;
        private string _newText;
        private HashSet<string> _immune;
        private RegisterReportModel _rrm;
        private Dictionary<string, GedcomIndividual> _gedcomIndividuals;

        private HashSet<string> _visitedIndis;
        private string _fileName;
        private long _fileSize;

        private Random _random;

        public Random Randomizer => _random ??= new Random(DateTime.Now.Millisecond + (int)(DateTime.Now.Ticks % 100019));

        /// <summary>
        /// Read the GEDCOM file and assign create the maps
        /// to be used in changing names.
        /// </summary>
        /// <returns>true on success; LastException set otherwise</returns>
        public bool Init(RegisterReportModel rrm)
        {
            try
            {
                _rrm = rrm;
                _fileName = rrm.Settings.GedcomFile;
                _fileSize = new FileInfo(_fileName).Length;
                _file = new GedcomFile().Parse(rrm.Settings.GedcomFile);
                _namePool = new NamesPools();

                _rrm.ResetGedcom();

                _gedcomIndividuals = new Dictionary<string, GedcomIndividual>();
                foreach (GedcomIndividual gi in _rrm.Individuals)
                {
                    gi.FindFamilies(true);
                    IndividualView individualView = gi.IndividualView;
                    _gedcomIndividuals.Add(individualView.Id, gi);
                    _namePool.AssignSurname(individualView.Surname);
                    string sex = individualView.IndiTag.GetFirstDescendant(TagCode.SEX)?.Content;
                    if (sex == "M")
                    {
                        _namePool.AssignMame(individualView.GivenName);
                    }
                    else
                    {
                        _namePool.AssignFname(individualView.GivenName);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LastException = ex;
                return false;
            }
        }

        public bool Exec()
        {
            _visitedIndis = new HashSet<string>(_file.IndividualViews.Count * 4);
            _immune = new HashSet<string>(_file.IndividualViews.Count * 4);

            try
            {
                // walk back up all persons in the tree and once we reach the horizon flag 
                // as "immune" to stop obfuscation.
                foreach (GedcomIndividual indi in _rrm.Individuals)
                {
                    Ascend(indi, false);
                }

                // walk through all tags replacing non-immune names, turning text into 
                // random characters, ... 
                Tag zeroLevel = new Tag("", null) {Code = TagCode.UNK};
                foreach (Tag tag in _file.AllTags)
                {
                    switch (tag.Level)
                    {
                        case 0:
                            zeroLevel = tag;
                            if (zeroLevel.Code == TagCode.INDI)
                                CountIndividuals++;
                            break;
                        case 1:
                            if (zeroLevel.Code == TagCode.INDI)
                            {
                                //CountIndividuals++;
                                switch (tag.Code)
                                {
                                    case TagCode.NAME:
                                        if (_immune.Contains(zeroLevel.Id))
                                        {
                                            CountImmune++;
                                            break;
                                        }

                                        if (!_gedcomIndividuals.TryGetValue(zeroLevel.Id, out GedcomIndividual gedcomIndividual))
                                        {
                                            CountNoId++;
                                            break;
                                        }
                                        string surn = _namePool.MappedSurname(gedcomIndividual.IndividualView.Surname) ?? "";
                                        if (surn.Equals(gedcomIndividual.IndividualView.Surname, StringComparison.InvariantCultureIgnoreCase))
                                            CountSurnUnMapped++;
                                        else
                                            CountSurnMapped++;
                                        string gnam = gedcomIndividual.Gender == "M"
                                            ? _namePool.MappedMname(gedcomIndividual.IndividualView.GivenName)
                                            : _namePool.MappedFname(gedcomIndividual.IndividualView.GivenName);
                                        gnam ??= "";
                                        if (gnam.Equals(gedcomIndividual.IndividualView.GivenName, StringComparison.InvariantCultureIgnoreCase))
                                            CountGivenUnMapped++;
                                        else
                                            CountGivenMapped++;
                                        tag.Content = $"{InitCaps(gnam)} /{InitCaps(surn)}/";
                                        break;
                                    default:
                                        // todo: text in various places
                                        break;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

                // write the file back to a memory buffer, then 
                // truncate all URLS, ...
                StringBuilder sb = new StringBuilder((int) _fileSize);
                _cleaner = new ContentCleaner();
                int waitOnLevel = -1;
                int i;
                int iterLimit = _file.AllTags.Count;
                for (i = 0; i < iterLimit; i++)
                {
                    void SkipContinuations()
                    {
                        while (i + 1 < iterLimit && (_file.AllTags[i+1].Code == TagCode.CONC ||
                                                     _file.AllTags[i+1].Code == TagCode.CONT))
                        {
                            i++;
                        }
                    }

                    Tag tag = _file.AllTags[i];

                    Tag nextTag = i+1 >= _file.AllTags.Count ? null : _file.AllTags[i + 1];
                    bool continued = nextTag != null && (nextTag.Code == TagCode.CONC || nextTag.Code == TagCode.CONT);

                    if (waitOnLevel >= 0)
                    {
                        if (tag.Level > waitOnLevel)
                            continue;
                        waitOnLevel = -1;
                    }

                    switch (tag.Code)
                    {
                        case TagCode.PLAC:
                            sb.AppendLine(tag.ReAssemble());
                            break;
                        case TagCode.UNK:
                            CountDropUnk++;
                            waitOnLevel = tag.Level;
                            break;
                        case TagCode.OBJE:
                            CountDropObje++;
                            waitOnLevel = tag.Level;
                            break;
                        case TagCode.REPO:
                            CountDropRepo++;
                            waitOnLevel = tag.Level;
                            break;
                        case TagCode.CONT:
                        case TagCode.CONC:
                            break;
                        case TagCode.DATA:
                            if (string.IsNullOrEmpty(tag.Content))
                            {
                                sb.AppendLine(tag.ReAssemble());
                                break;
                            }
                            if (Scrub(tag, sb, continued))
                                waitOnLevel = tag.Level;
                            break;
                        case TagCode.TEXT:
                            //sb.AppendLine(tag.ReAssemble());
                            if  (Scrub(tag, sb, continued) && continued)
                                //waitOnLevel = tag.Level;
                                SkipContinuations();
                            break;
                        case TagCode.NOTE:
                            // may be a reference:
                            if (!tag.HasIdentifier)
                            {
                                sb.AppendLine(tag.ReAssemble());
                                break;
                            }
                            // or, the note body, which
                            // requires special handling, FTM (at least) emits them 
                            // empty of content but filled with next-level CONT/C 
                            ScrubNote(tag, sb);
                            waitOnLevel = tag.Level;
                            break;
                        case TagCode.DATE:
                            tag.Content = RandomizeDate(tag.Content);
                            sb.AppendLine(tag.ReAssemble());
                            break;
                        case TagCode.PAGE:
                            // overwrite the text, set the level to drop any continues,
                            // but fall through and keep the tag
                            tag.Content = "Page info [removed]";
                            waitOnLevel = tag.Level;
                            sb.AppendLine(tag.ReAssemble());
                            break;
                        default:
                            if (tag.Level == 0
                                || (tag.Level == 1 && tag.Code == TagCode.NAME)
                                || tag.Id != null)
                            {
                                sb.AppendLine(tag.ReAssemble());
                            }
                            else
                            {
                                bool eaten = Scrub(tag, sb, continued) && continued;
                                // scrub will eat all CONT / CONC
                                // but this: is NOT the way to skip them!
                                if (eaten)
                                {
                                    //waitOnLevel = tag.Level;
                                    SkipContinuations();
                                }
                            }
                            break;
                    }

                }

                _newText = _cleaner.PruneURLs(sb.ToString());

                ResultsText.AppendLine("*** Name Mapping ***");
                ResultsText.AppendLine($"    Individuals...............{CountIndividuals}");
                ResultsText.AppendLine($"    Immune....................{CountImmune}");
                //ResultsText.AppendLine($"    No Id.....................{CountNoId}");
                ResultsText.AppendLine($"    Surname unmapped..........{CountSurnUnMapped}");
                ResultsText.AppendLine($"    Surname mapped............{CountSurnMapped}");
                ResultsText.AppendLine($"    Given unmapped............{CountGivenUnMapped}");
                ResultsText.AppendLine($"    Given mapped..............{CountGivenMapped}");
                ResultsText.AppendLine("*** Other processing ***");
                ResultsText.AppendLine($"    URL truncations...........{_cleaner?.TextsChanged}");
                ResultsText.AppendLine($"    Dropped REPO..............{CountDropRepo}");
                ResultsText.AppendLine($"    Dropped OBJE..............{CountDropObje}");
                ResultsText.AppendLine($"    Dropped UNK...............{CountDropUnk}");

                Residuals = _namePool.CheckForResiduals(_newText, NamesToRecheck);

                File.WriteAllText(FileNameOut, _newText);

                ResultsText.AppendLine().Append("File written: ").Append(FileNameOut);

                List<NamesPools.NameCounter> repairs = Residuals.Where(
                    nc => nc.ResidualCount > 0 && nc.ResidualCount < 100 
                    && nc.OriginalName.Length > 4)
                    .ToList();

                bool didRepair = false;
                foreach (NamesPools.NameCounter repair in repairs)
                {
                    _newText = Regex.Replace(_newText, repair.OriginalName, InitCaps(repair.Name));
                    didRepair = repair.Repaired = true;
                }

                if (!didRepair)
                    return true;

                string altFile = Path.ChangeExtension(FileNameOut, $".alt{Path.GetExtension(FileNameOut)}");
                File.WriteAllText(altFile, _newText);
                ResultsText.AppendLine($"'Leftover' occurrences of top-{NamesToRecheck} surnames:");
                foreach (NamesPools.NameCounter r in Residuals)
                {
                    ResultsText.AppendLine(
                        $"Name in: {r.OriginalName}; Individuals: {r.Count}; Name out: {r.Name}; Leftovers: {r.ResidualCount}; Fixed: {r.Repaired}");
                }

                ResultsText.AppendLine().Append("Alt file written (residuals changed): ").Append(altFile);

                return true;
            }
            catch (Exception ex)
            {
                LastException = ex;
                return false;
            }
        }

        private void ScrubNote(Tag tag, StringBuilder sb)
        {
            //bool emptyBase = string.IsNullOrEmpty(tag.Content);
            string text = tag.FullText(true);
            text = _namePool.Scrub2(text) ?? text;
            
            tag.Content = null;
            sb.AppendLine(tag.ReAssemble());

            if (string.IsNullOrEmpty(text))
                return;

            text = RandomizeAnyDate(text);

            string[] lines = text.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None);
            string lTag = "CONC";  // first one does not begin with a nl
            foreach (string line in lines)
            {
                string lineOut = $"{tag.Level+1} {lTag} {line}";
                lTag = "CONT";  // any 'line' after the first needs a nl
                EmitContinuableText(sb, lineOut, tag.Level + 1);
            }
        }

        private string RandomizeDate(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            if (!DateTime.TryParse(s, out DateTime d))
                return s;

            int bump = Randomizer.Next(1, 60) - 30;
            d = d.AddDays(bump);

            return d.ToString("dd MMM yyyy").ToUpper();
        }

        Regex _anyDate = new Regex(@"(?i)(?<date>\d{1,2}\s+(jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)[A-Za-z]*,?\s\d{4})|(?<date>(jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)[A-Za-z]*\s+\d\d?,?\s+\d{4})");
        private string RandomizeAnyDate(string txt)
        {
            if ((txt ?? "").Length < 10)
                return txt;

            MatchCollection matches = _anyDate.Matches(txt);
            if (matches.Count == 0)
                return txt;

            StringBuilder sb = new StringBuilder(txt);
            for (int i = matches.Count - 1; i >= 0; i--)
            {
                if (!DateTime.TryParse(matches[i].Value, out DateTime dt))
                    continue;
                int bump = Randomizer.Next(1, 60) - 30;
                sb.Replace(matches[i].Value, dt.AddDays(bump).ToString("dd MMM yyyy"), matches[i].Index, matches[i].Length);
            }

            return sb.ToString();
        }


        private bool Scrub(Tag tag, StringBuilder sb, bool continued)
        {
            // if the tag has no content, just pass it through
            if (!continued && string.IsNullOrEmpty(tag.Content))
            {
                sb.AppendLine(tag.ReAssemble());
                return false;
            }

            // pull the full text and scrub it of names and URLs
            string fullText = tag.FullText();
            string scrubText = _cleaner.PruneURLs(_namePool.Scrub2(fullText) ?? fullText);
            tag.Content = scrubText ?? fullText;
           string c = tag.ReAssemble();
            int lvl = tag.Level + 1;

            // output as one or more tags - original plus any needed CONC
            EmitContinuableText(sb, c, lvl);
            return true;
        }

        private static void EmitContinuableText(StringBuilder sb, string tagLine, int level)
        {
            // break it up if it gets too long; shouldn't the TAG do that?
            while (tagLine.Length > OutputLineLength)
            {
                int at = OutputLineLength - 1;
                while (tagLine[at] == ' ' && at > 40) at--;
                sb.AppendLine(tagLine.Substring(0, at));
                tagLine = $"{level} CONC {tagLine.Substring(at)}";
            }

            sb.AppendLine(tagLine);
        }

        private string InitCaps(string s)
        {
            if ((s?.Length ?? 0) < 2 || !char.IsLetter(s[0]))
                return s;
            return $"{s.Substring(0, 1).ToUpper()}{s.Substring(1).ToLower()}";
        }

        private void Ascend(GedcomIndividual indi, bool isImmune)
        {
            if (indi == null) 
                return;
            if (_visitedIndis.Contains(indi.IndividualView.Id))
                return;
            _visitedIndis.Add(indi.IndividualView.Id);
            isImmune |= (indi.IntYearBorn ?? 9999) < Fence;
            isImmune |= (indi.IntYearDied ?? 9999) < Fence;
            if (isImmune)
                _immune.Add(indi.IndividualView.Id);
            Ascend(indi.ChildhoodFamily?.Husband, isImmune);
            Ascend(indi.ChildhoodFamily?.Wife, isImmune);
        }
    }

    public static class GedcomExtensions
    {
        public static string ReAssemble(this Tag t)
        {
            string id = t.Id == null ? null : $" {t.Id}";
            string content = t.Content == null ? null : $" {t.Content}";
            return $"{t.Level}{id} {t.Code}{content}";
        }
    }
}
