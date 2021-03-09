using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonClassesLib;
using G2RModel.Model;
using SimpleGedcomLib;

namespace Ged2Reg.Model
{
    /// <summary>
    /// Applies any policy on how to format the citations
    /// Keeps track of the number of uses of the same citation
    /// Keeps track of the first use
    /// Provides the "see note" alternative
    /// 
    /// </summary>
    public class CitationCoordinator
    {
        public static bool DeferConsecutiveRepeats { get; set; }
        public int Deferred { get; set; }

        //private List<CitationUsage> _usages = new List<CitationUsage>();

        //private Dictionary<string, Dictionary<string, CitationUsage>> SourceUsagesMap
        //    = new Dictionary<string, Dictionary<string, CitationUsage>>();

        private List<DistinctCitation> _distinctCitations;
        private Dictionary<Tag, DistinctCitation> _distinctCitationsMap = new Dictionary<Tag, DistinctCitation>();
        private List<CitableEntityEvents> _allEntityEventCitations = new List<CitableEntityEvents>();
        private List<TagCode> _allCodes;
        private List<PriorityEvaluator> _priorityEvaluators;
        private int[] _priorityCensus;
        //private List<PriorityEvaluator> _anitpriorityEvaluators;
        private int[] _anitpriorityCensus;

        //public CitationStrategy Strategy { get; set; } = CitationStrategy.MostOftenUsed;
        public BaptismOptions BaptismOption { get; set; } = BaptismOptions.WhenNoBirth;
        public bool IncludeBurial { get; set; }

        public bool ReferToFirstUse { get; set; }
        public bool SummarizeOtherCitations { get; set; }
        public int MaxToEmitPerEvent { get; set; }
        public string Priorities { get; set; }
        public string AnitPriorities { get; set; }

        public List<TagCode> PersonEventsToCite { get; set; } = new List<TagCode>()
        {
            TagCode.BIRT,
            TagCode.DEAT,
            TagCode.BAPM,
            TagCode.CHR,
            TagCode.BURI
        };

        public List<TagCode> FamilyEventsToCite { get; set; } = new List<TagCode>()
        {
            TagCode.MARR,
            TagCode.DIV,
        };


        public List<CitationCensusEntry> InitialCitationCensus { get; private set; } = new List<CitationCensusEntry>();
        public List<CitationCensusEntry> PrunedCitationCensus { get; private set; } = new List<CitationCensusEntry>();
        public List<CitationCensusEntry> FinalCitationCensus { get; private set; } = new List<CitationCensusEntry>();

        public CitationCoordinator(List<CitationView> allCitationViews)
        {
            Deferred = 0;
            _allCodes = new List<TagCode>(PersonEventsToCite);
            _allCodes.AddRange(FamilyEventsToCite);

            _distinctCitations = DistinctCitation.BuildList(allCitationViews);
            _distinctCitationsMap = new Dictionary<Tag, DistinctCitation>();
            foreach (DistinctCitation uc in _distinctCitations)
            {
                foreach (CitationView cv in uc.CitationViews)
                {
                    _distinctCitationsMap.Add(cv.SourceTag, uc);
                }
            }
        }

