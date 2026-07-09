namespace DMS_Backend.Services
{
    /// <summary>Strongly-typed JWT settings bound from configuration ("Jwt" section).</summary>
    public class JwtOptions
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = "DMS.Api";
        public string Audience { get; set; } = "DMS.Clients";
        public int ExpiryMinutes { get; set; } = 480; // 8 hours
    }
}
