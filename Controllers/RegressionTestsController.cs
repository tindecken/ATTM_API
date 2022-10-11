using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ATTM_API.Models.Entities;
using CommonModels;

namespace ATTM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegressionTestsController : ControllerBase
    {
        private readonly RegressionTestService _regressionTestService;

        public RegressionTestsController(RegressionTestService regressionTestService)
        {
            _regressionTestService = regressionTestService;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<List<RegressionTest>> Get() =>
            _regressionTestService.Get();

        [HttpGet("{id:length(24)}", Name = "GetRegressionTest")]
        public async Task<ActionResult<RegressionTest>> Get(string id)
        {
            var regressionTest = await _regressionTestService.Get(id);

            if (regressionTest == null)
            {
                return NotFound();
            }

            return regressionTest;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<RegressionTest>> Create(RegressionTest regressionTest)
        {
            var result = await _regressionTestService.Create(regressionTest);
            if (result != null)
            {
                return CreatedAtRoute("GetRegressionTest", new { id = regressionTest.Id.ToString() }, regressionTest);
            }
            else
            {
                return StatusCode(500, $"Internal server error, can't create RegressionTest {regressionTest.TestCaseCodeName}");
            }
        }

        [HttpGet("{regressionTestId:length(24)}", Name = "GetRegressionTestRunResult")]
        [Authorize]
        public async Task<ActionResult<JObject>> GetLastRegressionTestRunResult(string regressionTestId)
        {
            var response = await _regressionTestService.GetLastRegressionTestRunResult(regressionTestId);
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
        [HttpPost("{regressionTestId:length(24)}", Name = "SaveCommentsRegressionTest")]
        [Authorize]
        public async Task<ActionResult<JObject>> SaveComments(string regressionTestId, [FromBody] JObject commentObject)
        {
            var response = await _regressionTestService.SaveComments(regressionTestId, commentObject);
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
        [HttpPost("addComment")]
        [Authorize]
        public async Task<ActionResult<JObject>> AddComment([FromBody] AddCommentData addCommentData)
        {
            var response = await _regressionTestService.AddComment(addCommentData);
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

        [HttpPost("setRegressionQueue")]
        [Authorize]
        public async Task<ActionResult<JObject>> setRegressionQueue([FromBody] SetRegressionQueueData setRegressionQueueData)
        {
            var response = await _regressionTestService.setRegressionQueue(setRegressionQueueData);
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
        [HttpPost("setRegressionAnalyseStatus")]
        [Authorize]
        public async Task<ActionResult<JObject>> setRegressionAnalyseStatus([FromBody] SetRegressionAnalyseStatusData setRegressionAnalyseStatusData)
        {
            var response = await _regressionTestService.setRegressionAnalyseStatus(setRegressionAnalyseStatusData);
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