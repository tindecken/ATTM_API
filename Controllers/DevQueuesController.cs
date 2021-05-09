using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ATTM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevQueuesController : ControllerBase
    {
        private readonly DevQueueService _devQueueService;

        public DevQueuesController(DevQueueService devQueueService)
        {
            _devQueueService = devQueueService;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<List<DevQueue>> Get() =>
            _devQueueService.Get();

        [HttpGet("{id:length(24)}", Name = "GetDevQueue")]
        public async Task<ActionResult<DevQueue>> Get(string id)
        {
            var devQueue = await _devQueueService.Get(id);

            if (devQueue == null)
            {
                return NotFound();
            }

            return devQueue;
        }
    }
}