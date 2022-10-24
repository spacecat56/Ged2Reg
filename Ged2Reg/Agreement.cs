// Agreement.cs
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
using System.IO;
using System.Runtime.Serialization;
using CommonClassesLib;

namespace Ged2Reg
{
    public enum StateOfPlay
    {
        None,
        EUL,
        Authorship
    }

    [DataContract]
    public class Agreement
    {
        #region persistence and constructor
        public static Agreement Load(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = GetPath();
            }
            if (!File.Exists(path))
                throw new FileNotFoundException();
            SimpleSerializer<Agreement> ser = new SimpleSerializer<Agreement>() { MaxStringLength = 16384};
            Agreement rv = ser.Load(path);
            return rv;
        }

        public Agreement Save(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = GetPath();
            }
            SimpleSerializer<Agreement> ser = new SimpleSerializer<Agreement>();
            ser.Persist(path, this);
            return this;
        }

        private static string GetPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"West Leitrim Software\Ged2Reg\AgreementRecord.xml");
        }

        public Agreement()
        {
            AgreedUser = Environment.UserName;
        }
        #endregion

        [DataMember] public DateTime? AgreedOn { get; set; }
        [DataMember] public string AgreedUser { get; set; }
        [DataMember] public string AgreedEul { get; set; }
        [DataMember] public string AgreedAuthorship { get; set; }
        [DataMember] public StateOfPlay Status { get; set; } = StateOfPlay.None;

        internal void Reset()
        {
            Status = StateOfPlay.None;
        }
    }
}
