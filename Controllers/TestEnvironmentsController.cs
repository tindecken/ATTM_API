using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATTM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestEnvironmentsController : ControllerBase
    {
        private readonly TestEnvironmentService _testEnvironmentService;

        public TestEnvironmentsController(TestEnvironmentService testEnvironmentService)
        {
            _testEnvironmentService = testEnvironmentService;
        }

        [HttpGet]
        public ActionResult<List<TestEnvironment>> Get() =>
            _testEnvironmentService.Get();

        [HttpGet("{id:length(24)}", Name = "GetTestEnvironment")]
        public async Task<ActionResult<TestEnvironment>> Get(string id)
        {
            var testEnv = await _testEnvironmentService.Get(id);

            if (testEnv == null)
            {
                return NotFound();
            }

            return testEnv;
        }

        [HttpPost]
        public async Task<ActionResult<TestEnvironment>> Create(TestEnvironment testEnv)
        {
            var result = await _testEnvironmentService.Create(testEnv);
            if(result != null) {
                return CreatedAtRoute("GetTestEnvironment", new { id = testEnv.Id.ToString() }, testEnv);
            }else {
                return StatusCode(409, $"Test Environment '{testEnv.Name}' already exists.");
            }
        }
    }
}