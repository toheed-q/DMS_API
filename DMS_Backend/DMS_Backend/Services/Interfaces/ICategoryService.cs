using DMS_Backend.Common;
using DMS_Backend.Contracts;

namespace DMS_Backend.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync();
        Task<Result<CategoryDto>> GetByIdAsync(int id);
    }
}
