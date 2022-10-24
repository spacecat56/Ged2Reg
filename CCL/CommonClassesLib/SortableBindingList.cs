// SortableBindingList.cs
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CommonClassesLib
{
    public class SortableBindingList<TT> : BindingList<TT>
    {
        private bool _mSorted;
        private ListSortDirection _mSortDirection = ListSortDirection.Ascending;
        private PropertyDescriptor _mSortProperty;

        protected override bool SupportsSortingCore => true;
        protected override bool IsSortedCore => _mSorted;
        protected override ListSortDirection SortDirectionCore => _mSortDirection;
        protected override PropertyDescriptor SortPropertyCore => _mSortProperty;

        public PropertyInfo SortKey2 { get; set; }
        public PropertyInfo SortKey3 { get; set; }

        public ICustomFilter<TT> Filter { get; private set; }
        private List<TT> _completeList;

        public int UnfilteredCount => _completeList?.Count ?? Items.Count;

        public void AddAll(IEnumerable<TT> source)
        {
            foreach (TT t in source)
            {
                Items.Add(t);
            }
        }

        // todo: protect the list integrity while filtered!

        public void ApplyFilter(ICustomFilter<TT> filter)
        {
            try
            {
                RaiseListChangedEvents = false;
                if (filter == null)
                {
                    if (_completeList == null) return;
                    Items.Clear();
                    foreach (TT t in _completeList)
                    {
                        Items.Add(t);
                    }
                    Filter = null;
                    return;
                }
                if (Filter == null)
                {
                    _completeList = new List<TT>(Items);
                }
                Filter = filter;
                List<TT> selecteds = _completeList.FindAll(x => Filter.Passes(x));
                Items.Clear();
                foreach (TT selected in selecteds)
                {
                    Items.Add(selected);
                }
            }
            finally
            {
                RaiseListChangedEvents = true;
                OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, 0));
            }
        }

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            _mSortDirection = direction;
            _mSortProperty = prop;
            var listRef = Items as List<TT>;
            if (listRef == null || listRef.Count == 0)
                return;

            PropertyComparer<TT> pc = new PropertyComparer<TT>(prop, direction == ListSortDirection.Descending);
            if (SortKey2 != null)
            {
                AncillaryComparer<TT> s2 = new AncillaryComparer<TT>(SortKey2);
                pc.Next = s2;
                if (SortKey3 != null)
                {
                    AncillaryComparer<TT> s3 = new AncillaryComparer<TT>(SortKey3);
                    s2.Next = s3;
                }
            }

            listRef.Sort(pc);

            _mSorted = true;

            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
    }

    /// <summary>
    /// This uses a PropertDescriptor, because that is what the grid automatic sort 
    /// provides. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyComparer<T> : IComparer<T>
    {
        public bool Descending { get; }
        public PropertyDescriptor Property { get; }
        public IComparer<T> Next { get; set; }

        public PropertyComparer(PropertyDescriptor property, bool descending)
        {
            if (property == null)
                throw new
                    ArgumentNullException(nameof(property));
            Descending = descending;
            Property = property;
        }

        public int Compare(T x, T y)
        {
            object xVal = (x == null) ? null : Property.GetValue(x);
            object yVal = (y == null) ? null : Property.GetValue(y);
            int value;
            if (xVal == null)
            {
                if (yVal == null)
                    value = 0;
                else
                    value = -1;
            }
            else if (yVal == null)
            {
                value = 1;
            }
            else
            {
                value = Comparer.Default.Compare(xVal, yVal);
            }

            if (value == 0 && Next != null)
                value = Next.Compare(x, y);

            return Descending ? -value : value;
        }
    }

    /// <summary>
    /// This uses a PropertyInfo, because there is no way to get a PropertyDescriptor
    /// except when the internals of the Forms machinery hands one to you.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AncillaryComparer<T> : IComparer<T>
    {
        public PropertyInfo Property { get; }
        public IComparer<T> Next { get; set; }

        public AncillaryComparer(PropertyInfo property)
        {
            if (property == null)
                throw new
                    ArgumentNullException(nameof(property));
            Property = property;
        }

        public int Compare(T x, T y)
        {
            object xVal = (x == null) ? null : Property.GetValue(x);
            object yVal = (y == null) ? null : Property.GetValue(y);
            int value;
            if (xVal == null)
            {
                if (yVal == null)
                    value = 0;
                else
                    value = -1;
            }
            else if (yVal == null)
            {
                value = 1;
            }
            else
            {
                value = Comparer.Default.Compare(xVal, yVal);
            }

            if (value == 0 && Next != null)
                value = Next.Compare(x, y);

            return value;
        }
    }

    public interface ICustomFilter<TT>
    {
        ICustomFilter<TT> Next { get; set; }
        bool Passes(TT candidate);
        void Append(ICustomFilter<TT> test);
        List<IPropertyEvaluator> Unpack();
    }

    public abstract class CustomFilterBase<TTt> : ICustomFilter<TTt>
    {
        #region Implementation of ICustomFilter<TTT>

        public ICustomFilter<TTt> Next { get; set; }
        public IPropertyEvaluator Evaluator { get; set; }

        public bool Passes(TTt candidate)
        {
            return Evaluate(candidate);
        }

        public void Append(ICustomFilter<TTt> test)
        {
            ICustomFilter<TTt> tail = this;
            while (tail.Next != null) tail = tail.Next;
            tail.Next = test;
        }

        public List<IPropertyEvaluator> Unpack()
        {
            List<IPropertyEvaluator> rvl = new List<IPropertyEvaluator>();
            CustomFilterBase<TTt> chainy = this;
            while (chainy != null)
            {
                rvl.Add(chainy.Evaluator);
                chainy = chainy.Next as CustomFilterBase<TTt>;
            }
            return rvl;
        }

        protected bool Evaluate(TTt candidate)
        {
            return Eval(candidate) && (Next?.Passes(candidate) ?? true);
        }

        protected virtual bool Eval(TTt candidate)
        {
            return Evaluator?.Evaluate(candidate) ?? true;
        }

        #endregion
    }

    public enum ComparisonOperation
    {
        Undefined,
        Equal,
        LessThan,
        GreaterThan,
        LessOrEqual,
        GreaterOrEqual,
        Contains,
        NullOrEmpty,
        Match
    }

    public enum ComparisonSense
    {
        Undefined,
        Is,
        Not
    }

    public interface IPropertyEvaluator
    {
        bool Evaluate(object candidate);
        bool Validate(ref string msg);
        string Description { get; }
    }

    public class PropertyEvaluator<TTtt> : IPropertyEvaluator where TTtt : IComparable 
    {
        public PropertyDescriptor Descriptor { get; set; }
        public TTtt ComparedValue { get; set; }
        public ComparisonSense Sense { get; set; }
        public ComparisonOperation Comparison { get; set; }

        public string Description => $"{Descriptor?.Name} {Sense} {Comparison} '{ComparedValue}'";

        private Regex _regex;
        private Regex GetRegex()
        {
            string rex = ComparedValue as string;
            if (string.IsNullOrEmpty(rex)) return null;
            if (_regex != null && _regex.ToString().Equals(rex))
                return _regex;
            _regex = new Regex(rex);
            return _regex;
        }

        #region Implementation of IPropertyEvaluator

        public bool Evaluate(object candidate)
        {
            object cVal = Descriptor?.GetValue(candidate);
            if (Comparison == ComparisonOperation.Contains)
            {
                string bar = cVal?.ToString() ?? "";
                string foo = ComparedValue?.ToString() ?? "534589734yt87yw89hsifuaifadfugagfe7rgdfadfhdjhfh";
                return Sense==ComparisonSense.Is ? bar.Contains(foo) : !bar.Contains(foo);
            }
            if (Comparison == ComparisonOperation.NullOrEmpty)
            {
                string maybe = cVal as string;
                if ("".Equals(maybe)) return Sense == ComparisonSense.Is;
                return Sense == ComparisonSense.Is ? cVal == null : cVal != null;
            }
            if (Comparison == ComparisonOperation.Match)
            {
                string maybe = cVal as string;
                Regex rex = GetRegex();
                if (rex == null || maybe == null) return false;
                bool rv0  = rex.IsMatch(maybe);
                return Sense == ComparisonSense.Is ? rv0 : !rv0;
            }

            int c = (cVal == null || cVal is TTtt) ? -ComparedValue.CompareTo(cVal) : -1;
            bool rv = false;
            switch (Comparison)
            {
                case ComparisonOperation.Equal:
                    rv = c == 0;
                    break;
                case ComparisonOperation.GreaterOrEqual:
                    rv = c >= 0;
                    break;
                case ComparisonOperation.GreaterThan:
                    rv = c == 1;
                    break;
                case ComparisonOperation.LessOrEqual:
                    rv = c <= 0;
                    break;
                case ComparisonOperation.LessThan:
                    rv = c == -1;
                    break;
            }
            if (Sense == ComparisonSense.Not)
                rv = !rv;
            return rv;
        }

        public bool Validate(ref string msg)
        {
            if (Descriptor == null)
            {
                msg = "Property not selected";
                return false;
            }
            if (Sense == ComparisonSense.Undefined)
            {
                msg = "Sense not selected";
                return false;
            }
            if (Comparison == ComparisonOperation.Undefined)
            {
                msg = "Comparison not selected";
                return false;
            }
            if (typeof(TTtt).IsAssignableFrom(typeof(bool)) && Comparison != ComparisonOperation.Equal)
            {
                msg = "Comparison of bool is limited to Equal";
                return false;
            }
            if (!typeof(TTtt).IsEquivalentTo(typeof(string)) && Comparison == ComparisonOperation.Contains)
            {
                msg = "'Contains' test only applies to strings";
                return false;
            }
            if (ComparedValue == null)
            {
                msg = "Warning: comapring to null";
            }

            return true;
        }

        #endregion
    }
    
}
