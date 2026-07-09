using DMS_Backend.Common;
using DMS_Backend.Contracts;
using DMS_Backend.Data;
using DMS_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMS_Backend.Services
{
    /// <summary>Routes are reference data (used for dropdowns/filters), returned in full.</summary>
    public class RouteService : IRouteService
    {
        private readonly ApplicationDbContext _db;

        public RouteService(ApplicationDbContext db) => _db = db;

        public async Task<Result<IReadOnlyList<RouteDto>>> GetAllAsync()
        {
            var routes = await _db.Routes.AsNoTracking()
                .OrderBy(r => r.Name)
                .Select(r => new RouteDto
                {
                    Id = r.RouteId,
                    Name = r.Name,
                    Description = r.Description,
                    ShopCount = r.Shops.Count
                })
                .ToListAsync();

            return Result<IReadOnlyList<RouteDto>>.Success(routes);
        }

        public async Task<Result<RouteDto>> GetByIdAsync(int id)
        {
            var route = await _db.Routes.AsNoTracking()
                .Where(r => r.RouteId == id)
                .Select(r => new RouteDto
                {
                    Id = r.RouteId,
                    Name = r.Name,
                    Description = r.Description,
                    ShopCount = r.Shops.Count
                })
                .FirstOrDefaultAsync();

            return route is null
                ? Result<RouteDto>.Failure($"Route {id} was not found.", ErrorType.NotFound)
                : Result<RouteDto>.Success(route);
        }
    }
}
