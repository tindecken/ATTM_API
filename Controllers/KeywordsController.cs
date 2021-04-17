using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        [HttpGet("refresh")]
        [Authorize]
        public async Task<JObject> Refresh() {
            return await _keywordService.RefreshAsync();
        }
    }
}