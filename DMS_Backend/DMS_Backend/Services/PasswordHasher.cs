using System.Security.Cryptography;
using System.Text;
using DMS_Backend.Services.Interfaces;

namespace DMS_Backend.Services
{
    /// <summary>
    /// Mirrors the desktop app's password scheme (SHA-256 -> lowercase hex) so
    /// existing user credentials keep working after data is migrated to the API.
    /// NOTE (improvement for later): SHA-256 without a per-user salt is weak. Plan
    /// is to upgrade to a salted BCrypt/PBKDF2 hash and re-hash on next login —
    /// isolated behind IPasswordHasher so no caller changes.
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;
            var computed = Hash(password);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computed),
                Encoding.UTF8.GetBytes(storedHash));
        }
    }
}
