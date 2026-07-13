namespace DMS_Backend.Services.Interfaces
{
    /// <summary>Tax percentages configured by the business (Settings screen).</summary>
    public record TaxSettings(
        decimal GstPercent,
        decimal AdvanceTaxPercent,
        decimal SalesTaxPercent,
        decimal OtherTaxPercent);

    public interface ISystemSettingsService
    {
        /// <summary>Reads the configured tax rates. Server-side source of truth —
        /// clients never send tax percentages.</summary>
        Task<TaxSettings> GetTaxSettingsAsync();
    }
}
