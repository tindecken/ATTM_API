using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATTM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestAUTsController : ControllerBase
    {
        private readonly TestAUTService _testAUTService;

        public TestAUTsController(TestAUTService testAUTService)
        {
            _testAUTService = testAUTService;
        }

        [HttpGet]
        public async Task<ActionResult<List<TestAUT>>> Get() =>
            await _testAUTService.Get();

        [HttpGet("{id:length(24)}", Name = "GetTestAUT")]
        public async Task<ActionResult<TestAUT>> Get(string id)
        {
            var testAUT = await _testAUTService.Get(id);

            if (testAUT == null)
            {
                return NotFound();
            }

            return testAUT;
        }
        
    }
}