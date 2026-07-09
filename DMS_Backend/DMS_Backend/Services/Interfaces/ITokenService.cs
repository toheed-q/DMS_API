using DMS.Models;

namespace DMS_Backend.Services.Interfaces
{
    /// <summary>Creates signed access tokens for authenticated users.</summary>
    public interface ITokenService
    {
        (string token, DateTime expiresAt) CreateToken(User user);
    }
}
