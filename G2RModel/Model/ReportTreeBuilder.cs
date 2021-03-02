using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Ged2Reg.Model;

namespace G2RModel.Model
{
    public enum TreeMode
    {
        Ancestors,
        Descendants
    }
    public class ReportTreeBuilder
    {
        // properties of interest as results
        public ListOfReportEntry[] Generations { get; private set; }
        public ListOfReportEntry[] CompleteGenerations { get; private set; }
        public int LastSlotWithPeople { get; private set; }
        public List<GedcomIndividual> NonContinued { get; private set; }

        // configuration properties
        public bool AllFamilies { get; set; }
        public bool AllowMultiple { get; set; }
        public GedcomIndividual Root { get; set; }
        public int? GenerationsOverride { get; set; }

        public TreeMode Mode { get; set; }
        // optional; citations not processed if null
        public CitationsMap Cm { get; set; }

        // private fields
        private ReportContext _c;
        private ReportEntry _root;
        private HashSet<string> _treedPersons;

        public ReportTreeBuilder Init()
        {
            _c = ReportContext.Instance;

            // the root is generation 0
            // the array holds ordered lists of main persons by generation
            // the array index is the generation number
            // set up Generation 0 to initialize for recursion
            Generations = new ListOfReportEntry[GenerationsOverride ?? _c.Settings.Generations];
            Generations[0] = new ListOfReportEntry();
            _treedPersons = new HashSet<string>();
            NonContinued = new List<GedcomIndividual>();

            ReportEntryFactory.Init(!AllowMultiple);
            _root = ReportEntryFactory.Instance.GetReportEntry(Root);
            _root.AssignedMainNumber = 1;
            AddPersonToGeneration(_root, 0);

            return this;
        }

