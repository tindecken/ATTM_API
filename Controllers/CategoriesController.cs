using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                return StatusCode(409, $"TestSuite '{testSuite.Name}' already exists.");
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
    }
}