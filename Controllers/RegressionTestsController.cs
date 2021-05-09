using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ATTM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegressionTestsController : ControllerBase
    {
        private readonly RegressionTestService _regressionTestService;

        public RegressionTestsController(RegressionTestService regressionTestService)
        {
            _regressionTestService = regressionTestService;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<List<RegressionTest>> Get() =>
            _regressionTestService.Get();

        [HttpGet("{id:length(24)}", Name = "GetRegressionTest")]
        public async Task<ActionResult<RegressionTest>> Get(string id)
        {
            var regressionTest = await _regressionTestService.Get(id);

            if (regressionTest == null)
            {
                return NotFound();
            }

            return regressionTest;
        }
    }
}