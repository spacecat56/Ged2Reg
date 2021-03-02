using System;

namespace Ged2Reg.Model
{
    public class ReportStats
    {
        public TimeSpan PrepTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan ReportTime => EndTime.Subtract(StartTime);
        public int MainPerson { get; set; }
        public int MainSpouse { get; set; }
        public int NonContinuedPerson { get; set; }
        public int NonContinuedSpouses { get; set; }
        public int Citations { get; set; }
        public int DistinctCitations { get; set; }
        public int SpouseParents { get; set; }
        public int MaybeLiving { get; set; }

        public ReportStats Init(TimeSpan prep)
        {
            PrepTime = prep;
            StartTime = DateTime.Now;
            return this;
        }
    }
}