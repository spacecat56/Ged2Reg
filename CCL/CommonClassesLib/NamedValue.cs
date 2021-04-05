using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommonClassesLib {
    public class NamedValue<T>
    {

        public static SortableBindingList<NamedValue<T>> MakeList(IEnumerable<T> victims, string nameProp)
        {
            PropertyInfo[] pia = typeof(T).GetProperties();
            PropertyInfo pi = pia.FirstOrDefault(x => x.Name.Equals("Name"));
            SortableBindingList<NamedValue<T>> rvl = new SortableBindingList<NamedValue<T>>();
            foreach (T victim in victims)
            {
                rvl.Add(new NamedValue<T>() {Name = (string) pi?.GetValue(victim) ?? victim.ToString(), Value = victim});
            }
            return rvl;
        }

        public string Name { get; set; }
        public T Value { get; set; }

        public NamedValue() { }

        public NamedValue(string name, T value)
        {
            Name = name;
            Value = value;
        }
    }

    public class SequencedNamedValue<TT> : NamedValue<TT>
    {
        public int Sequence { get; set; }

        public SequencedNamedValue(int seq, string name, TT value) : base(name, value)
        {
            Sequence = seq;
        }
    }

    public class BoundStateNamedValue<TTt, TTtt> : NamedValue<TTt>
    {

        //public static SortableBindingList<BoundStateNamedValue<TTt>> Convert(IEnumerable<NamedValue<TTt>> these)
        //{
        //    SortableBindingList<BoundStateNamedValue<TTt>> rvl = new SortableBindingList<BoundStateNamedValue<TTt>>();
        //    foreach (NamedValue<TTt> value in these)
        //    {
        //        rvl.Add(new BoundStateNamedValue<TTt>(value));
        //    }
        //    return rvl;
        //}

        public TTtt State { get; set; } = default (TTtt);

        public BoundStateNamedValue() { }
        public BoundStateNamedValue(string name, TTt value) : base(name, value) { }
        public BoundStateNamedValue(NamedValue<TTt> nv) : base (nv.Name, nv.Value) { }

    }

}