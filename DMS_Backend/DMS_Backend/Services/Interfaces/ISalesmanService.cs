using DMS_Backend.Common;
using DMS_Backend.Contracts;

namespace DMS_Backend.Services.Interfaces
{
    public interface ISalesmanService
    {
        Task<Result<PagedResult<SalesmanDto>>> GetSalesmenAsync(PaginationQuery query, bool? activeOnly);
        Task<Result<SalesmanDto>> GetByIdAsync(int id);
    }
}
