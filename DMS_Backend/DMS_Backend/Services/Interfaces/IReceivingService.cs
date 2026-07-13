using DMS_Backend.Common;
using DMS_Backend.Contracts;

namespace DMS_Backend.Services.Interfaces
{
    /// <summary>Customer-level payments (receivings) against a shop's ledger.</summary>
    public interface IReceivingService
    {
        Task<Result<ReceivingDto>> CreateAsync(CreateReceivingRequest request);
        Task<Result<PagedResult<ReceivingDto>>> GetByShopAsync(int shopId, PaginationQuery query);
    }
}
