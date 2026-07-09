namespace DMS_Backend.Services.Interfaces
{
    /// <summary>Hashes and verifies user passwords. Abstracted so the algorithm
    /// can be upgraded (e.g. SHA-256 -> BCrypt) without touching callers.</summary>
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string storedHash);
    }
}
