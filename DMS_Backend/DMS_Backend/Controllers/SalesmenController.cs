using DMS_Backend.Common;
using DMS_Backend.Contracts;
using DMS_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS_Backend.Controllers
{
    /// <summary>Employee records — office roles only. A field salesman has no business
    /// browsing the roster (and is scoped to their own data everywhere else).</summary>
    [Authorize(Roles = $"{Roles.Admin},{Roles.User}")]
    [Route("api/salesmen")]
    public class SalesmenController : BaseApiController
    {
        private readonly ISalesmanService _salesmanService;

        public SalesmenController(ISalesmanService salesmanService) => _salesmanService = salesmanService;

        /// <summary>Paginated, searchable salesman list. Optionally active-only.</summary>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PaginationQuery query, [FromQuery] bool? activeOnly)
            => HandleResult(await _salesmanService.GetSalesmenAsync(query, activeOnly));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
            => HandleResult(await _salesmanService.GetByIdAsync(id));
    }
}
