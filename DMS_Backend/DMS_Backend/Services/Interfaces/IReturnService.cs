using DMS_Backend.Common;
using DMS_Backend.Contracts;

namespace DMS_Backend.Services.Interfaces
{
    /// <summary>Customer returns, attributed to a salesman.</summary>
    public interface IReturnService
    {
        Task<Result<ReturnDto>> CreateAsync(CreateReturnRequest request);

        /// <summary>Paginated returns, filterable by salesman/shop/date, with the
        /// aggregate total for the filtered set.</summary>
        Task<Result<ReturnsPagedResult>> GetReturnsAsync(ReturnQuery query);
    }
}
