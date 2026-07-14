using UIKit;

namespace DMS_App.Onboarding.Services;

public partial class MotionSettings
{
    private partial bool GetReduceMotion()
    {
        try
        {
            return UIAccessibility.IsReduceMotionEnabled;
        }
        catch
        {
            return false;
        }
    }
}
