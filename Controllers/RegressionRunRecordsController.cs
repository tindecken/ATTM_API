using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CommonModels;

namespace ATTM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegressionRunRecordsController : ControllerBase
    {
        private readonly RegressionRunRecordService _regressionRunRecordService;

        public RegressionRunRecordsController(RegressionRunRecordService regressionRunRecordService)
        {
            _regressionRunRecordService = regressionRunRecordService;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<List<RegressionRunRecord>> Get() =>
            _regressionRunRecordService.Get();

        [HttpGet("{id:length(24)}", Name = "GetRegressionRunRecord")]
        public async Task<ActionResult<RegressionRunRecord>> Get(string id)
        {
            var regressionRunRecord = await _regressionRunRecordService.Get(id);

            if (regressionRunRecord == null)
            {
                return NotFound();
            }

            return regressionRunRecord;
        }
    }
}