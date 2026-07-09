using DMS_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS_Backend.Controllers
{
    [Authorize]
    [Route("api/routes")]
    public class RoutesController : BaseApiController
    {
        private readonly IRouteService _routeService;

        public RoutesController(IRouteService routeService) => _routeService = routeService;

        /// <summary>All routes with their shop counts (reference data).</summary>
        [HttpGet]
        public async Task<IActionResult> Get()
            => HandleResult(await _routeService.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
            => HandleResult(await _routeService.GetByIdAsync(id));
    }
}
