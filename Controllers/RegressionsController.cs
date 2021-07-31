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

        [HttpPost]
        public async Task<ActionResult<Regression>> Create(Regression regression)
        {
            var result = await _regressionService.Create(regression);
            if (result != null)
            {
                return CreatedAtRoute("GetRegression", new { id = regression.Id.ToString() }, regression);
            }
            else
            {
                return StatusCode(409, $"Regression '{regression.Name}' already exists.");
            }
        }

        [HttpPost("{regressionId:length(24)}/createregressiontests")]
        [Authorize]
        public async Task<ActionResult<JObject>> CreateRegressionTests(string regressionId, [FromBody] List<string> testcaseIds)
        {
            var response = await _regressionService.CreateRegressionTestsFromTestCaseIds(regressionId, testcaseIds);
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

        [HttpPost("{regressionId:length(24)}/regressiontest/{regressionTestId:length(24)}")]
        public async Task<ActionResult<JObject>> AddTestToRegression(string regressionId, string regressionTestId)
        {
            var response = await _regressionService.AddTestToRegression(regressionId, regressionTestId);
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

        [HttpGet("{id:length(24)}/getdetail", Name = "GetDetailRegression")]
        public async Task<ActionResult<JObject>> GetDetailRegression(string id)
        {
            var response = await _regressionService.GetDetailRegression(id);
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