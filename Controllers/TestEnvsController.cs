using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ATTM_API.Models.Entities;
using Newtonsoft.Json.Linq;

namespace ATTM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestEnvsController : ControllerBase
    {
        private readonly TestEnvService _testEnvService;

        public TestEnvsController(TestEnvService testEnvironmentService)
        {
            _testEnvService = testEnvironmentService;
        }

        [HttpGet]
        public ActionResult<List<TestEnv>> Get() =>
            _testEnvService.Get();

        [HttpGet("{id:length(24)}", Name = "GetTestEnvironment")]
        public async Task<ActionResult<TestEnv>> Get(string id)
        {
            var response = await _testEnvService.GetTestEnv(id);
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

        [HttpPost]
        public async Task<ActionResult<TestEnv>> Create(TestEnv testEnv)
        {
            var result = await _testEnvService.Create(testEnv);
            if(result != null) {
                return CreatedAtRoute("GetTestEnvironment", new { id = testEnv.Id.ToString() }, testEnv);
            }else {
                return StatusCode(409, $"Test Environment '{testEnv.Name}' already exists.");
            }
        }
        [HttpPost("clone")]
        [Authorize]
        public async Task<ActionResult<JObject>> CloneTestEnv([FromBody] TestEnvCloneData testEnvCloneData)
        {
            var response = await _testEnvService.CloneTestEnv(testEnvCloneData);
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
        [HttpPost("update")]
        public async Task<ActionResult<JObject>> UpdateTestEnv([FromBody] TestEnvHistory testEnvHistory)
        {
            var response = await _testEnvService.UpdateTestEnvAsync(testEnvHistory);
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
        [HttpPost("create")]
        public async Task<ActionResult<JObject>> UpdateTestEnv([FromBody] TestEnv testEnv)
        {
            var response = await _testEnvService.NewTestEnv(testEnv);
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
        [HttpPost("delete")]
        public async Task<ActionResult<JObject>> DeleteTestEnv([FromBody] TestEnvHistory testEnvHistory)
        {
            var response = await _testEnvService.DeleteTestEnv(testEnvHistory);
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
        [HttpGet("histories/{id:length(24)}")]
        public async Task<ActionResult<TestEnvHistory>> GetTestEnvHistories(string id)
        {
            var response = await _testEnvService.GetUpdateHistories(id);
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