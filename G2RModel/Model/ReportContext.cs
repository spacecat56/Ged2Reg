using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ged2Reg.Model
{
    public class ReportContext
    {
        private static ReportContext _theInstance;
        public static ReportContext Instance => _theInstance;

        public static ReportContext Init(G2RSettings settings)
        {
            return _theInstance = new ReportContext() {Settings = settings};
        }

        public G2RSettings Settings { get; private set; }

        //public int LastPersonNumber { get; set; }

        //public int CurrentGeneration { get; set; }

        //public Dictionary<string, string> MappedNames { get; } = new Dictionary<string, string>();

        public RegisterReportModel Model { get; set; }


    }
}
