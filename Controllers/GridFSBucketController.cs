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
    public class GridFSBucketController : ControllerBase
    {
        private readonly GridFSBucketService _gridFSBucketService;

        public GridFSBucketController(GridFSBucketService gridFSBucketService)
        {
            _gridFSBucketService = gridFSBucketService;
        }


        [HttpGet("{id:length(24)}", Name = "GetScreenshot")]
        public async Task<ActionResult<JObject>> Get(string id)
        {
            var screenshot = await _gridFSBucketService.Get(id);

            if (screenshot == null)
            {
                return NotFound();
            }

            return screenshot;
        }
    }
}