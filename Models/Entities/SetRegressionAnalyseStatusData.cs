using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATTM_API.Models.Entities
{
    public class SetRegressionAnalyseStatusData
    {
        public List<string> RegressionTestIds { get; set; }
        public string Status { get; set; }
        public string AnalyseBy { get; set; }
        public string Reason { get; set; }
        public string Issue { get; set; }
    }
}
