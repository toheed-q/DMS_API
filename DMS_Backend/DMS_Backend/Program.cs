using DMS_Backend.Data;
using DMS_Backend.Extensions;
using DMS_Backend.Middleware;
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

    // Composition root — each concern registered via its own extension (SRP/DRY).
    builder.Services
        .AddSqlDatabase(builder.Configuration)
        .AddApplicationServices()
        .AddJwtAuthentication(builder.Configuration)
        .AddApiRateLimiting(builder.Configuration);

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // Apply migrations + seed a default admin on first run.
    await DbInitializer.InitializeAsync(app);

    // ---- HTTP pipeline ----
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
