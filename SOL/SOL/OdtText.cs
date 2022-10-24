// OdtText.cs
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