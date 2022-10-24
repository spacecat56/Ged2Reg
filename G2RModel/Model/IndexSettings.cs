// IndexSettings.cs
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

using CommonClassesLib;

namespace Ged2Reg.Model
{
    public enum IndexRole
    {
        Names,
        Places
    }

   public class IndexSettings : AbstractBindableModel
    {
        private string _indexName = "names";
        private string _indexHeading = "Index of Names";
        private bool _enabled = true;
        private int _columns = 2;
        private string _sep = "\t";

        private IndexRole _role;

        public IndexRole Role
        {
            get => _role;
            set { _role = value; OnPropertyChanged();}
        }

        // looks good this way:
        // \e "	" \c "2" \z "1033"


        public string Separator
        {
            get { return _sep; }
            set { _sep = value; }
        }

        public int Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }

        public string IndexHeading
        {
            get { return _indexHeading; }
            set { _indexHeading = value; OnPropertyChanged(); }
        }

        //private string x = "\\e \"\t\" \\c \"2\" \\z \"1033\"";

        public string IndexName
        {
            get { return _indexName; }
            set { _indexName = value; OnPropertyChanged(); }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; OnPropertyChanged(); }
        }
    }
}