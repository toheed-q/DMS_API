using DMS_Backend.Common;
using DMS_Backend.Contracts;
using DMS_Backend.Data;
using DMS_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMS_Backend.Services
{
    /// <summary>Customer (shop) reads: paginated, searchable, filterable by route.</summary>
    public class ShopService : IShopService
    {
        private readonly ApplicationDbContext _db;

        public ShopService(ApplicationDbContext db) => _db = db;

        public async Task<Result<PagedResult<ShopDto>>> GetShopsAsync(PaginationQuery query, int? routeId)
        {
            var shops = _db.Shops.AsNoTracking();

            if (routeId.HasValue)
                shops = shops.Where(s => s.RouteId == routeId.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim();
                shops = shops.Where(s =>
                    s.Name.Contains(term) ||
                    (s.Address != null && s.Address.Contains(term)) ||
                    (s.ContactNumber != null && s.ContactNumber.Contains(term)));
            }

            var totalCount = await shops.CountAsync();

            var items = await shops
                .OrderBy(s => s.Name)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(s => new ShopDto
                {
                    Id = s.ShopId,
                    Name = s.Name,
                    Address = s.Address,
                    ContactNumber = s.ContactNumber,
                    NTNNumber = s.NTNNumber,
                    FBRNumber = s.FBRNumber,
                    RouteId = s.RouteId,
                    RouteName = s.Route != null ? s.Route.Name : null,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return Result<PagedResult<ShopDto>>.Success(new PagedResult<ShopDto>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            });
        }

        public async Task<Result<ShopDto>> GetByIdAsync(int id)
        {
            var shop = await _db.Shops.AsNoTracking()
                .Where(s => s.ShopId == id)
                .Select(s => new ShopDto
                {
                    Id = s.ShopId,
                    Name = s.Name,
                    Address = s.Address,
                    ContactNumber = s.ContactNumber,
                    NTNNumber = s.NTNNumber,
                    FBRNumber = s.FBRNumber,
                    RouteId = s.RouteId,
                    RouteName = s.Route != null ? s.Route.Name : null,
                    CreatedAt = s.CreatedAt
                })
                .FirstOrDefaultAsync();

            return shop is null
                ? Result<ShopDto>.Failure($"Shop {id} was not found.", ErrorType.NotFound)
                : Result<ShopDto>.Success(shop);
        }
    }
}
