﻿// ListOfSettingsSets.cs
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
using System.Runtime.Serialization;
using CommonClassesLib;

namespace Ged2Reg.Model
{
    //[DataContract] // does not work here because of the way we use the enclosed type
    public class ListOfSettingsSets : List<G2RSettings>
    {
        public const string DefaultSetName = "DefaultSettings";

        //[DataMember] public string LastActiveSet { get; set; }

        public ListOfSettingsSets Save(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = GetPath();
            }

            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            SimpleSerializer<ListOfSettingsSets> ser = new SimpleSerializer<ListOfSettingsSets>();
            ser.Persist(path, this);
            return this;
        }
        public static ListOfSettingsSets Load(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = GetPath();
            }

            if (!File.Exists(path))
            {
                ListOfSettingsSets rv0 = new ListOfSettingsSets();
                try
                {
                    G2RSettings rvi = G2RSettings.Load();
                    rvi.SetName = DefaultSetName;
                    rv0.Add(rvi);
                }
                catch (FileNotFoundException)
                {
                    G2RSettings rvi = new G2RSettings() { SetName = DefaultSetName }.Defaults().Init(); ;
                    rv0.Add(rvi);
                }

                rv0.Save();
                return rv0;
            }
            SimpleSerializer<ListOfSettingsSets> ser = new SimpleSerializer<ListOfSettingsSets>();
            ListOfSettingsSets rv = ser.Load(path);
            return rv;
        }

        public G2RSettings GetSettings(string name = DefaultSetName, G2RSettings cloneFrom = null, bool preferLastActive = false)
        {
            G2RSettings rv = preferLastActive
                ? Find(s => s.LastActive)
                : null;
            rv ??= Find(s => name.Equals(s.SetName));
            if (rv != null) return rv.InitInternals();
            rv = new G2RSettings(){SetName = name}.Defaults().Init();
            if (cloneFrom != null)
            {
                // todo: clone settings
            }
            Add(rv);
            return rv;
        }

        private static string GetPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"West Leitrim Software\Ged2Reg\Ged2RegSettingsSets.xml");
        }

        public void Update(G2RSettings settings)
        {
            G2RSettings victim = Find(s => settings.SetName.Equals(s.SetName));
            if (victim != null)
                Remove(victim);
            Add(settings);
        }
    }
}