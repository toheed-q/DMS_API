using DMS.Models;
using DMS_Backend.Services;
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
                    var username = cfg["Seed:AdminUsername"] ?? "admin";
                    var password = cfg["Seed:AdminPassword"] ?? "admin123";

                    db.Users.Add(new User
                    {
                        Username = username,
                        Password = PasswordHasher.Hash(password),
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
                    logger.LogInformation("Seeded default admin user '{Username}'", username);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database initialization failed");
                throw;
            }
        }
    }
}
