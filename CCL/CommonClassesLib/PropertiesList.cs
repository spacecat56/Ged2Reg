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
