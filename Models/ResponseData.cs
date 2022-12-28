using System.Net;

namespace ATTM_API.Models
{
    public class ResponseData
    {
        public bool Success { get; set; }
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public object Data { get; set; }
        public object Error { get; set; }
        public string Message { get; set; }
        public int Count { get; set; }
    }
}
