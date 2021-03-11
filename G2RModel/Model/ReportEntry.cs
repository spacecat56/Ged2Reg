using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Ged2Reg.Model;
using SimpleGedcomLib;

namespace G2RModel.Model
{
    public class ReportEntry : ReportEntryBase
    {
        public string OverrideId { get; set; }
        public string NaturalId => IndividualView?.Id;

        /// <summary>
        /// The "natural" Id is the GEDCOM id. But, we allow an
        /// override (set to recognize duplicates, when and where that
        /// is supported) and also a fallback (could the underlying
        /// instances ever be null? If so, we still won't throw).
        /// </summary>
        public string Id => OverrideId ?? NaturalId ?? InternalId;

        private BigInteger _assignedMainNumber;

        public BigInteger AssignedMainNumber
        {
            get => _assignedMainNumber;
            set => _assignedMainNumber = value;
        }

        public int AssignedChildNumber { get; set; }
        public AncestryNameList Ancestry { get; set; }

        /// <summary>
        /// Marks spouse of a focal-line person
        /// so s/he can be omitted if that is the option
        /// chosen in the settings.  This is easier than relying
        /// on an 'include' flag because if the 'continue' option
        /// is also chosen nothing additional needs to be done
        /// beyond putting them in the tree
        /// </summary>
        public bool OutOfFocus { get; set; }

 
        /// <summary>
        /// but we also need InFocus, which is NOT the same as
        /// "not OutOfFocus"... InFocus males with OOF wives
        /// need additional help to get the linked child emitted
        /// </summary>
        public bool InFocus
        {
            get => _inFocus;
            set => _inFocus = value;
        }
       private bool _inFocus;

        // these are used to fine-tune content 
        // on the Ancestors report

        /// <summary>
        /// Normally if the wife immediately follows the husband
        /// (i.e. both parents are known and listed) then
        /// we do not re-state the marriage details in the block for the
        /// wife; thus, SuppressSpouseInfo == true;
        /// HOWEVER, when the wife had additional marriages it is better to
        /// list them... thus, SuppressSpouseInfo == false
        /// </summary>
        public bool SuppressSpouseInfo { get; set; }

        /// <summary>
        /// Still, it would be tedious to fully recapitulate the details for the
        /// marriage in the immediately preceding block... by setting this
        /// property the details can be suppressed.
        /// </summary>
        public ReportEntry SpouseToMinimize { get; set; }

        private bool _emitChildrenAfter;

        /// <summary>
        /// Ancestry reports may have one or two parents listed in succeeding
        /// generations; this flag is used to cue when to emit the
        /// children, especially needed when only the male is listed
        /// </summary>
        public bool EmitChildrenAfter
        {
            get => _emitChildrenAfter;
            set => _emitChildrenAfter = value;
        }

        public bool DidInit { get; set; }

        public bool FamiliesAreSorted => Individual?.FamiliesAreSorted ?? false;

        public ListOfReportFamilyEntries FamilyEntries { get; set; }
        public static readonly ListOfReportFamilyEntries SafeEmptyFamilies = new ListOfReportFamilyEntries();
        public ListOfReportFamilyEntries SafeFamilies => FamilyEntries ?? SafeEmptyFamilies;
        public ReportEntry Spouse => FindSpouse();

        private ReportEntry FindSpouse()
        {
            if (MyFamily == null) return null;
            if (MyFamily.Husband?.Individual == Individual) return MyFamily.Wife;
            return MyFamily.Husband;
        }

        public ReportFamilyEntry MyFamily { get; set; }
        public ReportFamilyEntry ChildhoodFamily { get; set; }

        // this is used on ancestry report to implement
        // options and repositioning of list(s) of children
        public List<GedcomFamily> FamiliesToReport { get; set; }
        // this is used where we stop exploding to avoid repetition /
        // recursion, to reference the number of the repeated ancestor
        public List<ReportEntry> ContinuesWith { get; set; }

        public int Generation { get; set; }
        public GedcomIndividual Individual { get; set; }
        public IndividualView IndividualView => Individual?.IndividualView;
        public List<GedcomFamily> Families => Individual?.Families;
        public string ChildNumberRoman => AssignedChildNumber.ToRoman();

        private int? _numberOfChildren;
        private ReportFamilyEntry _linkedFamily;
        public int NumberOfChildren => _numberOfChildren ?? (_numberOfChildren = CountChildren()) ?? 0;
        public bool HasDescendants => NumberOfChildren > 0;
        public bool HasParents => ChildhoodFamily?.Husband != null || ChildhoodFamily?.Wife != null;

        public bool ChildEntryEmitted { get; set; }

        public bool IsRepeat => AssignedMainNumber > 0 &&
                                AssignedMainNumber > FirstAppearance;

