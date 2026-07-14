// Microsoft.Maui.Font and Microsoft.Maui.Graphics.Font are both in scope via the
// MAUI implicit usings. Canvases want the Graphics one.
using GFont = Microsoft.Maui.Graphics.Font;

namespace DMS_App.Onboarding.Drawables;

/// <summary>
/// Shared plumbing for the three illustration drawables.
///
/// Each drawable is authored in a fixed 358 x 380 design space and letterboxed
/// into whatever the GraphicsView actually gets, so the art holds from 360dp to
/// 430dp phones without a second layout.
///
/// <para><b>T</b> is the loop position, 0 to 1. Author every frame so that
/// <c>T == 1</c> is the correct *static end state* — then reduce-motion is just
/// "freeze T at 1 and never tick", and no second set of drawing code exists.</para>
/// </summary>
public abstract class SlideDrawableBase : IDrawable
{
    public const float DesignWidth = 358f;
    public const float DesignHeight = 380f;

    /// <summary>Loop position, 0 to 1. Driven by the page's single dispatcher timer.</summary>
    public float T { get; set; }

    public Palette Palette { get; set; } = Palette.Light;

    /// <summary>How long one loop of this slide runs.</summary>
    public abstract double LoopSeconds { get; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // Map the 358x380 design box into the real rect, preserving aspect.
        var scale = Math.Min(dirtyRect.Width / DesignWidth, dirtyRect.Height / DesignHeight);
        var dx = dirtyRect.X + (dirtyRect.Width - DesignWidth * scale) / 2f;
        var dy = dirtyRect.Y + (dirtyRect.Height - DesignHeight * scale) / 2f;

        canvas.SaveState();
        canvas.Translate(dx, dy);
        canvas.Scale(scale, scale);

        DrawSlide(canvas);

        canvas.RestoreState();
    }

    /// <summary>Draw in the 358 x 380 design space. Cache paths/paints in fields — no allocation here.</summary>
    protected abstract void DrawSlide(ICanvas canvas);

    // ---- timing helpers -------------------------------------------------

    /// <summary>Progress (0..1) of T through the window [start, end]. Clamped outside it.</summary>
    protected float Phase(float start, float end)
    {
        if (T <= start) return 0f;
        if (T >= end) return 1f;
        return (T - start) / (end - start);
    }

    protected static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

    protected static float Lerp(float a, float b, float t) => a + (b - a) * t;

    protected static Color Mix(Color a, Color b, float t)
    {
        t = Clamp01(t);
        return new Color(
            a.Red + (b.Red - a.Red) * t,
            a.Green + (b.Green - a.Green) * t,
            a.Blue + (b.Blue - a.Blue) * t,
            a.Alpha + (b.Alpha - a.Alpha) * t);
    }

    protected static float EaseOutCubic(float t) => 1f - MathF.Pow(1f - Clamp01(t), 3f);

    protected static float EaseInCubic(float t) => MathF.Pow(Clamp01(t), 3f);

    protected static float EaseInOutCubic(float t)
    {
        t = Clamp01(t);
        return t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f;
    }

    /// <summary>Overshoot-and-settle, for badge/tick pops. Lands exactly on 1 at t=1.</summary>
    protected static float EaseOutBack(float t, float overshoot = 1.7f)
    {
        t = Clamp01(t);
        var c1 = overshoot;
        var c3 = c1 + 1f;
        return 1f + c3 * MathF.Pow(t - 1f, 3f) + c1 * MathF.Pow(t - 1f, 2f);
    }

    /// <summary>Damped spring (stiffness ~180 / damping ~20 feel). Settles on 1.</summary>
    protected static float Spring(float t)
    {
        t = Clamp01(t);
        if (t >= 1f) return 1f;
        return 1f - MathF.Exp(-7f * t) * MathF.Cos(9f * t);
    }

    /// <summary>A single bounce that lands (used for the coin drop).</summary>
    protected static float EaseOutBounce(float t)
    {
        t = Clamp01(t);
        const float n1 = 7.5625f, d1 = 2.75f;
        if (t < 1f / d1) return n1 * t * t;
        if (t < 2f / d1) { t -= 1.5f / d1; return n1 * t * t + 0.75f; }
        if (t < 2.5f / d1) { t -= 2.25f / d1; return n1 * t * t + 0.9375f; }
        t -= 2.625f / d1;
        return n1 * t * t + 0.984375f;
    }

    /// <summary>Rises 0 -> 1 -> 0 across the window. For one-shot glows/highlights.</summary>
    protected float Pulse(float start, float end)
    {
        var p = Phase(start, end);
        return MathF.Sin(p * MathF.PI);
    }

    // ---- drawing helpers ------------------------------------------------

    protected static void FillRoundRect(ICanvas canvas, float x, float y, float w, float h, float r, Color fill)
    {
        canvas.FillColor = fill;
        canvas.FillRoundedRectangle(x, y, w, h, r);
    }

    protected static void StrokeRoundRect(ICanvas canvas, float x, float y, float w, float h, float r, Color stroke, float thickness = 1f)
    {
        canvas.StrokeColor = stroke;
        canvas.StrokeSize = thickness;
        canvas.DrawRoundedRectangle(x, y, w, h, r);
    }

    /// <summary>Centred label. Keeps DrawString's box maths in one place.</summary>
    protected static void Text(ICanvas canvas, string text, float x, float y, float w, float h,
        Color color, float size, bool bold = false,
        HorizontalAlignment align = HorizontalAlignment.Center)
    {
        canvas.FontColor = color;
        canvas.FontSize = size;
        canvas.Font = bold ? GFont.DefaultBold : GFont.Default;
        canvas.DrawString(text, x, y, w, h, align, VerticalAlignment.Center);
    }

    /// <summary>The green "done" tick badge shared by slides 2 and 3.</summary>
    protected static void TickBadge(ICanvas canvas, float cx, float cy, float radius, Color fill, Color tick)
    {
        canvas.FillColor = fill;
        canvas.FillCircle(cx, cy, radius);

        canvas.StrokeColor = tick;
        canvas.StrokeSize = MathF.Max(1.5f, radius * 0.22f);
        canvas.StrokeLineCap = LineCap.Round;

        var p = new PathF();
        p.MoveTo(cx - radius * 0.42f, cy + radius * 0.02f);
        p.LineTo(cx - radius * 0.10f, cy + radius * 0.34f);
        p.LineTo(cx + radius * 0.46f, cy - radius * 0.34f);
        canvas.DrawPath(p);
    }
}
