using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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
        public ActionResult<List<Category>> Get() =>
            _categoryService.Get();

        [HttpGet("{id:length(24)}", Name = "GetCategory")]
        public ActionResult<Category> Get(string id)
        {
            var category = _categoryService.Get(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        [HttpPost]
        public ActionResult<Category> Create(Category category)
        {
            var result = _categoryService.Create(category);
            if(result != null) {
                return CreatedAtRoute("GetCategory", new { id = category.Id.ToString() }, category);
            }else {
                return StatusCode(409, $"Category '{category.CategoryName}' already exists.");
            }
        }


        [HttpPost("{id:length(24)}/testsuites")]
        public ActionResult<Category> CreateTestSuite([FromRoute] string id, TestSuite testSuite)
        {
            var result = _categoryService.CreateTestSuite(id, testSuite);
            if(result != null) {
                return Ok(testSuite);
            }else {
                return StatusCode(409, $"TestSuite '{testSuite.TestSuiteName}' already exists.");
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
        public IActionResult Delete(string id)
        {
            var category = _categoryService.Get(id);

            if (category == null)
            {
                return NotFound();
            }

            _categoryService.Remove(category.Id);

            return NoContent();
        }
    }
}