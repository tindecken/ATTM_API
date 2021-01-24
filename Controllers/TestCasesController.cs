using System;
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
    public class TestCasesController : ControllerBase
    {
        private readonly TestCaseService _testCaseService;

        public TestCasesController(TestCaseService testCaseService)
        {
            _testCaseService = testCaseService;
        }

        [HttpGet]
        public async Task<ActionResult<List<TestCase>>> Get() =>
            await _testCaseService.Get();

        [HttpGet("{id:length(24)}", Name = "GetTestCase")]
        public async Task<ActionResult<TestCase>> Get(string id)
        {
            var testCase = await _testCaseService.Get(id);

            if (testCase == null)
            {
                return NotFound();
            }

            return testCase;
        }

        [HttpPost("savetestcase")]
        public async Task<ActionResult<JObject>> SaveTestCase(TestCase tc)
        {
            return await _testCaseService.SaveTestCaseAsync(tc);
        }
    }
}