namespace DMS_App.Onboarding.Controls;

/// <summary>
/// The gradient primary button.
///
/// Two things force this to be a custom Button rather than a style:
///  - Button.Background takes a Brush, so the gradient is a LinearGradientBrush.
///    No image, no Border hack.
///  - The press feedback is timed (90ms down, 130ms up). VisualStateManager applies
///    states instantly and cannot ease, so we drive Pressed/Released ourselves.
/// </summary>
public sealed class PrimaryCta : Button
{
    private const uint PressMs = 90;
    private const uint ReleaseMs = 130;
    private const double PressScale = 0.97;

    public PrimaryCta()
    {
        HeightRequest = 56;
        MinimumHeightRequest = 48;
        CornerRadius = 18;
        BorderWidth = 0;
        FontSize = 17;
        FontAttributes = FontAttributes.Bold;
        TextColor = Colors.White;
        Padding = new Thickness(16, 0);

        Background = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1),
            GradientStops =
            [
                new GradientStop(Color.FromArgb("#1565C0"), 0.0f),
                new GradientStop(Color.FromArgb("#1976D2"), 0.5f),
                new GradientStop(Color.FromArgb("#42A5F5"), 1.0f),
            ]
        };

        // Starts invisible; the one-shot pulse animates it in and back out.
        Shadow = new Shadow
        {
            Brush = new SolidColorBrush(Color.FromArgb("#42A5F5")),
            Offset = new Point(0, 6),
            Radius = 0,
            Opacity = 0f
        };

        Pressed += OnPressed;
        Released += OnReleased;
    }

    /// <summary>When true, press feedback is opacity-only and the pulse is skipped.</summary>
    public bool ReduceMotion { get; set; }

    private void OnPressed(object? sender, EventArgs e)
    {
        if (ReduceMotion)
        {
            Opacity = 0.85;
            return;
        }

        this.ScaleTo(PressScale, PressMs, Easing.CubicOut);
    }

    private void OnReleased(object? sender, EventArgs e)
    {
        if (ReduceMotion)
        {
            Opacity = 1;
            return;
        }

        this.ScaleTo(1d, ReleaseMs, Easing.SpringOut);
    }

    /// <summary>
    /// The one-shot "final CTA" pulse: scale 1 -> 1.025 -> 1 over 700ms, with a blue
    /// glow that appears and fades. Scale and Shadow ride the same Animation so they
    /// stay locked together.
    /// </summary>
    public void PulseOnce()
    {
        if (ReduceMotion || Shadow is null)
            return;

        this.AbortAnimation("ctaPulse");

        var shadow = Shadow;
        var pulse = new Animation();

        pulse.Add(0.0, 0.5, new Animation(v => Scale = v, 1.0, 1.025, Easing.CubicOut));
        pulse.Add(0.5, 1.0, new Animation(v => Scale = v, 1.025, 1.0, Easing.CubicIn));

        pulse.Add(0.0, 0.5, new Animation(v => shadow.Radius = (float)v, 0, 30, Easing.CubicOut));
        pulse.Add(0.5, 1.0, new Animation(v => shadow.Radius = (float)v, 30, 0, Easing.CubicIn));

        pulse.Add(0.0, 0.5, new Animation(v => shadow.Opacity = (float)v, 0f, 0.45f, Easing.CubicOut));
        pulse.Add(0.5, 1.0, new Animation(v => shadow.Opacity = (float)v, 0.45f, 0f, Easing.CubicIn));

        pulse.Commit(this, "ctaPulse", 16, 700);
    }
}
