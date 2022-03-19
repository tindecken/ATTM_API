using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATTM_API.Models.Entities
{
    public class UpdateTestEnvData
    {
        public string UpdateMessage { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateType { get; set; }
    }
}
