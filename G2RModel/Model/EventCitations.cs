using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OoXmlWranglerLib;
using SimpleGedcomLib;
using WpdInterfaceLib;

//using Novacode.Src;

namespace Ged2Reg.Model
{
    public class EventCitations
    {
        public TagCode TagCode => this.Tag?.Code ?? TagCode.UNK;
        public DistinctCitation SelectedItem { get; set; }
        public List<DistinctCitation> DistinctCitations { get; set; } = new List<DistinctCitation>();
        public Tag Tag { get; set; }
        internal string OtherKnownSources;

        public EventCitations() { }

        public EventCitations Append(DistinctCitation uc)
        {
            DistinctCitations.Add(uc);
            uc.ReferenceCount++;
            return this;
        }

        public bool Select(DistinctCitation dc, bool force = false)
        {
            if (SelectedItem != null && !force) 
                return false;
            if (!DistinctCitations.Contains(dc)) 
                return false;
            SelectedItem = dc;
            SelectedItem.SelectedCount++;
            return true;
        }

        /// <summary>
        /// Consider the current settings, and build the footnote accordingly,
        /// adding it to the given doc and appending a reference inline in the
        /// given paragraph
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool EmitNote(IWpdDocument doc, IWpdParagraph p)
        {
            if (SelectedItem == null)
                return false;

            G2RSettings settings = ReportContext.Instance.Settings; // for lexical convenience

            if (settings.RepeatNoteRefInline && SelectedItem.IsEmitted && SelectedItem.FirstFootnote != null)
            {
                WpdNoteRefField nrf = doc.BuildNoteRef();
                nrf.MarkName = SelectedItem.FirstFootnote.BookmarkName;
                nrf.ContentText = settings.Brackets
                    ? $"[{SelectedItem.FirstFootnote.Id}]"
                    : $"{SelectedItem.FirstFootnote.Id}";
                nrf.InsertHyperlink = true;
                //    new OoxNoteRefField(doc, null)
                //{
                //    // CharStyle = "FootnoteReference",
                //    MarkName = SelectedItem.FirstFootnote.BookmarkName, 
                //    ContentText = settings.Brackets ? $"[{SelectedItem.FirstFootnote.Id}]" : $"{SelectedItem.FirstFootnote.Id}", 
                //    InsertHyperlink = true
                //};
                p.AppendField(nrf.Build());
                p.StyleName = ("FootnoteReference");
                return true;
            }

            if (settings.UseSeeNote && SelectedItem.IsEmitted && SelectedItem.FirstFootnote != null)
            {
                CitationResult cr = settings.CitationShortFormatter.Apply(SelectedItem.CitationViews[0]); // no URL in this format
                string noteText = ReportContext.Instance.Settings.DebuggingOutput
                    ? $"{cr.SourceId} {cr.Text}"
                    : cr.Text;
                //OoxFootnote laterNote = settings.AsEndnotes
                //    ? new OoxEndnote(doc, noteText, settings.BracketArray)
                //    : new OoxFootnote(doc, noteText, settings.BracketArray);
                WpdFootnoteBase laterNote = settings.AsEndnotes 
                    ? doc.BuildEndNote(noteText, settings.BracketArray) 
                    : doc.BuildFootNote(noteText, settings.BracketArray);
                laterNote.AppendText("(see note ").AppendNoteRef(SelectedItem.FirstFootnote).AppendText(").");
                laterNote.Apply(p);
                return true;
            }

            //OoxFootnote f = settings.AsEndnotes
            //    ? new OoxEndnote(doc, brackets: settings.BracketArray)
            //    : new OoxFootnote(doc, brackets: settings.BracketArray);
            WpdFootnoteBase f = settings.AsEndnotes
                ? doc.BuildEndNote(brackets: settings.BracketArray)
                : doc.BuildFootNote(brackets: settings.BracketArray);
            CitationResult c = BestCitation(settings.SummarizeAdditionalCitations, settings.CitationFormatter);
            if (ReportContext.Instance.Settings.DebuggingOutput)
            {
                f.AppendText($" [{c.SourceId}]");
            }
            foreach (CitationResultPiece piece in c.Pieces)
            {
                switch (piece.PieceType)
                {
                    case PieceType.Text:
                        f.AppendText(piece.Text);
                        break;
                    case PieceType.Hyperlink:
                        try
                        {
                            f.AppendHyperlink(piece.Text);
                        }
                        catch (System.UriFormatException ufx)
                        {
                            f.AppendText($"[{piece.Text}]");
                            System.Diagnostics.Debug.WriteLine($"Emit invalid URI as plain text:{piece.Text} in {c.SourceId}");
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            f.Apply(p, settings.BookMarkNotes && SelectedItem.SelectedCount > 1); // bookmark not needed if this is used only once
            // this function is too expensive: 25% of total runtime to get MaxFootnoteId
            //if (doc.MaxFootnoteId() != f.Id)
            //{
            //    throw new Exception("Internal error: footnote not applied");
            //}
            SelectedItem.IsEmitted = true;
            SelectedItem.FirstFootnote = SelectedItem.FirstFootnote ?? f;

            return true;
        }

        public CitationResult BestCitation(bool appendSummary, CitationFormatter citationFormatter)
        {
            CitationView forExample = SelectedItem?.CitationViews[0];
            if (forExample == null) return null;

            CitationResult cr = citationFormatter.Apply(SelectedItem.CitationViews?[0]);
            //string content = $"{cr.Text}{cr.URL}"; // todo: library and this app to support hyperlinks within footnotes.
            string extra = null;
            if (appendSummary && DistinctCitations.Count > 1)
            {
                int extraLimit = ReportContext.Instance.Settings.NumberCitesToSumamrize;
                if (extraLimit == 0)
                {
                    extra = OtherKnownSources = OtherKnownSources ?? $" (and {DistinctCitations.Count-1} additional {(DistinctCitations.Count > 2?"sources":"source")})";
                }
                else if (!string.IsNullOrEmpty(OtherKnownSources ?? SummarizeOtherSources(extraLimit)))
                {
                    extra = $" (Other sources include {OtherKnownSources}.)";
                }
                cr.AppendText(extra);
            }

            //return $"{content}{extra}";
            return cr;
        }

        private string SummarizeOtherSources(int extraLimit)
        {
            List<DistinctCitation> others = (from dc in DistinctCitations where dc != SelectedItem select dc).OrderByDescending(dc => dc.Priority).ToList();
            if (others.Count == 0)
                return null;
            if (extraLimit == 0)
                return OtherKnownSources = $"{others.Count} more";
            StringBuilder sb = new StringBuilder();
            string connector = null;
            HashSet<string> deDuplicator = new HashSet<string>();
            deDuplicator.Add(TextCleaner.TitleCleaner.Exec(SelectedItem.Title, TextCleanerContext.OthersList));        // todo: use a formatter for this text
            for (int i = 0, j = 0; i < others.Count && j < extraLimit; i++)
            {
                string maybeDifferent = TextCleaner.TitleCleaner.Exec(others[i].Title, TextCleanerContext.OthersList); // todo: use a formatter for this text
                if (deDuplicator.Contains(maybeDifferent))
                    continue;
                deDuplicator.Add(maybeDifferent);
                j++;
                sb.Append(connector).Append(maybeDifferent); 
                connector = connector ?? "; ";
            }

            if (sb.Length == 0)
                return OtherKnownSources = ""; 

            if (others.Count > extraLimit)
                sb.Append(connector).Append($"and {others.Count - extraLimit} more");

            return OtherKnownSources = sb.ToString();
        }

        public bool Matches(EventCitations other)
        {
            return SelectedItem == other.SelectedItem;
        }
    }
}