using DMS_Backend.Common;
using DMS_Backend.Contracts;

namespace DMS_Backend.Services.Interfaces
{
    /// <summary>Read access to the product catalog.</summary>
    public interface IProductService
    {
        Task<Result<PagedResult<ProductDto>>> GetProductsAsync(PaginationQuery query);
        Task<Result<ProductDto>> GetByIdAsync(int id);
    }
}
