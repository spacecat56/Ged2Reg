using System.Numerics;

namespace G2RModel.Model
{
    public class ReportEntryBase
    {
        internal static BigInteger _nextInternalId;
        public string InternalId { get; set; } = $"{_nextInternalId++}";
    }
}