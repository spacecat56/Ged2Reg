using System.Collections.Generic;
using G2RModel.Model;
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

        public void Emit(IWpdParagraph p, Formatting fmtName, Formatting fmtNumber)
        {
            GenerationNumberMapper gm = GenerationNumberMapper.Instance;
            p.Append(" (");
            bool furst = true;
            int gen = _ancestors.Count;
            foreach (AncestorName name in _ancestors)
            {
                if (!furst) p.Append(", ", false, fmtName);
                furst = false;
                p.Append(name.GivenNames, false, fmtName);
                p.Append($"{gm.GenerationNumberFor(gen--)}", false, fmtNumber);
            }
            p.Append(")");
        }
    }
}
