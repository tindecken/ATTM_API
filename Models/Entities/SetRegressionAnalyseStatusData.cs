using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonModels;

namespace ATTM_API.Models.Entities
{
    public class SetRegressionAnalyseStatusData
    {
        public List<string> RegressionTestIds { get; set; }
        public TestStatus Status { get; set; }
        public string AnalyseBy { get; set; }
        public string Reason { get; set; }
        public string Issue { get; set; }
    }
}
