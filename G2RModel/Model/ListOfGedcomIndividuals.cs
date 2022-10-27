using System.Collections.Generic;
using CommonClassesLib;
using SimpleGedcomLib;

namespace Ged2Reg.Model
{
    public class ListOfGedcomIndividuals : SortableBindingList<GedcomIndividual>
    {
        public ListOfGedcomIndividuals()
        {
        }

        public ListOfGedcomIndividuals(List<IndividualView> views)
        {
            foreach (IndividualView view in views)
            {
                Add(new GedcomIndividual(){IndividualView = view});
            }
        }

        public int Locate(string c, int ix)
        {
            for (int i = ix; i < Count; i++)
            {
                if ((Items[i].Surname ?? "`").StartsWith(c)) return i;
            }

            return 0;
        }
    }
}