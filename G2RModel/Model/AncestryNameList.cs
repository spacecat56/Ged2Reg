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

        private List<GedcomIndividual> _gedcomIndividuals = new List<GedcomIndividual>();
        private List<AncestorName> _ancestors = new List<AncestorName>();

        public AncestryNameList(GedcomIndividual ind)
        {
            //_ancestors.Add(new AncestorName(){GivenNames = ind.SafeGivenName, Surname = ind.SafeSurname });
            // we need to defer construction of the list until
            // after the overall machinery has had time to figure out who is, or my be, still living
            // otherwise we are grabbing the name too early 
            _gedcomIndividuals.Add(ind);
        }

        public AncestryNameList Descend(GedcomIndividual ind)
        {
            AncestryNameList rv = new AncestryNameList(ind);
            rv._gedcomIndividuals.AddRange(_gedcomIndividuals);
            return rv;
        }

        public void Emit(IWpdParagraph p, Formatting fmtName, Formatting fmtNumber)
        {
            if (_gedcomIndividuals.Count > _ancestors.Count)
                Build();

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

        private void Build()
        {
            _ancestors.Clear();
            foreach (GedcomIndividual ind in _gedcomIndividuals)
            {
                _ancestors.Add(new AncestorName(){ GivenNames = ind.SafeGivenName, Surname = ind.SafeSurname });
            }
        }
    }
}
