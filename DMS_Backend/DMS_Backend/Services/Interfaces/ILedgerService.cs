using DMS_Backend.Common;
using DMS_Backend.Contracts;

namespace DMS_Backend.Services.Interfaces
{
    /// <summary>A customer's (shop's) ledger: aggregate KPIs and the entry list.</summary>
    public interface ILedgerService
    {
        Task<Result<LedgerSummaryDto>> GetSummaryAsync(int shopId);
        Task<Result<PagedResult<LedgerEntryDto>>> GetEntriesAsync(int shopId, LedgerEntryQuery query);
    }
}
