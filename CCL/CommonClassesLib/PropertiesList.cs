// PropertiesList.cs
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
using System.ComponentModel;
using System.Reflection;

namespace CommonClassesLib
{
    public class PropertiesList<TT> : SortableBindingList<NamedValue<PropertyInfo>>
    {
        public Exception LastException { get; set; }

        public PropertiesList() { }

        public PropertiesList(string[] names)
        {
            Init(names);
        }

        public void Init(string[] names)
        {
            try
            {
                foreach (string name in names)
                {
                    if (name == null)
                    {
                        Items.Add(new NamedValue<PropertyInfo>("", null));
                        continue;
                    }
                    PropertyInfo p = typeof(TT).GetProperty(name);
                    if (p==null) continue;
                    Items.Add(new NamedValue<PropertyInfo>(name, p));
                }
            }
            catch (Exception ex)
            {
                LastException = ex;
            }
        }

        public void AutoSort()
        {
            PropertyDescriptor pd = TypeDescriptor.GetProperties(typeof(NamedValue<PropertyInfo>))["Name"];
            ApplySortCore(pd, ListSortDirection.Ascending);
        }

    }

    public class PropertyDescriptorsList<T> : SortableBindingList<NamedValue<PropertyDescriptor>>
    {
        public Exception LastException { get; set; }

        public PropertyDescriptorsList()
        {
            Init(null);
        }

        public PropertyDescriptorsList(string[] names)
        {
            Init(names);
        }

        public void Init(string[] names)
        {
            try
            {
                PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(typeof(T));
                HashSet<string> wanted = names != null ? new HashSet<string>(names) : new HashSet<string>();
                if (names == null)
                {
                    foreach (object o in pdc)
                    {
                        wanted.Add((o as PropertyDescriptor)?.Name);
                    }
                }
                foreach (string name in wanted)
                {
                    if (name == null)
                    {
                        Items.Add(new NamedValue<PropertyDescriptor>("", null));
                        continue;
                    }
                    PropertyDescriptor p = pdc[name];
                    if (p == null) continue;
                    Items.Add(new NamedValue<PropertyDescriptor>(name, p));
                }
                AutoSort();
            }
            catch (Exception ex)
            {
                LastException = ex;
            }
        }

        public void AutoSort()
        {
            PropertyDescriptor pd = TypeDescriptor.GetProperties(typeof(NamedValue<PropertyDescriptor>))["Name"];
            ApplySortCore(pd, ListSortDirection.Ascending);
        }

    }
}
