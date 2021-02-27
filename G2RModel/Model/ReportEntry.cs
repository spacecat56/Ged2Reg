﻿using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Ged2Reg.Model;
using SimpleGedcomLib;

namespace G2RModel.Model
{
    public class ReportEntry
    {
        public BigInteger AssignedMainNumber { get; set; }
        public int AssignedChildNumber { get; set; }
        public AncestryNameList Ancestry { get; set; }

        // these are used to control output positioning 
        // on the Ancestors report
        public bool SuppressSpouseInfo { get; set; }
        public bool EmitChildrenAfter { get; set; }
        public bool FamiliesAreSorted => Individual?.FamiliesAreSorted ?? false;

        public ListOfReportFamilyEntries FamilyEntries { get; set; }
        public static readonly ListOfReportFamilyEntries SafeEmptyFamilies = new ListOfReportFamilyEntries();
        public ListOfReportFamilyEntries SafeFamilies => FamilyEntries ?? SafeEmptyFamilies;
        public ReportEntry Spouse => FindSpouse();

        private ReportEntry FindSpouse()
        {
            if (MyFamily == null) return null;
            if (MyFamily.Husband == this) return MyFamily.Wife;
            return MyFamily.Husband;
        }

        public ReportFamilyEntry MyFamily { get; set; }
        public ReportFamilyEntry ChildhoodFamily { get; set; }

        // this is used on ancestry report to implement
        // options and repositioning of list(s) of children
        public List<GedcomFamily> FamiliesToReport { get; set; }
        // this is used where we stop exploding to avoid repetition /
        // recursion, to reference the number of the repeated ancestor
        public List<GedcomIndividual> ContinuesWith { get; set; }

        public int Generation { get; set; }
        public GedcomIndividual Individual { get; set; }
        public IndividualView IndividualView => Individual?.IndividualView;
        public List<GedcomFamily> Families => Individual?.Families;
        public string ChildNumberRoman => AssignedChildNumber.ToRoman();

        private int? _numberOfChildren;
        public int NumberOfChildren => _numberOfChildren ?? (_numberOfChildren = CountChildren()) ?? 0;
        public bool HasDescendants => NumberOfChildren > 0;
        public bool HasParents => ChildhoodFamily?.Husband != null || ChildhoodFamily?.Wife != null;

        public bool ChildEntryEmitted { get; set; }

        public string GetNumber(bool withGeneration) => withGeneration
            ? $"{Generation:00}-{AssignedMainNumber}"
            : $"{AssignedMainNumber}";

        private ReportEntry() { }

        public ReportEntry(GedcomIndividual indi, ReportFamilyEntry thisFamily = null)
        {
            Individual = indi;
            MyFamily = thisFamily; // todo: avoid duplication 
            //Init(indi);
        }

        public ReportEntry Init()
        {
            FamilyEntries = new ListOfReportFamilyEntries();
            Individual.FindFamilies(true); // todo: true?
            foreach (GedcomFamily family in Individual.Families)
            {
                FamilyEntries.Add(ReportEntryFactory.Instance.GetReportFamily(family));
            }

            if (Individual.ChildhoodFamily == null) return this;
            ChildhoodFamily = ReportEntryFactory.Instance.GetReportFamily(Individual.ChildhoodFamily);
            return this;
        }

        public void SetContinuation(GedcomIndividual indi)
        {
            (ContinuesWith ??= new List<GedcomIndividual>()).Add(indi);
        }

        public string GetContinuation(bool withGeneration)
        {
            if ((ContinuesWith?.Count ?? 0) == 0) return null;

            StringBuilder sb = new StringBuilder();
            string sep = " ";
            sb.Append("(Continues with");
            foreach (GedcomIndividual indi in ContinuesWith)
            {
                sb.Append(sep).Append(GetNumber(withGeneration));
                sep = ", ";
            }

            sb.Append(".)");

            return sb.ToString();
        }
        private int? CountChildren()
        {
            int rv = 0;
            foreach (ReportFamilyEntry family in SafeFamilies)
            {
                rv += family.Children.Count;
            }

            return rv;
        }

        public void SortFamilies()
        {
            // todo: needed?  hopeless? complicated?
        }
    }

    public class ListOfReportEntry : List<ReportEntry>
    {
        public List<GedcomIndividual> ToListOfIndividuals()
        {
            List<GedcomIndividual> rvl = this.Select(re => re.Individual).ToList();
            return rvl;
        }
    }

    public class ListOfReportFamilyEntries : List<ReportFamilyEntry> { }
}