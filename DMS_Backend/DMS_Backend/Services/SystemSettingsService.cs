using DMS_Backend.Data;
using DMS_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMS_Backend.Services
{
    /// <summary>
    /// Reads business settings from the SystemSettings key/value table — the same
    /// keys the desktop app uses, so both stay in sync.
    /// </summary>
    public class SystemSettingsService : ISystemSettingsService
    {
        private readonly ApplicationDbContext _db;

        public SystemSettingsService(ApplicationDbContext db) => _db = db;

        public async Task<TaxSettings> GetTaxSettingsAsync()
        {
            string[] keys = ["GSTPercent", "AdvanceTaxPercent", "SalesTaxPercent", "OtherTaxPercent"];

            var settings = await _db.SystemSettings
                .AsNoTracking()
                .Where(s => keys.Contains(s.SettingKey))
                .ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue);

            return new TaxSettings(
                Percent(settings, "GSTPercent"),
                Percent(settings, "AdvanceTaxPercent"),
                Percent(settings, "SalesTaxPercent"),
                Percent(settings, "OtherTaxPercent"));
        }

        private static decimal Percent(IDictionary<string, string> settings, string key)
            => settings.TryGetValue(key, out var raw) && decimal.TryParse(raw, out var value)
                ? value
                : 0m;
    }
}
