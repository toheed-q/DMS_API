using DMS_Backend.Common;
using DMS_Backend.Contracts;

namespace DMS_Backend.Services.Interfaces
{
    /// <summary>User administration (admin only).</summary>
    public interface IUserService
    {
        Task<Result<UserDto>> CreateAsync(CreateUserRequest request);
        Task<Result<PagedResult<UserDto>>> GetUsersAsync(PaginationQuery query);
    }
}
