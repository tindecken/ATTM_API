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
    public class CategoriesController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        public CategoriesController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<List<Category>> Get() =>
            _categoryService.Get();

        [HttpGet("{id:length(24)}", Name = "GetCategory")]
        public async Task<ActionResult<Category>> Get(string id)
        {
            var category = await _categoryService.Get(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        [HttpGet("getAll")]
        public async Task<JObject> getAll()
        {
            return await _categoryService.GetAllAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Category>> Create(Category category)
        {
            var result = await _categoryService.Create(category);
            if(result != null) {
                return CreatedAtRoute("GetCategory", new { id = category.Id.ToString() }, category);
            }else {
                return StatusCode(409, $"Category '{category.Name}' already exists.");
            }
        }


        [HttpPost("{catId:length(24)}/testsuites")]
        public async Task<ActionResult<TestSuite>> CreateTestSuite(string catId, TestSuite testSuite)
        {
            var result = await _categoryService.CreateTestSuite(catId, testSuite);
            if(result != null) {
                return CreatedAtRoute("GetTestSuite", new { controller = "testsuites", id = result.Id }, testSuite);
            }else {
                return StatusCode(409, $"TestSuite '{testSuite.CodeName}' already exists.");
            }
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, Category categoryIn)
        {
            var category = _categoryService.Get(id);

            if (category == null)
            {
                return NotFound();
            }

            _categoryService.Update(id, categoryIn);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var category = await _categoryService.Get(id);

            if (category == null)
            {
                return NotFound();
            }
            _categoryService.Remove(category.Id);

            return NoContent();
        }

        [HttpPost("{categoryId:length(24)}/testsuites/delete")]
        [Authorize]
        public async Task<ActionResult<JObject>> DeleteTestSuites(string categoryId, [FromBody] List<string> lstTestSuiteIds)
        {
            var response = await _categoryService.DeleteTestSuites(categoryId, lstTestSuiteIds);
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
        [HttpPost("delete")]
        [Authorize]
        public async Task<ActionResult<JObject>> DeleteCategory([FromBody] string categoryId)
        {
            var response = await _categoryService.DeleteCategory(categoryId);
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

        [HttpPost("create")]
        [Authorize]
        public async Task<ActionResult<JObject>> CreateCategory([FromBody] Category category)
        {
            var response = await _categoryService.CreateCategory(category);
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
        [HttpPost("updatecategory")]
        public async Task<ActionResult<JObject>> UpdateCategory(Category cat)
        {
            var response = await _categoryService.UpdateCategoryAsync(cat);
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
        [HttpGet("test")]
        public async Task<ActionResult> Test()
        {
            return StatusCode(200, "It 33333333.....");
        }
    }
}