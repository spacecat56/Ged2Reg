using System.Text;
using System.Xml.Linq;

namespace WpdInterfaceLib
{
    public interface IWpdIndexEntry
    {
        string IndexValue { get; set; }
        string IndexName { get; set; }
        string SeeInstead { get; set; }
    }

    public class IndexEntry : OoxFieldBase, IWpdIndexEntry
    {
        public string IndexValue { get; set; }
        public string IndexName { get; set; }
        public string SeeInstead { get; set; }
        public IWpdDocument Document { get; set; }

        public IndexEntry(IWpdDocument document) : base(document) { }

        #region Overrides of AbstractField

        public override OoxFieldBase Build()
        {
            // build the contents of the field
            string fieldContents = $" XE \"{IndexValue}\" ";
            if (SeeInstead != null)
                fieldContents = $"{fieldContents}\\t \"See {SeeInstead}\" ";
            if (IndexName != null)
                fieldContents = $"{fieldContents}\\f \"{IndexName}\" ";

            // wrap it in the field delimiters
            //Xml = Build(fieldContents);
            FieldText = fieldContents;
            return this;
        }
        #endregion
    }

    /// <summary>
    /// Class to produce the XML for an index field.
    /// See ECMA-376-1:2016 / Office Open XML File Formats — Fundamentals and Markup Language Reference / October 2016, pages 1220 - 1222
    ///
    /// NB several of the less-common options may not have been tested
    /// 
    /// </summary>
    public class IndexField : OoxFieldBase
    {
        public static string UpdateFieldPrompt { get; set; } = "Right-click and Update (this) field to generate the index";
        public string Bookmark { get; set; }
        public int Columns { get; set; }
        public string SequencePageSeparator { get; set; }
        public string EntryPageSeparator { get; set; }
        public string IndexName { get; set; }
        public string PageRangeSeparator { get; set; }
        public string LetterHeading { get; set; }
        public string XrefSeparator { get; set; }
        public string PagePageSeparator { get; set; }
        public string LetterRange { get; set; }
        public bool RunSubentries { get; set; }
        public string SequenceName { get; set; }
        public bool EnableYomi { get; set; }
        public string LanguageId { get; set; }



        public IndexField(OoxDoc document, XElement xml = null) : base(document) { }

        #region Overrides of AbstractField

        public override OoxFieldBase Build()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(" INDEX ");
            AppendNonEmpty(sb, "b", Bookmark);
            if (Columns > 0)
                AppendNonEmpty(sb, "c", Columns.ToString());
            AppendNonEmpty(sb, "d", SequencePageSeparator);
            AppendNonEmpty(sb, "e", EntryPageSeparator);
            AppendNonEmpty(sb, "f", IndexName);
            AppendNonEmpty(sb, "g", PageRangeSeparator);
            AppendNonEmpty(sb, "h", LetterHeading);
            AppendNonEmpty(sb, "k", XrefSeparator);
            AppendNonEmpty(sb, "l", PagePageSeparator);
            AppendNonEmpty(sb, "p", LetterRange);
            if (RunSubentries) sb.Append("\\r ");
            AppendNonEmpty(sb, "s", SequenceName);
            if (EnableYomi) sb.Append("\\y ");
            AppendNonEmpty(sb, "z", LanguageId);

            //Xml = Build(sb.ToString(), UpdateFieldPrompt);
            FieldText = sb.ToString();
            ContentText = UpdateFieldPrompt;
            return this;
        }

        private void AppendNonEmpty(StringBuilder sb, string field, string fieldArg)
        {
            if (string.IsNullOrEmpty(fieldArg)) return;
            // we always leave a trailing space, needed to separate from the field end mark
            sb.Append($"\\{field} \"{fieldArg}\" ");
        }

        #endregion
    }
}
