using DMS_Backend.Contracts;
using DMS_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS_Backend.Controllers
{
    [Authorize]
    [Route("api/receivings")]
    public class ReceivingsController : BaseApiController
    {
        private readonly IReceivingService _receivingService;

        public ReceivingsController(IReceivingService receivingService) => _receivingService = receivingService;

        /// <summary>Records a customer payment (receiving) against a shop.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ReceivingDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateReceivingRequest request)
            => HandleCreated(await _receivingService.CreateAsync(request));

        /// <summary>Paginated receiving history for a shop.</summary>
        [HttpGet]
        public async Task<IActionResult> GetByShop([FromQuery] int shopId, [FromQuery] PaginationQuery query)
            => HandleResult(await _receivingService.GetByShopAsync(shopId, query));
    }
}
