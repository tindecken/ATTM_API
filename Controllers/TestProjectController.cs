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

        [HttpPost("generatecode")]
        public JObject generateCode([FromBody]List<TestCase> testcases)
        {
            return _testProjectService.GenerateCode(testcases, "Debug");
        }

        [HttpPost("createdevqueue")]
        public JObject createDevQueue([FromBody] JObject payload)
        {
            //var testcases = payload.ToObject<List<TestCase>>();
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
            return _testProjectService.CreateDevQueue(lstTestCases, testClient);
        }
    }
}