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
    public class TestClientsController : ControllerBase
    {
        private readonly TestClientService _testClientService;

        public TestClientsController(TestClientService testClientService)
        {
            _testClientService = testClientService;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<List<TestClient>> Get() =>
            _testClientService.Get();

        [HttpGet("{id:length(24)}", Name = "GetClient")]
        public async Task<ActionResult<TestClient>> Get(string id)
        {
            var category = await _testClientService.Get(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        [HttpPost]
        public async Task<ActionResult<Category>> Create(TestClient testClient)
        {
            var result = await _testClientService.Create(testClient);
            if(result != null) {
                return CreatedAtRoute("GetClient", new { id = testClient.Id.ToString() }, testClient);
            }else {
                return StatusCode(409, $"TestClient '{testClient.Name}' already exists.");
            }
        }


        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, TestClient testClient)
        {
            var client = _testClientService.Get(id);

            if (client == null)
            {
                return NotFound();
            }

            _testClientService.Update(id, testClient);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var client = await _testClientService.Get(id);

            if (client == null)
            {
                return NotFound();
            }
            _testClientService.Remove(client.Id);

            return NoContent();
        }
    }
}