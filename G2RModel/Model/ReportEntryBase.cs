using System.Numerics;

namespace G2RModel.Model
{
    public class ReportEntryBase
    {
        internal static BigInteger _nextInternalId;
        public string InternalId { get; set; } = $"{_nextInternalId++}";

        public virtual string Who
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}