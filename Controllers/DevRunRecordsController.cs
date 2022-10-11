using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CommonModels;

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

        [Authorize]
        [HttpGet("testcases/{testCaseId:length(24)}")]
        public async Task<ActionResult<JObject>> GetDevRunRecordsForTestCase(string testCaseId)
        {
            var response = await _devRunRecordService.GetDevRunRecordsForTestCase(testCaseId);
            if (response == null) return StatusCode(500, $"Internal server error.");
            var result = response.GetValue("result").ToString();
            if (result.Equals("success"))
            {
                return StatusCode(200, response);
            }
            else
            {
                return StatusCode(500, response);
            }
        }

        [Authorize]
        [HttpGet("getlast/{testCaseId:length(24)}")]
        public async Task<ActionResult<JObject>> GetLastDevRunRecordsForTestCase(string testCaseId)
        {
            var response = await _devRunRecordService.GetLastDevRunRecordsForTestCase(testCaseId);
            if (response == null) return StatusCode(500, $"Internal server error.");
            var result = response.GetValue("result").ToString();
            if (result.Equals("success"))
            {
                return StatusCode(200, response);
            }
            else
            {
                return StatusCode(500, response);
            }
        }
        [Authorize]
        [HttpGet("getlast/testcase")]
        public async Task<ActionResult<JObject>> GetAllDevRunRecordsForTestCase()
        {
            var response = await _devRunRecordService.GetAllDevRunRecordsForTestCase();
            if (response == null) return StatusCode(500, $"Internal server error.");
            var result = response.GetValue("result").ToString();
            if (result.Equals("success"))
            {
                return StatusCode(200, response);
            }
            else
            {
                return StatusCode(500, response);
            }
        }
    }
}