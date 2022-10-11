using System;
using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonModels;
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
            var response = await _testProjectService.GenerateDevCode(testcases);
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

        [HttpPost("generateregcode")]
        public async Task<ActionResult<JObject>> generateRegressionCode([FromBody] List<RegressionTest> regressionTests)
        {
            var response = await _testProjectService.GenerateRegressionCode(regressionTests);
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
        [HttpPost("getlatestcode")]
        public async Task<ActionResult<JObject>> getLatestCode()
        {
            var response = await _testProjectService.GetLatestCode();
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
        [HttpPost("copycodetoclient/{type}")]
        public async Task<ActionResult<JObject>> copyCodeToClients([FromBody] TestClient testClient, string type)
        {
            var response = await _testProjectService.CopyCodeToClient(testClient, type);
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
        [HttpPost("runautorunner")]
        public async Task<ActionResult<JObject>> runAutoRunner([FromBody] TestClient testClient)
        {
            var response = await _testProjectService.RunAutoRunner(testClient);
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

        [HttpPost("checkrunner/{process}")]
        public async Task<ActionResult<JObject>> CheckRunner([FromBody] TestClient testClient, string process)
        {
            var response = await _testProjectService.CheckRunner(testClient, process);
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

        [HttpPost("updatereleaseforclient/{newValue}")]
        public async Task<ActionResult<JObject>> updateReleaseForClient([FromBody] TestClient testClient, string newValue)
        {
            var response = await _testProjectService.UpdateReleaseForClient(testClient, newValue);
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