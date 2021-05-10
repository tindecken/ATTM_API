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

        [HttpPost("createregressiontest")]
        [Authorize]
        public async Task<ActionResult<JObject>> CreateRegressionTest([FromBody] TestCase testCase)
        {
            var response = await _regressionTestService.CreateRegressionTestFromTestCase(testCase);
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

        [HttpPost("{testcaseId:length(24)}", Name = "createregressiontest")]
        [Authorize]
        public async Task<ActionResult<JObject>> CreateRegressionTest(string testcaseId)
        {
            var response = await _regressionTestService.CreateRegressionTestFromTestCaseId(testcaseId);
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
    }
}