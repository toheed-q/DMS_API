using System.Text;
using System.Threading.RateLimiting;
using DMS_Backend.Data;
using DMS_Backend.Services;
using DMS_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DMS_Backend.Extensions
{
    /// <summary>
    /// Groups service registration so Program.cs stays declarative and readable.
    /// Each concern (database, app services, auth, rate limiting) lives in its own
    /// method — new services get added in one obvious place (DRY / SRP).
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlDatabase(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
            return services;
        }

        /// <summary>Registers business/domain services behind their interfaces (DIP).
        /// Add every new I*Service -> *Service mapping here.</summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<ITokenService, JwtTokenService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IReceivingService, ReceivingService>();
            services.AddScoped<ISystemSettingsService, SystemSettingsService>();
            services.AddScoped<IBillService, BillService>();
            services.AddScoped<IReturnService, ReturnService>();
            services.AddScoped<ILedgerService, LedgerService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IShopService, ShopService>();
            services.AddScoped<IRouteService, RouteService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ISalesmanService, SalesmanService>();
            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<JwtOptions>(config.GetSection("Jwt"));
            var jwt = config.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
                });
            services.AddAuthorization();
            return services;
        }

        /// <summary>A strict "auth" policy protects login/brute-force endpoints; a
        /// relaxed global limiter (per client IP) guards the rest of the API.</summary>
        public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration config)
        {
            var authCfg = config.GetSection("RateLimiting:Auth");
            var globalCfg = config.GetSection("RateLimiting:Global");

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddFixedWindowLimiter("auth", o =>
                {
                    o.PermitLimit = authCfg.GetValue("PermitLimit", 5);
                    o.Window = TimeSpan.FromSeconds(authCfg.GetValue("WindowSeconds", 60));
                    o.QueueLimit = 0;
                });

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = globalCfg.GetValue("PermitLimit", 100),
                            Window = TimeSpan.FromSeconds(globalCfg.GetValue("WindowSeconds", 60)),
                            QueueLimit = 0
                        }));
            });
            return services;
        }
    }
}
