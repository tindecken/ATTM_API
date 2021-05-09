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
    public class DevRunRecordsController : ControllerBase
    {
        private readonly DevRunRecordService _devRunRecordService;

        public DevRunRecordsController(DevRunRecordService devRunRecordService)
        {
            _devRunRecordService = devRunRecordService;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<List<DevRunRecord>> Get() =>
            _devRunRecordService.Get();

        [HttpGet("{id:length(24)}", Name = "GetDevRunRecord")]
        public async Task<ActionResult<DevRunRecord>> Get(string id)
        {
            var devRunRecord = await _devRunRecordService.Get(id);

            if (devRunRecord == null)
            {
                return NotFound();
            }

            return devRunRecord;
        }
    }
}