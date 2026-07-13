using DMS_Backend.Contracts;
using DMS_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS_Backend.Controllers
{
    [Authorize]
    [Route("api/shops/{shopId:int}/ledger")]
    public class LedgerController : BaseApiController
    {
        private readonly ILedgerService _ledgerService;

        public LedgerController(ILedgerService ledgerService) => _ledgerService = ledgerService;

        /// <summary>Ledger KPIs: total bills, returns, received, outstanding and excess.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(LedgerSummaryDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSummary(int shopId)
            => HandleResult(await _ledgerService.GetSummaryAsync(shopId));

        /// <summary>Paginated ledger entries (bills, returns, manual entries).</summary>
        [HttpGet("entries")]
        public async Task<IActionResult> GetEntries(int shopId, [FromQuery] LedgerEntryQuery query)
            => HandleResult(await _ledgerService.GetEntriesAsync(shopId, query));
    }
}
