using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATTM_API.Models.Entities
{
    public class SetRegressionQueueData
    {
        public List<string> RegressionTestIds { get; set; }
        public string ClientName { get; set; }
        public bool IsHighPriority { get; set; }
        public string UpdateBy { get; set; }
    }
}
