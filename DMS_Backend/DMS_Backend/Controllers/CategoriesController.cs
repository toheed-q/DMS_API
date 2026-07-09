using DMS_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS_Backend.Controllers
{
    [Authorize]
    [Route("api/categories")]
    public class CategoriesController : BaseApiController
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService) => _categoryService = categoryService;

        /// <summary>All categories with parent name and product counts (reference data).</summary>
        [HttpGet]
        public async Task<IActionResult> Get()
            => HandleResult(await _categoryService.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
            => HandleResult(await _categoryService.GetByIdAsync(id));
    }
}
