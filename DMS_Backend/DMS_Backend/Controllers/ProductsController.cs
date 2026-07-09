using DMS_Backend.Contracts;
using DMS_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS_Backend.Controllers
{
    [Authorize]
    [Route("api/products")]
    public class ProductsController : BaseApiController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService) => _productService = productService;

        /// <summary>Paginated, searchable product list (image blob excluded for speed).</summary>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PaginationQuery query)
            => HandleResult(await _productService.GetProductsAsync(query));

        /// <summary>Single product by id.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
            => HandleResult(await _productService.GetByIdAsync(id));
    }
}
