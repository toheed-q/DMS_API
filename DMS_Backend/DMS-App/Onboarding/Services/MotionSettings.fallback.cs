namespace DMS_App.Onboarding.Services;

/// <summary>Desktop targets have no reduce-motion signal we can read; assume motion is allowed.</summary>
public partial class MotionSettings
{
    private partial bool GetReduceMotion() => false;
}
