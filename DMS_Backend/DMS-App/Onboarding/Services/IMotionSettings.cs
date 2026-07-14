namespace DMS_App.Onboarding.Services;

/// <summary>
/// Whether the OS is asking us to cut animation. MAUI has no cross-platform API
/// for this, so the real answer comes from a per-platform partial.
/// </summary>
public interface IMotionSettings
{
    /// <summary>True when the user has turned animation off system-wide.</summary>
    bool ReduceMotion { get; }
}

public partial class MotionSettings : IMotionSettings
{
    public bool ReduceMotion => GetReduceMotion();

    private partial bool GetReduceMotion();
}
