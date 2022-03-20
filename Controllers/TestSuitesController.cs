using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ATTM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestSuitesController : ControllerBase
    {
        private readonly TestSuiteService _testSuiteService;

        public TestSuitesController(TestSuiteService testSuiteService)
        {
            _testSuiteService = testSuiteService;
        }

        [HttpGet]
        public async Task<ActionResult<List<TestSuite>>> Get() =>
            await _testSuiteService.Get();

        [HttpGet("{id:length(24)}", Name = "GetTestSuite")]
        public async Task<ActionResult<TestSuite>> Get(string id)
        {
            var testsuite = await _testSuiteService.Get(id);

            if (testsuite == null)
            {
                return NotFound();
            }

            return testsuite;
        }

        [HttpPost("{tsId:length(24)}/testgroups")]
        public async Task<ActionResult<TestSuite>> CreateTestGroup(string tsId, TestGroup testGroup)
        {
            var result = await _testSuiteService.CreateTestGroup(tsId, testGroup);
            if (result != null)
            {
                return CreatedAtRoute("GetTestGroup", new { controller = "testgroups", id = result.Id }, testGroup);
            }
            else
            {
                return StatusCode(409, $"TestGroup '{testGroup.CodeName}' already exists.");
            }
        }

        [HttpPost("{testSuiteId:length(24)}/testgroups/delete")]
        [Authorize]
        public async Task<ActionResult<JObject>> DeleteTestGroups(string testSuiteId, [FromBody] List<string> lstTestGroupIds)
        {
            var response = await _testSuiteService.DeleteTestGroups(testSuiteId, lstTestGroupIds);
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
        [HttpPost("updatetestsuite")]
        public async Task<ActionResult<JObject>> UpdateCategory(TestSuite testsuite)
        {
            var response = await _testSuiteService.UpdateTestSuiteAsync(testsuite);
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