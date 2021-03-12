using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Ged2Reg.Model;
using SimpleGedcomLib;

namespace GedcomObfuscationLib
{
    public class GedcomObfuscator
    {
        public string FileName { get; set; }
        public Exception LastException { get; set; }
        public string FileNameOut { get; set; }
        public int Fence { get; set; } = 1700;

        private GedcomFile _file;
        private NamesPools _namePool;
        private ContentCleaner _cleaner;
        private string _newText;
        private HashSet<string> _immune;
        private RegisterReportModel _rrm;
        private Dictionary<string, GedcomIndividual> _individualViews;

        private HashSet<string> _visitedIndis;
        private string _fileName;
        private long _fileSize;

        private Random _random;

        public Random Randomizer => _random ??= new Random(DateTime.Now.Millisecond);

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

                _individualViews = new Dictionary<string, GedcomIndividual>();
                foreach (GedcomIndividual gi in _rrm.Individuals)
                {
                    gi.FindFamilies(true);
                    IndividualView indi = gi.IndividualView;
                    _individualViews.Add(indi.Id, gi);
                    _namePool.AssignSurname(indi.Surname);
                    string sex = indi.IndiTag.GetFirstDescendant(TagCode.SEX)?.Content;
                    if (sex == "M")
                    {
                        _namePool.AssignMame(indi.GivenName);
                    }
                    else
                    {
                        _namePool.AssignFname(indi.GivenName);
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

        public bool ExecBrute()
        {
            try
            {
                //StringBuilder sb = new StringBuilder(File.ReadAllText(teFile.Text));
                string s = File.ReadAllText(FileName);
                StringBuilder sb = ApplyTextReplacement(s);

                _cleaner = new ContentCleaner();
                _newText = _cleaner.PruneURLs(sb.ToString());
                return true;
            }
            catch (Exception ex)
            {
                LastException = ex;
                return false;
            }
        }

        public StringBuilder ApplyTextReplacement(string s)
        {
            List<TextReplacer> textReplacers = _namePool.GetReplacers();

            textReplacers = textReplacers.OrderByDescending(t => t.Length).ToList();
            foreach (TextReplacer replacer in textReplacers)
            {
                s = replacer.Apply(s);
            }

            StringBuilder sb = new StringBuilder(s);
            foreach (TextReplacer replacer in textReplacers)
            {
                replacer.Apply(sb);
            }

            return sb;
        }

        public int CountIndividuals { get; set; }
        public int CountImmune { get; set; }
        public int CountNoId { get; set; }
        public int CountSurnMapped { get; set; }
        public int CountGivenMapped { get; set; }
        public int CountSurnUnMapped { get; set; }
        public int CountGivenUnMapped { get; set; }

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

                                        if (!_individualViews.TryGetValue(zeroLevel.Id, out GedcomIndividual gi))
                                        {
                                            CountNoId++;
                                            break;
                                        }
                                        string surn = _namePool.MappedSurname(gi.Surname);
                                        if (surn.Equals(gi.Surname, StringComparison.InvariantCultureIgnoreCase))
                                            CountSurnUnMapped++;
                                        else
                                            CountSurnMapped++;
                                        string gnam = gi.Gender == "M"
                                            ? _namePool.MappedMname(gi.GivenName)
                                            : _namePool.MappedFname(gi.GivenName);
                                        if (gnam.Equals(gi.GivenName, StringComparison.InvariantCultureIgnoreCase))
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

                Debug.WriteLine("*** Name Mapping ***");
                Debug.WriteLine($"    Individuals...............{CountIndividuals}");
                Debug.WriteLine($"    Immune....................{CountImmune}");
                Debug.WriteLine($"    No Id.....................{CountNoId}");
                Debug.WriteLine($"    Surname unmapped..........{CountSurnUnMapped}");
                Debug.WriteLine($"    Surname mapped............{CountSurnMapped}");
                Debug.WriteLine($"    Given unmapped............{CountGivenUnMapped}");
                Debug.WriteLine($"    Given mapped..............{CountGivenMapped}");

                // write the file back to a memory buffer, then 
                // truncate all URLS, ...
                StringBuilder sb = new StringBuilder((int) _fileSize);
                int waitOnLevel = -1;
                foreach (Tag tag in _file.AllTags)
                {
                    if (waitOnLevel >= 0)
                    {
                        if (tag.Level > waitOnLevel)
                            continue;
                        waitOnLevel = -1;
                    }

                    switch (tag.Code)
                    {
                        case TagCode.UNK:
                        case TagCode.OBJE:
                        case TagCode.REPO:
                            waitOnLevel = tag.Level;
                            break;
                        case TagCode.CONT:
                        case TagCode.CONC:
                            break;
                        //case TagCode.AUTH:
                        //case TagCode.NOTE:
                        //case TagCode.TEXT:
                        //case TagCode.TITL:
                        //    // todo: scrub and re-emit the text
                        //    Scrub(tag, sb);
                        //    waitOnLevel = tag.Level;
                        //    //sb.AppendLine(tag.ReAssemble());
                        //    break;
                        case TagCode.DATE:
                            tag.Content = RandomizeDate(tag.Content);
                            sb.AppendLine(tag.ReAssemble());
                            break;
                        case TagCode.PAGE:
                            // overwrite the text, set the level to drop any continues,
                            // but fall through and keep the tag
                            tag.Content = "Page specific ref info removed";
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
                                Scrub(tag, sb);
                                // scrub will eat all CONT / CONC
                                // but this: is NOT the way to skip them!
                                //waitOnLevel = tag.Level;
                            }
                            break;
                    }

                }

                _cleaner = new ContentCleaner();
                _newText = _cleaner.PruneURLs(sb.ToString());

                //sb = ApplyTextReplacement(_newText);
                //_newText = sb.ToString();

                File.WriteAllText(FileNameOut, _newText);

                return true;
            }
            catch (Exception ex)
            {
                LastException = ex;
                return false;
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

        private void Scrub(Tag tag, StringBuilder sb)
        {
            //string newContent = null;

            //newContent = $"{tag.Code.Map()} (removed)";
            
            //switch (tag.Code)
            //{
            //    case TagCode.TITL:
            //    case TagCode.TEXT:
            //        break;
            //}

            // if the tag has no content, just pass it through
            if (string.IsNullOrEmpty(tag.Content))
            {
                sb.AppendLine(tag.ReAssemble());
                return;
            }
            // pull the full text and scrub it of names
            tag.Content = _namePool.Scrub2(tag.FullText()) ?? tag.Content;
            // break it up if it gets too long; shouldn't the TAG do that?
            string c = tag.ReAssemble();
            while (c.Length > 254)
            {
                int at = 253;
                while (c[at] == ' ' && at > 40) at--;
                sb.AppendLine(c.Substring(0, at));
                c = $"{tag.Level + 1} CONC {c.Substring(at)}";
            }
            sb.AppendLine(c);
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