        public BigInteger FirstAppearance => (Individual.FirstReportEntry?.AssignedMainNumber 
                                              ?? AssignedMainNumber);

        public bool IsFemale => Individual?.Gender == "F";

        public string GetNumber(bool withGeneration) => withGeneration
            ? $"{Generation:00}-{AssignedMainNumber}"
            : $"{AssignedMainNumber}";

        public ReportEntry(GedcomIndividual indi, ReportFamilyEntry thisFamily = null)
        {
            Individual = indi;
            MyFamily = thisFamily;
            //Init(indi); // NO this leads to unwanted recursion
        }

        public ReportEntry Init()
        {
            FamilyEntries = new ListOfReportFamilyEntries();
            Individual.FindFamilies(true); 
            foreach (GedcomFamily family in Individual.Families)
            {
                if (_linkedFamily?.Family == family)
                    // this is essential for correct ancestry structuring
                    FamilyEntries.Add(_linkedFamily); 
                else
                    FamilyEntries.Add(ReportEntryFactory.Instance.GetReportFamily(family));
            }

            if (Individual.ChildhoodFamily == null) return this;
            ChildhoodFamily = ReportEntryFactory.Instance.GetReportFamily(Individual.ChildhoodFamily);
            // we need to link across generations when doing multi-occurence
            // this does not accomplish the task ChildhoodFamily?.Link(this);

            // we're not going to REFUSE to run Init() again...
            // ... but we will let the caller find out if we already did it
            DidInit = true;

            return this;
        }

        public void SetContinuation(ReportEntry indi)
        {
            (ContinuesWith ??= new List<ReportEntry>()).Add(indi);
        }

        public string GetContinuation(bool withGeneration)
        {
            if ((ContinuesWith?.Count ?? 0) == 0) return null;

            StringBuilder sb = new StringBuilder();
            string sep = " ";
            sb.Append("(Continues with");
            foreach (ReportEntry re in ContinuesWith)
            {
                sb.Append(sep).Append(re.GetNumber(withGeneration));
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
                rv += family.Children?.Count ?? 0;
            }

            return rv;
        }

        public List<ReportFamilyEntry> FindMainNumberedFamilies()
        {
            List<ReportFamilyEntry> rvl = new List<ReportFamilyEntry>();
            if (!EmitChildrenAfter)  
                return rvl;

            foreach (ReportFamilyEntry familyEntry in FamilyEntries)
            {
                if (familyEntry.FindMainNumberedChildren().Count > 0)
                    rvl.Add(familyEntry);
            }

            return rvl;
        }
        public List<ReportEntry> FindMainNumberedChildren()
        {
            List<ReportEntry> rvl = new List<ReportEntry>();
            if (!EmitChildrenAfter)  //(FamilyEntries == null || (FamiliesToReport?.Count ?? 0) == 0)
                return rvl;

            foreach (ReportFamilyEntry familyEntry in FamilyEntries)
            {
                rvl.AddRange(familyEntry.FindMainNumberedChildren());
            }
            return rvl;
        }

        public void OverrideAsNotLiving()
        {
            Individual.PresumedDeceased = true;
        }

        public IEnumerable<ReportEntry> GatherVisiblePersons()
        {
            List<ReportEntry> rvl = new List<ReportEntry>();
            rvl.Add(this);
            if (ChildhoodFamily!=null)
                rvl.AddRange(ChildhoodFamily.GatherVisiblePersons());
            if (FamilyEntries == null)
                return rvl;
            foreach (ReportFamilyEntry familyEntry in FamilyEntries)
            {
                rvl.AddRange(familyEntry.GatherVisiblePersons());
                foreach (ReportEntry party in familyEntry.Couple)
                {
                    if (party?.Individual == null || party.Individual == Individual)
                        continue;
                    rvl.AddRange(ReportEntryFactory.Instance.GetParents(party.Individual));
                }
            }

            return rvl;
        }

        public void Link(ReportFamilyEntry linkedFam)
        {
            _linkedFamily = linkedFam;
        }

        public int MarriageNumberWith(ReportEntry re)
        {
            if (!FamiliesAreSorted) return 0;
            int ix = Individual.SpouseIndex(re.Individual);
            return ix;
        }

        public bool NameIsUnknown() => Individual?.NameIsUnknown ?? true;
    }

    public class ListOfReportEntry : List<ReportEntry>
    {
        public ListOfReportEntry() { }

        public ListOfReportEntry(List<ReportEntry> l)
        {
            if (l==null) return;
            AddRange(l);
        }

        public List<GedcomIndividual> ToListOfIndividuals()
        {
            List<GedcomIndividual> rvl = this.Select(re => re.Individual).ToList();
            return rvl;
        }
    }

    public class ListOfReportFamilyEntries : List<ReportFamilyEntry> { }
}
