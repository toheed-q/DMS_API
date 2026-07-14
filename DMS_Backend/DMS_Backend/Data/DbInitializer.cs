using System.Security.Cryptography;
using DMS.Models;
using DMS_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMS_Backend.Data
{
    /// <summary>
    /// Applies pending migrations and seeds a default admin the first time the
    /// database is created, so the API is usable immediately.
    /// </summary>
    public static class DbInitializer
    {
        public static async Task InitializeAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                var db = services.GetRequiredService<ApplicationDbContext>();
                await db.Database.MigrateAsync();

                if (!await db.Users.AnyAsync())
                {
                    var cfg = services.GetRequiredService<IConfiguration>();
                    var hasher = services.GetRequiredService<IPasswordHasher>();
                    var username = cfg["Seed:AdminUsername"] ?? "admin";

                    // No hardcoded fallback: an unconfigured seed password becomes a
                    // random one, surfaced once here so the first login is still possible.
                    var configured = cfg["Seed:AdminPassword"];
                    var password = string.IsNullOrWhiteSpace(configured) ? GeneratePassword() : configured;

                    db.Users.Add(new User
                    {
                        Username = username,
                        Password = hasher.Hash(password),
                        Role = "Admin",
                        FullName = "System Administrator",
                        IsApproved = true,
                        CreatedAt = DateTime.UtcNow,
                        CanAccessPOS = true,
                        CanAccessProducts = true,
                        CanAccessStockViewer = true,
                        CanAccessShops = true,
                        CanAccessRoutes = true,
                        CanAccessCategories = true,
                        CanAccessBillsHistory = true,
                        CanAccessEmployees = true,
                        CanAccessCompanies = true
                    });
                    await db.SaveChangesAsync();

                    if (string.IsNullOrWhiteSpace(configured))
                        logger.LogWarning(
                            "Seeded admin '{Username}' with a generated password: {Password} — " +
                            "store it now and change it; it is not recoverable from the database.",
                            username, password);
                    else
                        logger.LogInformation("Seeded admin user '{Username}' from configuration", username);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database initialization failed");
                throw;
            }
        }

        /// <summary>Cryptographically random, URL-safe password for an unconfigured seed admin.</summary>
        private static string GeneratePassword() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(18))
                   .Replace('+', 'A').Replace('/', 'z').TrimEnd('=');
    }
}
