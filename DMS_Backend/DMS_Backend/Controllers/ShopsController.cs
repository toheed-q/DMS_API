using DMS_Backend.Contracts;
using DMS_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS_Backend.Controllers
{
    [Authorize]
    [Route("api/shops")]
    public class ShopsController : BaseApiController
    {
        private readonly IShopService _shopService;

        public ShopsController(IShopService shopService) => _shopService = shopService;

        /// <summary>Paginated, searchable shop list. Optionally filter by route.</summary>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PaginationQuery query, [FromQuery] int? routeId)
            => HandleResult(await _shopService.GetShopsAsync(query, routeId));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
            => HandleResult(await _shopService.GetByIdAsync(id));
    }
}
