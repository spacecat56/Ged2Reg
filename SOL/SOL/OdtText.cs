using System.Collections.Generic;
using System.Xml.Linq;

namespace SOL
{
    public class OdtText
    {
        private static XElement _tab;
        private static XElement _nl;
        private static XElement _sp;

        private static bool _isInit = false;

        /// <summary>
        /// Like this, INSTEAD OF a static constructor, to avoid any
        /// phasing / race condition with the initialization of the namespace manager.
        /// </summary>
        static void Init()
        {
            _tab = new XElement(XName.Get("tab", OdtNames.NamespaceManager.LookupNamespace("text")));
            _nl = new XElement(XName.Get("line-break", OdtNames.NamespaceManager.LookupNamespace("text")));
            _sp = new XElement(XName.Get("s", OdtNames.NamespaceManager.LookupNamespace("text")));
            _isInit = true;
        }

        public static object[] Prepare(string s)
        {
            if (!_isInit) Init();
            List<object> rvl = new List<object>();

            if (s == null)
                return rvl.ToArray();

            bool priorSpace = false;
            foreach (char c in s.ToCharArray())
            {
                switch (c)
                {
                    case '\n':
                        rvl.Add(new XElement(_nl));
                        priorSpace = false;
                        break;
                    case '\t':
                        rvl.Add(new XElement(_tab));
                        priorSpace = false;
                        break;
                    case ' ':
                        if (priorSpace)
                            rvl.Add(new XElement(_sp));
                        else
                            rvl.Add(' ');
                        priorSpace = true;
                        break;
                    default:
                        rvl.Add(c);
                        priorSpace = false;
                        break;
                }
            }

            return rvl.ToArray();
        }
    }
}