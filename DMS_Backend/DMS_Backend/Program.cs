using System.Text;
using System.Threading.RateLimiting;
using DMS_Backend.Data;
using DMS_Backend.Middleware;
using DMS_Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

// ---------------------------------------------------------------------------
// Serilog: clean, structured logs (console + rolling daily file). Framework
// noise is trimmed so logs stay signal-rich.
// ---------------------------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/dms-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        outputTemplate:
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // ---- Configuration objects ----
    builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
    var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();

    // ---- Database (SQL Server) ----
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // ---- Auth services ----
    builder.Services.AddSingleton<JwtTokenService>();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });
    builder.Services.AddAuthorization();

    // ---- Rate limiting ----
    // A stricter "auth" policy protects login/brute-force endpoints; a relaxed
    // global limiter (per client IP) guards the rest of the API.
    var authCfg = builder.Configuration.GetSection("RateLimiting:Auth");
    var globalCfg = builder.Configuration.GetSection("RateLimiting:Global");
    builder.Services.AddRateLimiter(options =>
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

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // ---- Apply migrations + seed a default admin on first run ----
    await DbInitializer.InitializeAsync(app);

    // ---- Pipeline ----
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
        app.MapOpenApi();

    app.UseSerilogRequestLogging();   // one tidy line per request
    app.UseHttpsRedirection();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("DMS API starting up");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "DMS API terminated unexpectedly during startup");
}
finally
{
    Log.CloseAndFlush();
}
