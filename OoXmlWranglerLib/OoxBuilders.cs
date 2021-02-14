using DocumentFormat.OpenXml.Wordprocessing;
using WpdInterfaceLib;

namespace OoXmlWranglerLib
{
    public static class OoxBuilders
    {
        public static RunProperties BuildRunProperties(Formatting fmt)
        {
            var rv = new RunProperties();
            if (fmt == null)
                return rv;

            if (!string.IsNullOrEmpty(fmt.CharacterStyleName))
                rv.AppendChild(new RunStyle() { Val = fmt.CharacterStyleName });

            if (fmt.Bold ?? false)
                rv.AppendChild(new Bold());

            if (fmt.Italic ?? false)
                rv.AppendChild(new Italic());

            // todo: more properties other than style?

            return rv;
        }
    }
}
