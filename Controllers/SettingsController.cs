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
    public class SettingsController : ControllerBase
    {
        private readonly SettingService _settingService;

        public SettingsController(SettingService settingService)
        {
            _settingService = settingService;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<List<Setting>> Get() =>
            _settingService.Get();

        [HttpGet("{id:length(24)}", Name = "GetSetting")]
        public async Task<ActionResult<Setting>> Get(string id)
        {
            var setting = await _settingService.Get(id);

            if (setting == null)
            {
                return NotFound();
            }

            return setting;
        }

        [HttpPost]
        public async Task<ActionResult<Setting>> Create(Setting setting)
        {
            var result = await _settingService.Create(setting);
            if (result != null)
            {
                return CreatedAtRoute("GetSetting", new { id = setting.Id.ToString() }, setting);
            }
            else
            {
                return StatusCode(409, $"Setting '{setting.Name}' already exists.");
            }
        }
        [HttpPost("{settingId:length(24)}")]
        public async Task<ActionResult<JObject>> UpdateSetting(string settingId, [FromBody] Setting setting)
        {
            var response = await _settingService.Update(settingId, setting);
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
        [HttpGet("{Name}")]
        public async Task<ActionResult<JObject>> GetSettingByName(string Name)
        {
            var response = await _settingService.GetSettingByName(Name);
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