using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpdInterfaceLib;


namespace Ged2Reg.Model
{
    public class AncestryNameList
    {
        class AncestorName
        {
            internal string GivenNames;
            internal string Surname;
        }

        private List<AncestorName> _ancestors = new List<AncestorName>();

        public AncestryNameList(GedcomIndividual ind)
        {
            _ancestors.Add(new AncestorName(){GivenNames = ind.SafeGivenName, Surname = ind.SafeSurname });
        }

        public AncestryNameList Descend(GedcomIndividual ind)
        {
            AncestryNameList rv = new AncestryNameList(ind);
            rv._ancestors.AddRange(_ancestors);
            return rv;
        }

        public void Emit(IWpdParagraph p, WpdInterfaceLib.Formatting fmtName, WpdInterfaceLib.Formatting fmtNumber)
        {
            p.Append(" (");
            bool furst = true;
            int gen = _ancestors.Count;
            foreach (AncestorName name in _ancestors)
            {
                if (!furst) p.Append(", ", false, fmtName);
                furst = false;
                p.Append(name.GivenNames, false, fmtName);
                p.Append($"{gen--}", false, fmtNumber);
            }
            p.Append(")");
        }
    }
}
