using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonModels;

namespace ATTM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeywordsController : ControllerBase
    {
        private readonly KeywordService _keywordService;

        public KeywordsController(KeywordService keywordService)
        {
            _keywordService = keywordService;
        }

        [HttpGet]
        public ActionResult<List<Keyword>> Get() =>
            _keywordService.Get();

        [HttpGet("getkeywords")]
        [Authorize]
        public async Task<ActionResult<JObject>> GetKeywords()
        {
            var response = await _keywordService.GetKeywords();
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