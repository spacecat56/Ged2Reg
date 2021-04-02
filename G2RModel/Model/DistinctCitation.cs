using System;
using System.Collections.Generic;
using System.Linq;
using DocxAdapterLib;
using SimpleGedcomLib;
using WpdInterfaceLib;

namespace Ged2Reg.Model
{
    public class DistinctCitation
    {
        public List<CitationView> CitationViews { get; set; } = new List<CitationView>();
        public static List<DistinctCitation> DistinctCitations { get; private set; }
        public static List<DistinctCitation> BuildList(List<CitationView> cvs)
        {
            DistinctCitations = new List<DistinctCitation>();

            List<CitationView> workList = (from cv in cvs orderby cv.SourceId ascending, cv.FullText ascending select cv).ToList();

            DistinctCitation current = new DistinctCitation();
            foreach (CitationView citationView in workList)
            {
                if (current.AppendIfMatch(citationView))
                    continue;
                DistinctCitations.Add(current = new DistinctCitation(citationView));
            }

            return DistinctCitations;
        }


        public string SourceId { get; set; }
        public string FullText { get; set; }
        public string Content { get; set; }
        //public string ShortText { get; set; }
        public int ReferenceCount { get; set; }
        public int SelectedCount { get; set; }
        public int PotentialAddedCoverage => ReferenceCount - SelectedCount;
        public bool IsEmitted { get; set; }
        //public int FirstUse { get; set; }

        private WpdFootnoteBase _firstFootnote;

        public WpdFootnoteBase FirstFootnote
        {
            get => _firstFootnote;
            set
            {
                if (_firstFootnote != null && value != _firstFootnote)
                    throw new InvalidOperationException("First Footnote is immutable!");
                _firstFootnote = value;
            }
        }

        public string Title { get; private set; }
        public int Priority { get; set; }

        public int AntiPriority => Priority < 0 ? Priority : 0;

        internal DistinctCitation() { }

        public DistinctCitation(CitationView cv)
        {
            SourceId = cv.SourceId;
            FullText = cv.FullText;
            Content = cv.SourceTag?.Content;
            Title = cv.TheSourceView?.Title;
            CitationViews.Add(cv);
        }

        public bool AppendIfMatch(CitationView cv)
        {
            if (cv.SourceId == null && cv.SourceId == SourceId)
            {
                // citation without source... FullText is not a good compare, 
                // we need to look at the Content property
                if (cv.SourceTag.Content != Content)
                    return false;
            }
            else if (cv.SourceId != SourceId || cv.FullText != FullText)
            {
                return false;
            }
            CitationViews.Add(cv);
            return true;
        }

        //public int SelectEverywhere(bool force = false)
        //{
        //    foreach (var VARIABLE in CitationViews)
        //    {
                
        //    }
        //}
        public void PruneToList(HashSet<Tag> allSourcedTags)
        {
            List<CitationView> survivors =
                (from cv in CitationViews where allSourcedTags.Contains(cv.GetEvent()) select cv).ToList();
            CitationViews.Clear();
            CitationViews.AddRange(survivors);
        }

        public void Reset()
        {
            IsEmitted = false;
            _firstFootnote = null;
        }
    }
}
