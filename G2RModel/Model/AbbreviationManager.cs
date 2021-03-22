using SimpleGedcomLib;

namespace G2RModel.Model
{
    public static class AbbreviationManager
    {
        public static bool AbbreviationsForChildEvents { get; set; }

        public static string TextFor(TagCode tc, bool isChild = false, string prefix = null)
        {
            bool abbr = isChild && AbbreviationsForChildEvents;
            switch (tc)
            {
                case TagCode.BIRT:
                    return abbr ? "b." : $"{prefix}born";
                case TagCode.BAPM:
                case TagCode.CHR:
                    return abbr ? "bp." : $"{prefix}baptized";
                case TagCode.MARR:
                    return abbr ? "m." : $"{prefix}married";
                case TagCode.DIV:
                    return abbr ? "div." : $"{prefix}divorced";
                case TagCode.DEAT:
                    return abbr ? "d." : $"{prefix}died";
                case TagCode.BURI:
                    return abbr ? "bd." : $"{prefix}buried";
                default:
                    return tc.Map().ToString();
            }
        }

    }
}
