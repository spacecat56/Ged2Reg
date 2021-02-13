using System.Text;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Wordprocessing;

namespace WpdInterfaceLib
{
    public class NoteRefField : OoxFieldBase
    {
        public string MarkName { get; set; }
        public bool SameFormatting { get; set; }
        public bool InsertHyperlink { get; set; }
        public bool InsertRelativePosition { get; set; }
 

        #region Overrides of AbstractField

        public override WpdFieldBase Build()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" NOTEREF ").Append(MarkName).Append(' ');
            if (SameFormatting)
                sb.Append("\\f ");
            if (InsertRelativePosition)
                sb.Append("\\p ");
            if (InsertHyperlink)
                sb.Append("\\h ");

            FieldText = sb.ToString();
            return this;
        }

        #endregion

        public NoteRefField(OoxDoc document, XElement xml = null) : base(document) { }
    }
}
