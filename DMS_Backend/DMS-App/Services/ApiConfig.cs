namespace DMS_App.Services;

/// <summary>
/// Where the DMS API lives. A Release build always talks to the live server; a Debug
/// build talks to the API running on the developer's machine (the Android emulator
/// reaches that host at 10.0.2.2, everything else at localhost).
/// </summary>
public static class ApiConfig
{
    /// <summary>The deployed API. Release builds — and therefore anything installed
    /// on a real device — always use this.</summary>
    public const string ProductionUrl = "https://dms-app.runasp.net";

    private const string DevPort = "5300";

    public static string BaseUrl =>
#if DEBUG
        DeviceInfo.Platform == DevicePlatform.Android
            ? $"http://10.0.2.2:{DevPort}"
            : $"http://localhost:{DevPort}";
#else
        ProductionUrl;
#endif

    /// <summary>True when talking to a plaintext dev server, which is the only case
    /// where the Android cleartext-traffic exception is needed.</summary>
    public static bool IsDevServer => BaseUrl.StartsWith("http://", StringComparison.Ordinal);
}
