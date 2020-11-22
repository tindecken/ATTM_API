using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATTM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestGroupsController : ControllerBase
    {
        private readonly TestGroupService _testGroupService;

        public TestGroupsController(TestGroupService testGroupService)
        {
            _testGroupService = testGroupService;
        }

        [HttpGet]
        public async Task<ActionResult<List<TestGroup>>> Get() =>
            await _testGroupService.Get();

        [HttpGet("{id:length(24)}", Name = "GetTestGroup")]
        public async Task<ActionResult<TestGroup>> Get(string id)
        {
            var testgroup = await _testGroupService.Get(id);

            if (testgroup == null)
            {
                return NotFound();
            }

            return testgroup;
        }

        [HttpPost("{tgId:length(24)}/testcases")]
        public async Task<ActionResult<TestCase>> CreateTestCase(string tgId, TestCase testCase)
        {
            var result = await _testGroupService.CreateTestCase(tgId, testCase);
            if (result != null)
            {
                return CreatedAtRoute("GetTestCase", new { controller = "testcases", id = result.Id }, testCase);
            }
            else
            {
                return StatusCode(409, $"TestCase '{testCase.Name}' already exists.");
            }
        }
    }
}