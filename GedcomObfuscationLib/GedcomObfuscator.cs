using System;
using System.Collections.Generic;
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
        public int Fence { get; set; } = 1800;

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

        public bool Exec()
        {
            _visitedIndis = new HashSet<string>(_file.IndividualViews.Count * 4);
            _immune = new HashSet<string>(_file.IndividualViews.Count * 4);

            try
            {
                // todo
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
                            break;
                        case 1:
                            if (zeroLevel.Code == TagCode.INDI)
                            {
                                switch (tag.Code)
                                {
                                    case TagCode.NAME:
                                        if (_immune.Contains(zeroLevel.Id))
                                            break;
                                        if (!_individualViews.TryGetValue(zeroLevel.Id, out GedcomIndividual gi))
                                            break;
                                        string surn = _namePool.MappedSurname(gi.Surname);
                                        string gnam = gi.Gender == "M"
                                            ? _namePool.MappedFname(gi.GivenName)
                                            : _namePool.MappedMname(gi.GivenName);
                                        tag.Content = $"{gnam} /{surn}/";
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
                foreach (Tag tag in _file.AllTags)
                {
                    sb.AppendLine(tag.ReAssemble());
                }

                _cleaner = new ContentCleaner();
                _newText = _cleaner.PruneURLs(sb.ToString());

                File.WriteAllText(FileNameOut, _newText);

                return true;
            }
            catch (Exception ex)
            {
                LastException = ex;
                return false;
            }
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
