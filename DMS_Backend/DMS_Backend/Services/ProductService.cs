using DMS_Backend.Common;
using DMS_Backend.Contracts;
using DMS_Backend.Data;
using DMS_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMS_Backend.Services
{
    /// <summary>
    /// Product catalog reads. Uses projection (Select) so the heavy image blob is
    /// never loaded for lists, plus server-side pagination — stays fast with a
    /// very large catalog.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _db;

        public ProductService(ApplicationDbContext db) => _db = db;

        public async Task<Result<PagedResult<ProductDto>>> GetProductsAsync(PaginationQuery query)
        {
            var products = _db.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim();
                products = products.Where(p =>
                    p.Name.Contains(term) ||
                    (p.CompanyName != null && p.CompanyName.Contains(term)));
            }

            var totalCount = await products.CountAsync();

            var items = await products
                .OrderBy(p => p.Name)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new ProductDto
                {
                    Id = p.ProductsId,
                    Name = p.Name,
                    CompanyName = p.CompanyName,
                    Size = p.Size,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : null,
                    CompanyPrice = p.CompanyPrice,
                    RetailPrice = p.RetailPrice,
                    TradePrice = p.TradePrice,
                    AvailableStock = p.AvailableStock,
                    HasImage = p.ProductImage != null
                })
                .ToListAsync();

            return Result<PagedResult<ProductDto>>.Success(new PagedResult<ProductDto>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            });
        }

        public async Task<Result<ProductDto>> GetByIdAsync(int id)
        {
            var product = await _db.Products
                .AsNoTracking()
                .Where(p => p.ProductsId == id)
                .Select(p => new ProductDto
                {
                    Id = p.ProductsId,
                    Name = p.Name,
                    CompanyName = p.CompanyName,
                    Size = p.Size,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : null,
                    CompanyPrice = p.CompanyPrice,
                    RetailPrice = p.RetailPrice,
                    TradePrice = p.TradePrice,
                    AvailableStock = p.AvailableStock,
                    HasImage = p.ProductImage != null
                })
                .FirstOrDefaultAsync();

            return product is null
                ? Result<ProductDto>.Failure($"Product {id} was not found.", ErrorType.NotFound)
                : Result<ProductDto>.Success(product);
        }
    }
}
