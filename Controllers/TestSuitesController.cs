using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    }
}