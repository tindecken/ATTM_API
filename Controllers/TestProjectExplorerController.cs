using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATTM_API.Controllers
{
    [Route("api/testprojectexplorer")]
    [ApiController]
    public class TestProjectExplorerController : ControllerBase
    {
        private readonly TestProjectExplorerService _testProjectExplorerService;

        public TestProjectExplorerController(TestProjectExplorerService testProjectExplorerService)
        {
            _testProjectExplorerService = testProjectExplorerService;
        }

        [HttpGet("getAll")]
        public async Task<JObject> getAllTests()
        {
            return await _testProjectExplorerService.GetAllTestsAsync();
        }
    }
}