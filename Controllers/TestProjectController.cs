using System;
using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ATTM_API.Controllers
{
    [Route("api/testproject")]
    [ApiController]
    public class TestProjectController : ControllerBase
    {
        private readonly TestProjectService _testProjectService;
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        public TestProjectController(TestProjectService testProjectService)
        {
            _testProjectService = testProjectService;
        }

        [HttpPost("generatedevcode")]
        public async Task<ActionResult<JObject>> generateDevCode([FromBody]List<TestCase> testcases)
        {
            var response = await _testProjectService.GenerateCode(testcases, "Dev");
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

        [HttpPost("generateregressioncode")]
        public async Task<ActionResult<JObject>> generateRegressionCode([FromBody] List<TestCase> testcases)
        {
            var response = await _testProjectService.GenerateCode(testcases, "Regression");
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

        [HttpPost("createdevqueue")]
        public async Task<ActionResult<JObject>> createDevQueue([FromBody] JObject payload)
        {
            List<TestCase> lstTestCases = new List<TestCase>();
            TestClient testClient = new TestClient();
            foreach (KeyValuePair<string, JToken> property in payload)
            {
                if (property.Key.Equals("testcases"))
                {
                    lstTestCases = property.Value.ToObject<List<TestCase>>();
                }
                else if (property.Key.Equals("testClient"))
                {
                    testClient = property.Value.ToObject<TestClient>();
                }
            }
            var response = await _testProjectService.CreateDevQueue(lstTestCases, testClient);
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

        [HttpPost("buildproject")]
        public async Task<ActionResult<JObject>> buildProject()
        {
            var response = await _testProjectService.BuildProject();
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