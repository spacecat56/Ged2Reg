using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G2RModel.Model
{
    public class AsyncActionDelegates
    {
        public delegate void ReportStatus(string txt);
        public delegate void CancelEnableDelegate(bool ena);

        public CancelEnableDelegate CancelEnable { get; set; }
        public ReportStatus PostStatusReport { get; set; }

        public bool CancelRequested { get; set; }
    }
}
