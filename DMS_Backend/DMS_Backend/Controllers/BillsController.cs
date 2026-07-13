using DMS_Backend.Contracts;
using DMS_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS_Backend.Controllers
{
    [Authorize]
    [Route("api/bills")]
    public class BillsController : BaseApiController
    {
        private readonly IBillService _billService;

        public BillsController(IBillService billService) => _billService = billService;

        /// <summary>Creates a bill (POS sale) atomically and posts it to the customer's ledger.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BillDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateBillRequest request)
            => HandleCreated(await _billService.CreateAsync(request));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
            => HandleResult(await _billService.GetByIdAsync(id));
    }
}
