using DMS_Backend.Contracts;
using DMS_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS_Backend.Controllers
{
    [Authorize]
    [Route("api/returns")]
    public class ReturnsController : BaseApiController
    {
        private readonly IReturnService _returnService;

        public ReturnsController(IReturnService returnService) => _returnService = returnService;

        /// <summary>Records a customer return against a salesman; returned products go back into stock.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ReturnDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateReturnRequest request)
            => HandleCreated(await _returnService.CreateAsync(request));

        /// <summary>Returns, filterable by salesman/shop/date, with the total returned.</summary>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] ReturnQuery query)
            => HandleResult(await _returnService.GetReturnsAsync(query));
    }
}
