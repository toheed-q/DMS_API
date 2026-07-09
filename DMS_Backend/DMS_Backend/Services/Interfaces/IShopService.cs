using DMS_Backend.Common;
using DMS_Backend.Contracts;

namespace DMS_Backend.Services.Interfaces
{
    public interface IShopService
    {
        Task<Result<PagedResult<ShopDto>>> GetShopsAsync(PaginationQuery query, int? routeId);
        Task<Result<ShopDto>> GetByIdAsync(int id);
    }
}
