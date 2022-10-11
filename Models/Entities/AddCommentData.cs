using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATTM_API.Models.Entities
{
    public class AddCommentData
    {
        public List<string> RegressionTestIds { get; set; }
        public string Comment { get; set; }
        public string CommentBy { get; set; }
    }
}
