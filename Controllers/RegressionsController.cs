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
    public class RegressionsController : ControllerBase
    {
        private readonly RegressionService _regressionService;

        public RegressionsController(RegressionService regressionService)
        {
            _regressionService = regressionService;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<List<Regression>> Get() =>
            _regressionService.Get();

        [HttpGet("{id:length(24)}", Name = "GetRegression")]
        public async Task<ActionResult<Regression>> Get(string id)
        {
            var regression = await _regressionService.Get(id);

            if (regression == null)
            {
                return NotFound();
            }

            return regression;
        }
    }
}