        public int Process(CitationStrategy strategy, CitationStrategy? secondary)
        {
            // todo: apply policies to select the winners among citations
            // curious: how many events to be cited?
            int citedEvents = 0;
            foreach (CitableEntityEvents eec in _allEntityEventCitations)
            {
                citedEvents += eec.CitationsByEvent?.Count ?? 0;
            }

            // take a 'before' census 
            InitialCitationCensus = TakeCensus();

            // reduce the reference lists within each distinct citation
            // to only the events that in the scope of the report
            // so that a strategy based on counting uses can work 
            // 1. make a list of all Tag instances in all CitableEntityEvents
            HashSet<Tag> allSourcedTags = new HashSet<Tag>();
            foreach (CitableEntityEvents eec in _allEntityEventCitations)
            {
                foreach (Tag tag in eec.CitationsByEvent.Keys)
                {
                    allSourcedTags.Add(tag);
                }
            }
            // 2. pass to each DistinctCitation to remove all CitationViews whose
            //  event tag is not in the list
            foreach (DistinctCitation dc in _distinctCitations)
            {
                dc.PruneToList(allSourcedTags);
            }

            // take an 'after' census 
            PrunedCitationCensus = TakeCensus();

            // begin selection of citations to include in the report
            // first, select all citations that are the only one for the event
            int uniq = 0;
            foreach (CitableEntityEvents eec in _allEntityEventCitations)
            {
                uniq += eec.SelectAnySoleCitations();
            }

            FinalCitationCensus = TakeCensus();

            switch (strategy)
            {
                case CitationStrategy.MostOftenUsed:
                    SelectByFrequencyOfUse();
                    break;
                case CitationStrategy.LeastOftenUsed:
                    SelectByFrequencyOfUse(true);
                    break;
                case CitationStrategy.PriorityDriven:
                    SelectByPriority();
                    if (secondary != null && secondary.Value != CitationStrategy.None)
                        Process(secondary.Value, null);
                    break;
                case CitationStrategy.None:
                    break;
                default:
                    throw new NotImplementedException($"Strategy {strategy} not implemented");
            }

            FinalCitationCensus = TakeCensus();

            return citedEvents;
        }

        public int Reset()
        {
            int rv = 0;

            foreach (DistinctCitation dc in _distinctCitations)
            {
                if (!dc.IsEmitted) continue;
                rv++;
                dc.Reset();
            }

            return rv;
        }

        private void SelectByPriority()
        {
            _priorityEvaluators = new List<PriorityEvaluator>();

            // negative priorities need to be evaluated first
            string[] antipatterns = AnitPriorities.Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int maxPri = -1;
            foreach (string anitpattern in antipatterns)
            {
                _priorityEvaluators.Add(new PriorityEvaluator(anitpattern) { Priority = maxPri-- });
            }

            string[] patterns = Priorities.Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            maxPri = patterns.Length;
            foreach (string pattern in patterns)
            {
                _priorityEvaluators.Add(new PriorityEvaluator(pattern) { Priority = maxPri-- });
            }

            _priorityCensus = new int[_priorityEvaluators.Count + 1];
            _priorityCensus[0] = _distinctCitations.Count;

            foreach (DistinctCitation dc in _distinctCitations)
            {
                for (int i = 0; i < _priorityEvaluators.Count; i++)
                {
                    PriorityEvaluator evaluator = _priorityEvaluators[i];
                   if (!evaluator.Evaluate(dc)) continue;
                    _priorityCensus[0]--;
                    _priorityCensus[i+1]++;
                    break;
                }
            }

            // this seems ill-advised:
            if (_priorityCensus[0] == _distinctCitations.Count)
            {
                throw new Exception("No match for any source priority pattern");
            }
            // work from example?
            // var test2NotInTest1 = test2.Where(t2 => !test1.Any(t1 => t2.Contains(t1)));

            // we're using a naive, nxm (n^2) algorithm; at least we can reduce the lists to ones that will hit somewhere
            List<CitableEntityEvents> candidates = _allEntityEventCitations.Where(eec =>
                eec.CitationsByEvent.Values.Any(ec => ec.DistinctCitations.Any(dc => dc.Priority > 0))).ToList();
            List<DistinctCitation> winners = _distinctCitations.Where(dc => dc.Priority > 0)
                .OrderByDescending(dc => dc.Priority).ToList();

            int successes = 0;
            foreach (CitableEntityEvents eec in candidates)
            {
                foreach (DistinctCitation winner in winners)
                {
                    int hits = eec.Select(winner);
                    if (hits == 0)
                        continue;
                    successes += hits;
                }
            }

        }

        private List<CitationCensusEntry> TakeCensus()
        {
            List<CitationCensusEntry> rvl = new List<CitationCensusEntry>();
            foreach (TagCode code in _allCodes)
            {
                CitationCensusEntry cce = new CitationCensusEntry() {TagCode = code};
                rvl.Add(cce);
                foreach (CitableEntityEvents eec in _allEntityEventCitations)
                {
                    eec.Census(cce);
                }
            }

            return rvl;
        }

