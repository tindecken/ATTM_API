using ATTM_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ATTM_API.Helpers
{
    public class TransformResponse: ControllerBase
    {
        public IActionResult Transform(ResponseData data)
        {
            return TransformData(data.StatusCode, data);
        }
        private IActionResult TransformData(HttpStatusCode statusCode, object data)
        {
            switch (statusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    return Ok(data);
                case System.Net.HttpStatusCode.NotFound:
                    return NotFound(data);
                case System.Net.HttpStatusCode.BadRequest:
                    return BadRequest(data);
                case System.Net.HttpStatusCode.NoContent:
                    return NoContent();
                case System.Net.HttpStatusCode.InternalServerError:
                    return StatusCode(500, data);
                default:
                    return StatusCode((int)statusCode, data);
            }
        }
    }
}
