// AbstractBindableModel.cs
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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace CommonClassesLib
{
    [DataContract]
    public abstract class AbstractBindableModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected static bool SuppressEvents { get; set; } = false;

        // note: with .NET 4.5 we could use [CallerMemberName]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (SuppressEvents) return; // this turns out to be not needed
            if (string.IsNullOrEmpty(propertyName)) return;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
