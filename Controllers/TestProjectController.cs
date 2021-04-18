using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATTM_API.Controllers
{
    [Route("api/testproject")]
    [ApiController]
    public class TestProjectController : ControllerBase
    {
        private readonly TestProjectService _testProjectService;

        public TestProjectController(TestProjectService testProjectService)
        {
            _testProjectService = testProjectService;
        }

        [HttpPost("generatecode")]
        public JObject generateCode([FromBody]List<TestCase> testcases)
        {
            return _testProjectService.GenerateCode(testcases, "Debug");
        }
    }
}