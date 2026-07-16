namespace DMS_App.Services;

/// <summary>
/// Where the DMS API lives, per platform. The Android emulator reaches the host
/// machine at 10.0.2.2; everything else talks to localhost. Port 5300 matches the
/// API's documented dev port.
/// </summary>
public static class ApiConfig
{
    public static string BaseUrl =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5300"
            : "http://localhost:5300";
}
