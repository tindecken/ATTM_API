using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("{id:length(24)}", Name = "GetKeyword")]
        public async Task<ActionResult<Keyword>> Get(string id)
        {
            var keyword = await _keywordService.Get(id);

            if (keyword == null)
            {
                return NotFound();
            }

            return keyword;
        }
    }
}