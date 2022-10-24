// MediaObjectView.cs
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

namespace SimpleGedcomLib
{
    public class MediaObjectView
    {
        private Tag _objectTag;

        public Tag ObjectTag
        {
            get { return _objectTag; }
            set
            {
                if (value == null || value.Code != TagCode.OBJE || value.Level != 0)
                    throw new ArgumentException("not a level-0 Object tag");
                _objectTag = value;
            }
        }

        public List<CitationView> Citations = new List<CitationView>();
        public SourceView SourceView { get; set; }

        public string FilePath => _objectTag?.GetChild(TagCode.FILE)?.Content;
        public string FileDate => _objectTag?.GetChild(TagCode.FILE)?.GetChild(TagCode._DATE)?.Content;
        public string Caption => _objectTag?.GetChild(TagCode.FILE)?.GetChild(TagCode.TITL)?.FullText();
        public string Comment => _objectTag?.GetChild(TagCode.FILE)?.GetChild(TagCode.TEXT)?.FullText();
        public string Id => _objectTag?.Id;
        public bool IsPrivate => _objectTag.Has(TagCode._PRIV);

        public MediaObjectView(Tag t)
        {
            ObjectTag = t;
        }
    }
}