        private void SelectByFrequencyOfUse(bool least = false)
        {
            int undecided = _allEntityEventCitations.Sum(eec => eec.Undecided);

            // even for frequency of use, we need to "push down" the ones with negative priority
            // otherwise, negative priorities don't really work
            List<DistinctCitation>[] withPotentials =
            {
                (from dc in _distinctCitations where dc.Priority >= 0 && dc.PotentialAddedCoverage > 0 select dc).ToList(),
                (from dc in _distinctCitations where dc.Priority < 0 && dc.PotentialAddedCoverage > 0 select dc).ToList(),
            };

            List<CitableEntityEvents> wantingCitations = _allEntityEventCitations.Where(e => e.Undecided > 0).ToList();

            foreach (List<DistinctCitation> withPotential in withPotentials)
            {
                if (ReportContext.Instance.Model.CheckCancel()) throw new CanceledByUserException();
                //int priorPriority = int.MaxValue;
                int decisionsMade = 0;
                //while (withPotential.Count > 0 && (undecided = _allEntityEventCitations.Sum(eec => eec.Undecided)) > 0)
                while (withPotential.Count > 0 && (undecided > 0))
                {
                    if (ReportContext.Instance.Model.CheckCancel()) throw new CanceledByUserException();
                    // the best next choice may(?) change with each iteration as some get filled in 
                    // although, the structure in place is probably not really achieving that awareness...
                    DistinctCitation choice = (least) 
                        ? withPotential.OrderBy(dc => dc.PotentialAddedCoverage).FirstOrDefault()
                        : withPotential.OrderByDescending(dc => dc.PotentialAddedCoverage).FirstOrDefault();
                    withPotential.Remove(choice);
                    //Debug.WriteLine($"DistinctCitation choice priority {choice?.Priority}");
                    //if (priorPriority < 0 && choice.Priority > priorPriority)
                    //    Debug.WriteLine("****Priorities going backwards!");
                    //priorPriority = choice.Priority;
                    foreach (CitableEntityEvents e in wantingCitations)
                    {
                        int dNow = e.Select(choice);
                        undecided -= dNow;
                        decisionsMade += dNow;
                    }

                    if (decisionsMade >= 100)
                    {
                        decisionsMade = 0;
                        wantingCitations = _allEntityEventCitations.Where(e => e.Undecided > 0).ToList();
                    }
                }
            }
        }

        public void PreProcess(List<GedcomIndividual> individuals)
        {
            // attach the citation sets to each individual and family instance 
            // that is in the scope of the current report
            foreach (GedcomIndividual individual in individuals)
            {
                PreProcess(individual);
            }
        }

        public void PreProcess(GedcomIndividual indi)
        {
            PreProcessIndividual(indi);

            foreach (GedcomFamily family in indi.SafeFamilies)
            {
                family.CitableEvents = new CitableMarriageEvents() { Family = family };
                foreach (TagCode code in FamilyEventsToCite)
                {
                    Tag t = family.FamilyView.FamTag.GetChild(code);
                    if (t==null) continue;
                    foreach (Tag sourc in t.GetChildren(TagCode.SOUR))
                    {
                        family.CitableEvents.Cite(t, _distinctCitationsMap[sourc]);
                    }
                }
                if (!family.CitableEvents.IsEmpty)
                    _allEntityEventCitations.Add(family.CitableEvents);

                GedcomIndividual spouse = family.Husband == indi ? family.Wife : family.Husband;
                PreProcessIndividual(spouse);
            }
        }

