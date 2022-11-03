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
    public class ConfiguresController : ControllerBase
    {
        private readonly ConfigureService _configureService;

        public ConfiguresController(ConfigureService configureService)
        {
            _configureService = configureService;
        }


        /// <summary>
        /// Get all configurations for the system in appSetting
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<JObject>> Get()
        {
            var response = await _configureService.Get();
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