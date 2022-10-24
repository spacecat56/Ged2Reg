// GedcomFile.cs
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
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleGedcomLib
{
    public class GedcomFile
    {
        private static Regex _invalidXmlChars = new Regex(
            @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
            RegexOptions.Compiled);

        public Tag Header { get; set; }
        public List<Tag> Individuals { get; set; } = new List<Tag>();
        public List<Tag> Families { get; set; } = new List<Tag>();
        public List<Tag> Notes { get; set; } = new List<Tag>();
        public List<Tag> Sources { get; set; } = new List<Tag>();
        public List<Tag> Repositories { get; set; } = new List<Tag>();
        public List<Tag> MediaObjects { get; set; } = new List<Tag>();
        public List<Tag> OtherRoots { get; set; } = new List<Tag>();

        public Dictionary<string, Tag> RefMap { get; set; } = new Dictionary<string, Tag>();
        public List<Tag> AllTags { get; set; } = new List<Tag>();
        public HashSet<string> UnknownTags { get; set; } = new HashSet<string>();
        public List<Tag> LostTags { get; set; } = new List<Tag>();
        

        public Tag FirstPerson { get; set; }

        public List<SourceView> SourceViews { get; set; } = new List<SourceView>();
        public List<MediaObjectView> MediaObjectViews { get; set; } = new List<MediaObjectView>();
        public List<CitationView> CitationViews { get; set; } = new List<CitationView>();
        public List<IndividualView> IndividualViews { get; set; } = new List<IndividualView>();
        public Dictionary<string, IndividualView> IndividualsMap { get; private set; } = new Dictionary<string, IndividualView>();
        public List<FamilyView> FamilyViews { get; set; } = new List<FamilyView>();


        public List<string> InvalidText { get; set; } = new List<string>();

        public Exception LastException { get; set; }
        public int Links { get; set; }
        public int CitSourceLinks { get; set; }
        public int CitMediaLinks { get; set; }
        public int CitIndiLinks { get; set; }
        public int SrcMediaLinks { get; set; }
        public int FamSpouseLinks { get; set; }

        private bool _isInit;
        public GedcomFile Init(Stream mapStream = null)
        {
            if (_isInit && mapStream == null)
                return this;
            _isInit = true;
            string[] resNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            string rn = resNames.FirstOrDefault(s => s.Contains("TagMap"));

            using (Stream s = mapStream ?? Assembly.GetExecutingAssembly().GetManifestResourceStream(rn))
            {
                if (s == null) return this;
                Tag.TagMapping = Tag.TagMapping ?? new Dictionary<string, TagCode>();
                using (StreamReader sr = new StreamReader(s))
                {
                    string t = null;
                    while ((t = sr.ReadLine()) != null)
                    {
                        if (t.StartsWith("#")) continue;
                        string[] mapping = t.Split(':');
                        if (mapping.Length != 2) continue;
                        if (!Enum.TryParse(mapping[1], out TagCode mappedCode)) continue;
                        if (!Tag.TagMapping.ContainsKey(mapping[0]))
                            Tag.TagMapping.Add(mapping[0], mappedCode);
                    }
                }
            }
            return this;
        }

        public GedcomFile Parse(string path)
        {
            try
            {
                Init();
                using (TextReader tr = new StreamReader(path))
                {
                    Tag current = null;
                    string line;
                    while ((line = Clean(tr.ReadLine())) != null)
                    {

                        Tag t = new Tag(line, null);
                        if (!t.IsValid)
                        {
                            InvalidText.Add(line);
                            continue;
                        }
                        AllTags.Add(t);
                        if (t.HasIdentifier && !RefMap.ContainsKey(t.Id))
                            RefMap.Add(t.Id, t);
                        if (t.Code == TagCode.UNK)
                        {
                            if (!UnknownTags.Contains(t.TagText))
                                UnknownTags.Add(t.TagText);
                        }
                        while (current != null && t.Level <= current.Level)
                            current = current.ParentTag;
                        t.ParentTag = current;
                        if (current == null)
                        {
                            current = t;
                            if (current.Level != 0)
                            {
                                LostTags.Add(current);
                            }
                            else
                            {
                                switch (current.Code)
                                {
                                    case TagCode.INDI:
                                        Individuals.Add(current);
                                        if (FirstPerson == null)
                                            FirstPerson = Individuals[Individuals.Count - 1];
                                        break;
                                    case TagCode.FAM:
                                        Families.Add(current);
                                        break;
                                    case TagCode.NOTE:
                                        Notes.Add(current);
                                        break;
                                    case TagCode.SOUR:
                                        Sources.Add(current);
                                        break;
                                    case TagCode.REPO:
                                        Repositories.Add(current);
                                        break;
                                    case TagCode.OBJE:
                                        MediaObjects.Add(current);
                                        break;
                                    case TagCode.HEAD:
                                        Header = current;
                                        break;
                                    default:
                                        OtherRoots.Add(current);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            current.Children.Add(t);
                            current = t;
                        }
                    }
                }
                foreach (Tag tag in AllTags)
                {
                    if (tag.Link(RefMap)) Links++;
                }

                foreach (Tag source in Sources)
                {
                    SourceViews.Add(new SourceView(source));
                }

                foreach (Tag tag in MediaObjects)
                {
                    MediaObjectViews.Add(new MediaObjectView(tag));
                }

                Dictionary<string, Tag> notesMap = Notes.ToDictionary(n => n.Id);

                foreach (Tag tag in Individuals)
                {
                    IndividualView iv = new IndividualView(tag);
                    IndividualViews.Add(iv);
                    iv.Link(notesMap);
                    string ivk = iv.Id;
                    // we need a map to eliminate performance hot spot linking families
                    if (!string.IsNullOrEmpty(ivk) && !IndividualsMap.ContainsKey(ivk))
                        IndividualsMap.Add(ivk, iv);
                }

                foreach (Tag family in Families)
                {
                    FamilyView fv = new FamilyView(family);
                    FamilyViews.Add(fv);
                    FamSpouseLinks += fv.Link(IndividualsMap);
                }

                foreach (Tag tag in AllTags)
                {
                    if (tag.Level <= 0 || tag.Code != TagCode.SOUR) continue;
                    CitationView cv = new CitationView(tag);
                    CitationViews.Add(cv);
                    if (cv.Link(SourceViews)) CitSourceLinks++;
                    CitMediaLinks += cv.Link(MediaObjectViews);
                    //CitIndiLinks += cv.Link(IndividualViews);
                }

                // connect the media that are directly on sources
                foreach (SourceView sv in SourceViews)
                {
                    if (!sv.SourceTag.Has(TagCode.OBJE)) continue;
                    SrcMediaLinks += sv.Link(MediaObjectViews);
                }

            }
            catch (Exception ex)
            {
                LastException = ex;
            }
            return this;
        }



        public static string Clean(string s)
        {
            return string.IsNullOrEmpty(s) ? s : _invalidXmlChars.Replace(s, "");
        }

        public string Report()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Tags: {AllTags.Count}");
            sb.AppendLine($"Persons: {Individuals.Count}");
            sb.AppendLine($"Families: {Families.Count}");
            sb.AppendLine($"Notes: {Notes.Count}");
            sb.AppendLine($"Sources: {Sources.Count}");
            sb.AppendLine($"Repositories: {Repositories.Count}");
            sb.AppendLine($"MediaObjects: {MediaObjects.Count}");
            sb.AppendLine($"OtherRoots: {OtherRoots.Count}");
            sb.AppendLine($"UnknownTags: {UnknownTags.Count}");
            sb.AppendLine($"LostTags: {LostTags.Count}");
            sb.AppendLine($"Id'd objects: {RefMap.Count}");
            sb.AppendLine($"Cit-Source Links: {CitSourceLinks}");
            sb.AppendLine($"Cit-Media Links: {CitMediaLinks}");
            sb.AppendLine($"Src-Media Links: {SrcMediaLinks}");
            sb.AppendLine($"Tag Links: {Links}");
            sb.AppendLine($"Invalid lines: {InvalidText.Count}");

            sb.AppendLine($"Citation views: {CitationViews.Count}");
            sb.AppendLine($"Media object views: {MediaObjectViews.Count}");
            sb.AppendLine($"Source views: {SourceViews.Count}");

            if (LastException != null)
                sb.AppendLine($"Exception: {LastException.Message}");

            return sb.ToString();
        }

        public string ReportStats(string id = null, int maxgen = 99)
        {
            StringBuilder sb = new StringBuilder();

            Tag start = FirstPerson;
            if (id != null)
                start = Individuals.FirstOrDefault(x => id.Equals(x.Id));
            if (start == null)
                return null;
            IndividualView startIv = IndividualViews.FirstOrDefault(x => x.IndiTag == start);
            if (startIv == null)
                return null;

            string note = ($"Home person is {startIv.Id} ({startIv.Name})");
            sb.AppendLine(note);

            BigInteger[,] genCounts = new BigInteger[maxgen, 2];
            RecurseTree(0, startIv, genCounts, sb);

            BigInteger thisGenTotal = 1;
            BigInteger mTotal = 0;
            BigInteger fTotal = 0;
            for (int i = 0; i < maxgen; i++)
            {
                BigInteger ms = genCounts[i, 0];
                BigInteger fs = genCounts[i, 1];
                if (ms + fs == 0)
                    continue;
                mTotal += ms;
                fTotal += fs;
                thisGenTotal *= 2;
                int gnbr = i + 1;

                double coverage = (double) (100 * (ms + fs)) / (double) thisGenTotal;

                string s =
                    $"Gen {gnbr, 2} ({GenerationString(i), 5}):   Males {ms, 3}, Females: {fs, 3};  Slots {thisGenTotal, 7};  Known: {coverage, 7:##0.000}% ";

                sb.AppendLine(s);
            }

            sb.AppendLine($"Total known ancestors: Male {mTotal,4}; Female {fTotal,4}");

            return sb.ToString();
        }

        public static string GenerationString(int generation)
        {
            switch (generation)
            {
                case 0:
                    return "I";
                case 1:
                    return "P";
                case 2:
                    return "GP";
                case 3:
                    return "GGP";
                default:
                    return $"{generation - 2}GGP";
            }
        }

        public void RecurseTree(int gen, IndividualView indi, BigInteger[,] genCounts, StringBuilder sb)
        {

            List<FamilyView> myFamilies = FamilyViews.FindAll(x => x.Chiildren.Contains(indi));

            if (myFamilies.Count == 0)
                return;

            if (myFamilies.Count > 1)
            {
                // todo: what?
                string note = ($"Person {indi.Id} ({indi.Name}) is a child in {myFamilies.Count} families");
                sb.AppendLine(note);
            }

            gen++;

            if (myFamilies[0].Husband != null)
            {
                genCounts[gen, 0]++;
                RecurseTree(gen, myFamilies[0].Husband, genCounts, sb);
            }

            if (myFamilies[0].Wife == null) return;
            genCounts[gen, 1]++;
            RecurseTree(gen, myFamilies[0].Wife, genCounts, sb);

        }

        private Dictionary<string, List<FamilyView>> _indiIdToFamilyViewsMap;

        public List<FamilyView> FindFamilies(IndividualView iv)
        {
            (_indiIdToFamilyViewsMap ??= BuildFvMap()).TryGetValue(iv.Id, out List<FamilyView> rv);
            return rv??new List<FamilyView>();
        }

        private Dictionary<string, List<FamilyView>> BuildFvMap()
        {
            _indiIdToFamilyViewsMap = new Dictionary<string, List<FamilyView>>();

            foreach (FamilyView fv in FamilyViews)
            {
                if (fv?.Husband?.Id != null)
                    Map(_indiIdToFamilyViewsMap, fv.Husband.Id, fv);
                if (fv?.Wife?.Id != null)
                    Map(_indiIdToFamilyViewsMap, fv.Wife.Id, fv);
            }
            return _indiIdToFamilyViewsMap;
        }

        private void Map(Dictionary<string, List<FamilyView>> d, string id, FamilyView fv)
        {
            if (!d.TryGetValue(id, out List<FamilyView> list))
            {
                d.Add(id, list = new List<FamilyView>());
            }
            list.Add(fv);
        }
    }
}
