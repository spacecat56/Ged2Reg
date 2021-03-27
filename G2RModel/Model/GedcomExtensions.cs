using SimpleGedcomLib;

namespace G2RModel
{
    public static class GedcomExtensions
    {
        public static string ReAssemble(this Tag t)
        {
            string id = t.Id == null ? null : $" {t.Id}";
            string content = t.Content == null ? null : $" {t.Content}";
            return $"{t.Level}{id} {t.Code}{content}";
        }

        public static string GetEditedCitationText(this Tag t)
        {
            if (t?.Code != TagCode.SOUR)
                return null;
            Tag edCite = t.GetFirstDescendant(TagCode._FOOT);
            string rv = edCite?.FullText();
            return rv;
        }
    }
}