        private void PreProcessIndividual(GedcomIndividual indi)
        {
            if (indi == null) 
                return;

            indi.CitableEvents = new CitablePersonEvents() {Individual = indi};

            bool hasBirth = false;
            foreach (TagCode tagCode in PersonEventsToCite)
            {
                Tag t = indi.IndividualView.IndiTag.GetChild(tagCode);
                if (t == null) continue;
                switch (tagCode)
                {
                    case TagCode.BIRT:
                        hasBirth = true;
                        break;
                    case TagCode.CHR when BaptismOption == BaptismOptions.None:
                    case TagCode.CHR when BaptismOption == BaptismOptions.WhenNoBirth && hasBirth:
                    case TagCode.BAPM when BaptismOption == BaptismOptions.None:
                    case TagCode.BAPM when BaptismOption == BaptismOptions.WhenNoBirth && hasBirth:
                    case TagCode.BURI when !IncludeBurial:
                        continue;
                }

                foreach (Tag sourc in t.GetChildren(TagCode.SOUR))
                {
                    indi.CitableEvents.Cite(t, _distinctCitationsMap[sourc]);
                }
            }

            if (!indi.CitableEvents.IsEmpty)
                _allEntityEventCitations.Add(indi.CitableEvents);
        }

        public static LocalCitationCoordinator Optimize(LocalCitationCoordinator props)
        {
            LocalCitationCoordinator rvl = new LocalCitationCoordinator()
                {DoCite = props.DoCite};
            LocalCitationCoordinator coordinator = new LocalCitationCoordinator() 
                { DoCite = props.DoCite };
            
            // get rid of nulls.  be SURE to preserve the ordering!
            foreach (CitationProposal prop in props)
            {
                if (prop == null) continue;
                coordinator.Add(prop);
            }

            // if there's only one, or the global policy is not to defer (combine)
            // or the local policy has an override not to combine, then we are done
            if (coordinator.Count < 2 
                || (props.OverrideDeferRepeats == null && !DeferConsecutiveRepeats)
                || (props.OverrideDeferRepeats.HasValue && props.OverrideDeferRepeats.Value))
                return coordinator;

            // find and combine consecutive repeats of the same citation
            CitationProposal candidateProposal = coordinator[0];
            for (int i = 1; i < coordinator.Count; i++)
            {
                if (coordinator[i].Matches(candidateProposal))
                { // push the name of the vanishing event(s) to the survivor 
                    coordinator[i].AddApplicableEvents(candidateProposal);
                    candidateProposal = coordinator[i];
                    continue;
                }
                rvl.Add(candidateProposal);
                candidateProposal = coordinator[i];
            }
            rvl.Add(candidateProposal);

            return rvl;
        }
    }


    public enum CitationStrategy
    {
        None,
        MostOftenUsed,
        //FewestPerPerson,
        LeastOftenUsed,
        PriorityDriven,
    }

    public class CitationStrategyChoice : NamedValue<CitationStrategy> { }

    public class CitationStrategyChoices : SortableBindingList<CitationStrategyChoice>
    {
        public CitationStrategyChoices()
        {
            foreach (CitationStrategy cs in Enum.GetValues(typeof(CitationStrategy)))
            {
                Add(new CitationStrategyChoice(){Name = cs.ToString(), Value = cs});
            }
        }
    }

    public enum BaptismOptions
    {
        None, 
        Always,
        WhenNoBirth
    }
    public class BaptismOptionChoice : NamedValue<BaptismOptions> { }
    public class BaptismOptionChoices : SortableBindingList<BaptismOptionChoice>
    {
        public BaptismOptionChoices()
        {
            foreach (BaptismOptions bo in Enum.GetValues(typeof(BaptismOptions)))
            {
                Add(new BaptismOptionChoice() { Name = bo.ToString(), Value  = bo });
            }
        }
    }

    public class PriorityEvaluator
    {
        public Regex Regex { get; }
        public int Priority { get; set; }
        public PriorityEvaluator(string pattern)
        {
            Regex = new Regex($"(?i)({pattern})");
        }

        public bool Evaluate(DistinctCitation dc)
        {
            //if (dc.FullText.Contains("Member Tree") || dc.Title.Contains("Member Tree"))
            //    System.Diagnostics.Debug.WriteLine($"got one: {dc.Title}");
            if (!Regex.IsMatch(dc.FullText) && (string.IsNullOrEmpty(dc.Title) || !Regex.IsMatch(dc.Title))) 
                return false;
            dc.Priority = Priority;
            return true;
        }

    }

    public static class GedExtensions
    {
        public static Tag GetEvent(this CitationView cv)
        {
            return cv.SourceTag?.ParentTag;
        }
    }
}
