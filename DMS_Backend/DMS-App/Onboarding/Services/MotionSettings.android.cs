using Android.Provider;

namespace DMS_App.Onboarding.Services;

public partial class MotionSettings
{
    /// <summary>
    /// Android exposes reduce-motion as an animator duration scale: developer
    /// options (and battery saver) set it to 0 to mean "no animation".
    /// </summary>
    private partial bool GetReduceMotion()
    {
        try
        {
            var resolver = Platform.AppContext.ContentResolver;
            if (resolver is null)
                return false;

            var scale = Settings.Global.GetFloat(resolver, Settings.Global.AnimatorDurationScale, 1f);
            return scale == 0f;
        }
        catch
        {
            // Never let an accessibility probe take the app down — assume motion is fine.
            return false;
        }
    }
}