        public ReportTreeBuilder Exec()
        {
            switch (Mode)
            {
                case TreeMode.Ancestors:
                    ApplyAncestryNumbering();
                    break;
                case TreeMode.Descendants:
                    ApplyDescendantNumbering(0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return this;
        }

        public ReportTreeBuilder ApplyReduction(string id, bool extend)
        {
            void ApplyResults(int lastGenFound1, ListOfReportEntry[] listOfReportEntries)
            {
                for (int i = 1; i <= lastGenFound1; i++)
                    listOfReportEntries[i] =
                        new ListOfReportEntry(listOfReportEntries[i].OrderBy(re => re.AssignedMainNumber).ToList());
                CompleteGenerations = Generations;
                Generations = listOfReportEntries;
            }

            ListOfReportEntry[] ngs = new ListOfReportEntry[Generations.Length];
            int firstGenFound = 0;
            int lastGenFound = -1;
            for (int i = 0; i < Generations.Length; i++)
            {
                if (Generations[i] == null) break;
                ngs[i] = new ListOfReportEntry(Generations[i].FindAll(re => re.IndividualView?.Id == id));
                if (ngs[i].Count > 0)
                {
                    lastGenFound = i;
                    if (firstGenFound == 0)
                        firstGenFound = i;
                }
            }

            if (lastGenFound < 0) 
                throw new Exception("focus person not found in selected tree scope");

            // pick up the spouse of the focus person in each generation where found
            for (int i = firstGenFound; i <= lastGenFound; i++)
            {
                HashSet<BigInteger> wanted = new HashSet<BigInteger>();
                foreach (ReportEntry re in ngs[i])
                {
                    BigInteger spouseNbr = re.AssignedMainNumber;
                    spouseNbr += (spouseNbr % 2 == 1) ? -1 : 1;
                    if (!wanted.Contains(spouseNbr))
                        wanted.Add(spouseNbr);
                }
                ngs[i].AddRange(Generations[i].FindAll(ind => wanted.Contains(ind.AssignedMainNumber)));
            }

            // pick up the descendants of focus person etc. walking down the 
            // generations from the last one where s/he was initially found
            for (int i = lastGenFound; i >0; i--)
            {
                HashSet<BigInteger> wanted = new HashSet<BigInteger>();
                foreach (ReportEntry re in ngs[i])
                {
                    BigInteger childnbr = re.AssignedMainNumber / 2; // truncated 
                    if (!wanted.Contains(childnbr))
                        wanted.Add(childnbr);
                    BigInteger cSpouse = childnbr + ((childnbr % 2 == 1) ? -1 : 1);
                    if (!wanted.Contains(cSpouse))
                        wanted.Add(cSpouse);
                }
                //ngs[i-1] ??= new ListOfReportEntry();
                ngs[i-1].AddRange(Generations[i-1].FindAll(ind => wanted.Contains(ind.AssignedMainNumber)));
            }

            if (!extend)
            {
                ApplyResults(lastGenFound, ngs);
                return this;
            }

            // fill in the older generations that extend from 
            // the most distant occurrence of the focus person
            for (int i = lastGenFound; i+1 < ngs.Length; i++)
            {
                if ((ngs[i]?.Count??0) == 0) break;
                HashSet<BigInteger> wanted = new HashSet<BigInteger>();
                foreach (ReportEntry re in ngs[i])
                {
                    BigInteger father = re.AssignedMainNumber * 2;
                    if (!wanted.Contains(father))
                        wanted.Add(father);
                    BigInteger mother = father + 1;
                    if (!wanted.Contains(mother))
                        wanted.Add(mother);
                }
                ngs[i+1].AddRange(Generations[i+1].FindAll(ind => wanted.Contains(ind.AssignedMainNumber)));
            }

            ApplyResults(lastGenFound, ngs);
            return this;
        }

        public void ApplyAncestryNumbering()
        {
            int generation = 0;

            Generations[0][0].EmitChildrenAfter = AllFamilies;
            if ((Generations[0][0].Families?.Count ?? 0) > 0)
                NumberChildren(Generations[0][0].MyFamily, null);

            while (++generation < Generations.Length)
            {
                ListOfReportEntry ip = Generations[generation - 1];
                if (ip.Count == 0)
                {
                    LastSlotWithPeople = generation - 2;
                    return;
                }

                // advance to the next generation, starting with an empty list of 'main' persons
                ListOfReportEntry op = Generations[generation] = new ListOfReportEntry();

                foreach (ReportEntry re in ip)
                {
                    re.Generation = generation;
                    re.Init();
                    if (AllFamilies)
                    {
                        foreach (ReportFamilyEntry family in re.FamilyEntries)
                        {
                            family.Init();
                            family.IsIncluded = true;
                            NumberChildren(family, null);
                        }
                    }

                    if (re.Individual.ChildhoodFamily == null)
                        continue;


                    // NB NOTHING BELOW HERE unless it is about the 
                    //  "childhood family"!

                    re.ChildhoodFamily.IsIncluded = true;
                    NumberChildren(re.ChildhoodFamily, re);

                    // add the parents, if known and new 
                    GedcomIndividual dad = re.Individual.ChildhoodFamily.Husband;
                    GedcomIndividual mom = re.Individual.ChildhoodFamily.Wife;
                    ReportEntry de = null;
                    bool dadIncluded = false;
                    if (dad != null)
                    {
                        if (dad.FirstReportEntry != null && !AllowMultiple)
                        {
                            re.SetContinuation(dad.FirstReportEntry);
                        }
                        else
                        {
                            de = ReportEntryFactory.Instance.GetReportEntry(dad);
                            de.AssignedMainNumber = re.AssignedMainNumber * 2;
                            de.EmitChildrenAfter = true;

                            // when allowing multiple occurences, the factory 
                            // does not have enough information to provide the 
                            // right instance of the family, so, we push it here (and for mom, below)
                            de.Link(re.ChildhoodFamily);
                            dad.FirstReportEntry ??= de;
                            dad.FindFamilies(true);
                            op.Add(de);
                            dadIncluded = true;
                        }
                    }
                    if (mom != null)
                    {
                        if (mom.FirstReportEntry != null && !AllowMultiple)
                        {
                            re.SetContinuation(mom.FirstReportEntry);
                        }
                        else
                        {
                            ReportEntry me = ReportEntryFactory.Instance.GetReportEntry(mom);
                            me.AssignedMainNumber = re.AssignedMainNumber * 2 + 1;
                            me.EmitChildrenAfter = true;
                            me.Link(re.ChildhoodFamily);
                            mom.FirstReportEntry ??= me;
                            mom.FindFamilies(true);
                            if (dadIncluded) de.EmitChildrenAfter = false; // mom steals them, if she is known
                            me.SuppressSpouseInfo = dadIncluded;
                            op.Add(me);
                        }
                    }
                }
            }
        }

        private void NumberChildren(ReportFamilyEntry cf, ReportEntry linked)
        {
            if (cf == null) return;
            int greatestChildSeq = 0;
            if (cf.Children == null || AllowMultiple)
            {
                cf.Init(linked);
            }
            foreach (ReportEntry child in cf.Children)
            {
                if (child.AssignedChildNumber != 0) continue;
                child.AssignedChildNumber = ++greatestChildSeq;
                if (child.AssignedMainNumber == 0)
                    NonContinued.Add(child.Individual);
                if (Cm != null)
                    child.Individual.LocateCitations(Cm);
                child.Individual.Expand(); // pick up extra info for the ancestors report
            }
        }

        private void ApplyDescendantNumbering(int generation)
        {
            ListOfReportEntry ip = Generations[generation];
            if ((ip?.Count??0) == 0)
                return;
            if (generation >= Generations.Length - 1)
            {
                // well, this is awkward....
                // we need to put i. 's on the children of the last gen in the report
                foreach (ReportEntry mainIndividual in ip)
                {
                    // being in the list entails: indi has some child[ren]
                    int greatestChildSeq = 0;
                    foreach (ReportFamilyEntry family in mainIndividual.FamilyEntries)
                    {
                        foreach (ReportEntry child in family.Children)
                        {
                            if (child.AssignedChildNumber != 0) continue;
                            child.AssignedChildNumber = ++greatestChildSeq;
                            NonContinued.Add(child.Individual);
                        }
                    }
                }
                return;
            }

            // advance to the next generation, starting with an empty list of 'main' persons
            int nextGen = generation + 1;
            BigInteger greatestId = ip[^1].AssignedMainNumber;

            foreach (ReportEntry mainIndividual in ip)
            {
                AncestryNameList anl = mainIndividual.Ancestry?.Descend(mainIndividual.Individual) ??
                                       new AncestryNameList(mainIndividual.Individual);
                // being in the list entails: indi has some child[ren]
                int greatestChildSeq = 0;
                mainIndividual.Init();
                foreach (ReportFamilyEntry family in mainIndividual.FamilyEntries)
                {
                    family.Init();
                    foreach (ReportEntry child in family.Children)
                    {
                        child.Ancestry = anl;
                        if (Cm != null)
                            child.Individual.LocateCitations(Cm);
                        if (child.AssignedChildNumber == 0)
                            child.AssignedChildNumber = ++greatestChildSeq;
                        if (child.Individual.NumberOfChildren == 0)
                        {
                            NonContinued.Add(child.Individual);
                            continue;
                        }
                        if (child.AssignedMainNumber == 0)
                            child.AssignedMainNumber = ++greatestId;
                        //op.Add(child);
                        AddPersonToGeneration(child, nextGen);
                    }
                }
            }
            ApplyDescendantNumbering(nextGen);
        }
        private void AddPersonToGeneration(ReportEntry p, int ix)
        {
            if (_treedPersons.Contains(p.IndividualView.Id))
            {
                if (!_c.Settings.AllowMultipleAppearances)
                {
                    Debug.WriteLine($"Duplicate person, not added: {p.IndividualView.Id}");
                    return;
                }
            }
            else
            {
                _treedPersons.Add(p.IndividualView.Id);
            }
            Generations[ix] ??= new ListOfReportEntry();
            Generations[ix].Add(p);
        }

    }
}
