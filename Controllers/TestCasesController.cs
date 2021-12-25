using System;
using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ATTM_API.Models.Entities;
using Newtonsoft.Json.Linq;
using NUnit.Framework.Internal;

namespace ATTM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestCasesController : ControllerBase
    {
        private readonly TestCaseService _testCaseService;
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        public TestCasesController(TestCaseService testCaseService)
        {
            _testCaseService = testCaseService;
        }

        [HttpGet]
        public async Task<ActionResult<List<TestCase>>> Get() =>
            await _testCaseService.Get();

        [HttpGet("getAllDetail")]
        public async Task<ActionResult<JObject>> GetAllDetail()
        {
            var response = await _testCaseService.GetAllDetail();
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
        public async Task<ActionResult<JObject>> SaveTestCase(TestCaseHistory testCaseHistory)
        {
            var response = await _testCaseService.SaveTestCaseAsync(testCaseHistory);
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
        [HttpPost("updatetestcase")]
        public async Task<ActionResult<JObject>> UpdateTestCase(TestCaseHistory testCaseHistory)
        {
            var response = await _testCaseService.UpdateTestCaseAsync(testCaseHistory);
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
        [HttpGet("{id:length(24)}/getupdatehistories", Name = "GetTestCaseUpdateHistories")]
        public async Task<ActionResult<TestCase>> GetUpdateHistories(string id)
        {
            var response = await _testCaseService.GetUpdateHistories(id);
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