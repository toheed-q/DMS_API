using DMS_Backend.Common;
using DMS_Backend.Contracts;

namespace DMS_Backend.Services.Interfaces
{
    public interface IRouteService
    {
        Task<Result<IReadOnlyList<RouteDto>>> GetAllAsync();
        Task<Result<RouteDto>> GetByIdAsync(int id);
    }
}
