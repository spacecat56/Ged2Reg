// TextCleaner.cs
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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CommonClassesLib;

namespace Ged2Reg.Model
{
     [DataContract]
     public class TextCleaner
     {
        // special handling for deserialization to set parent

        private static TextCleaner _titleCleaner;

        [DataMember]
        public static TextCleaner TitleCleaner
        {
            get => _titleCleaner ?? (_titleCleaner = new TextCleaner());
            set => _titleCleaner = value ?? new TextCleaner();
        }

        [DataMember]
        public List<TextCleanerEntry> Cleaners { get; set; } = new List<TextCleanerEntry>();

        internal HashSet<string> StringsWeHaveSeen = new HashSet<string>();

        public TextCleaner() { }

        public TextCleaner(IEnumerable<TextCleanerEntry> parts)
        {
            Init(parts);
        }

        public TextCleaner Init(IEnumerable<TextCleanerEntry> parts)
        {
            Cleaners = parts?.ToList() ?? new List<TextCleanerEntry>();

            return Init();
        }

        public TextCleaner Init()
        {
            StringsWeHaveSeen.Clear();
            foreach (TextCleanerEntry cleaner in Cleaners ?? new List<TextCleanerEntry>())
            {
                cleaner.Parent = this;
            }

            return this;
        }

        public string Exec(string txt, TextCleanerContext ctx)
        {
            foreach (TextCleanerEntry cleaner in Cleaners)
            {
                txt = cleaner.Exec(txt, ctx);
            }

            return txt;
        }

        internal bool IsRepeated(string s)
        {
            if (StringsWeHaveSeen.Contains(s))
                return true;
            StringsWeHaveSeen.Add(s);
            return false;
        }
    }

    [DataContract]
    public class TextCleanerEntry
    {
        [DataMember]
        public TextCleanerContext Context { get; set; }
        [DataMember]
        public string Input { get; set; }
        [DataMember]
        public string Output { get; set; }
        [DataMember]
        public bool FirstUseUnchanged { get; set; }
        [DataMember]
        public bool ReplaceEntire { get; set; }
        [IgnoreDataMember]
        public TextCleaner Parent { get; set; }

        public string Exec(string txt, TextCleanerContext ctx)
        {
            string Apply()
            {
                if (string.IsNullOrEmpty(txt) || !txt.Contains(Input))
                    return txt;
                if (FirstUseUnchanged && !Parent.IsRepeated(txt))
                    return txt;
                return ReplaceEntire ? Output : txt.Replace(Input, Output);
            }

            if (Context == TextCleanerContext.Nowhere)
                return txt;

            if (Context == TextCleanerContext.Everywhere) 
                return Apply();

            if (ctx == Context)
                return Apply();

            return txt;
        }
    }

    public enum TextCleanerContext
    {
        Nowhere,
        Everywhere,
        FullCitation,
        SeeNote,
        //Repetition,
        OthersList
    }

    //public enum TextCleanerOperationContext
    //{
    //    Citation,
    //    SeeNote,
    //    ListOthers
    //}

    public class ListOfTextCleanerEntry : SortableBindingList<TextCleanerEntry> { }

}
