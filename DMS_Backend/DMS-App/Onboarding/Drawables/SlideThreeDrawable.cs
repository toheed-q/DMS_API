namespace DMS_App.Onboarding.Drawables;

/// <summary>
/// Slide 3 — "Works without internet". Also the final Get Started frame.
///
/// The phone keeps working while the signal dies. A DMS delivery van sits on the road
/// behind it; a cloud floats above.
///
/// Loop (3s): signal greys out -> offline badge pops -> the three saved order rows tick
/// green one by one -> a dashed sync line marches from the phone to the cloud while a
/// sync arrow rotates -> the cloud gets a green tick -> the footer runs
/// "3 orders saved offline" -> "Syncing 3 orders…" -> "All 3 orders synced".
/// T = 1 is the fully-synced frame.
/// </summary>
public sealed class SlideThreeDrawable : SlideDrawableBase
{
    public override double LoopSeconds => 3.0;

    // Cached geometry — Draw() must not allocate, and PathF has no Clear().
    // The sync line is built once at full length and revealed by clipping, which
    // is also how the dashes stay continuous instead of restarting per segment.
    private readonly PathF _syncLine;
    private readonly PathF _arrowHead;
    private readonly PathF _cab;
    private readonly PathF _glass;
    private readonly float[] _dash = [5f, 4f];

    private const float PhoneX = 96f, PhoneY = 58f, PhoneW = 142f, PhoneH = 244f;
    private const float ScrX = PhoneX + 10f, ScrY = PhoneY + 12f, ScrW = PhoneW - 20f, ScrH = PhoneH - 24f;

    // Cloud + the sync path that reaches it.
    private const float CloudCx = 292f, CloudCy = 62f;
    private const float LineX0 = PhoneX + PhoneW - 4f, LineY0 = PhoneY + 34f;
    private const float LineCx = 268f, LineCy = 118f;   // control point — bows the line out

    // Phase windows
    private const float SignalS = 0.00f, SignalE = 0.12f;
    private const float BadgeS = 0.12f, BadgeE = 0.22f;
    private const float SyncS = 0.48f, SyncE = 0.75f;
    private const float CloudTickS = 0.75f, CloudTickE = 0.85f;

    private static readonly (float S, float E)[] RowTicks =
    [
        (0.22f, 0.28f),
        (0.30f, 0.36f),
        (0.38f, 0.44f),
    ];

    private static readonly (string Shop, string Items, string Total)[] Orders =
    [
        ("Ahmed Kiryana",  "6 items",  "Rs 24,850"),
        ("Bilal Store",    "3 items",  "Rs 9,400"),
        ("Rehman General", "11 items", "Rs 41,200"),
    ];

    public SlideThreeDrawable()
    {
        const float vx = 214f, vy = 250f;

        _cab = new PathF();
        _cab.MoveTo(vx - 44f, vy + 56f);
        _cab.LineTo(vx - 44f, vy + 26f);
        _cab.LineTo(vx - 28f, vy + 8f);
        _cab.LineTo(vx, vy + 8f);
        _cab.LineTo(vx, vy + 56f);
        _cab.Close();

        _glass = new PathF();
        _glass.MoveTo(vx - 38f, vy + 28f);
        _glass.LineTo(vx - 26f, vy + 14f);
        _glass.LineTo(vx - 6f, vy + 14f);
        _glass.LineTo(vx - 6f, vy + 28f);
        _glass.Close();

        _arrowHead = new PathF();
        _arrowHead.MoveTo(6f, -5f);
        _arrowHead.LineTo(1f, -6.5f);
        _arrowHead.LineTo(5.5f, -0.5f);
        _arrowHead.Close();

        // Full-length sync path, sampled once.
        _syncLine = new PathF();
        const int steps = 32;
        for (var i = 0; i <= steps; i++)
        {
            var (x, y) = SyncPoint((float)i / steps);
            if (i == 0) _syncLine.MoveTo(x, y);
            else _syncLine.LineTo(x, y);
        }
    }

    /// <summary>Quadratic bezier from the phone's edge to the cloud. Monotonic in x.</summary>
    private static (float X, float Y) SyncPoint(float t)
    {
        var mt = 1f - t;
        var x = mt * mt * LineX0 + 2f * mt * t * LineCx + t * t * CloudCx;
        var y = mt * mt * LineY0 + 2f * mt * t * LineCy + t * t * (CloudCy + 12f);
        return (x, y);
    }

    protected override void DrawSlide(ICanvas canvas)
    {
        var p = Palette;

        DrawRoadAndVan(canvas, p);
        DrawSyncLine(canvas, p);
        DrawCloud(canvas, p);
        DrawPhone(canvas, p);
    }

    // ---------------------------------------------------------------- road / van

    private void DrawRoadAndVan(ICanvas canvas, Palette p)
    {
        // Road
        FillRoundRect(canvas, 0f, 306f, 358f, 74f, 0f, p.SurfaceAlt);
        canvas.StrokeColor = p.Outline;
        canvas.StrokeSize = 2f;
        canvas.StrokeDashPattern = [12f, 10f];
        canvas.DrawLine(0f, 352f, 358f, 352f);
        canvas.StrokeDashPattern = null;

        // Van — sits behind the phone, to the right.
        const float vx = 214f, vy = 250f;

        // Cargo body
        FillRoundRect(canvas, vx, vy, 108f, 56f, 6f, p.Primary);

        // Cab
        canvas.FillColor = p.PrimaryDeep;
        canvas.FillPath(_cab);

        // Windscreen
        canvas.FillColor = p.IsDark ? Color.FromArgb("#90CAF9").WithAlpha(0.5f) : Color.FromArgb("#BBDEFB");
        canvas.FillPath(_glass);

        // DMS panel on the side
        FillRoundRect(canvas, vx + 16f, vy + 14f, 74f, 26f, 4f, Colors.White.WithAlpha(p.IsDark ? 0.16f : 0.9f));
        Text(canvas, "DMS", vx + 16f, vy + 14f, 74f, 26f, p.PrimaryDeep, 15f, bold: true);

        // Wheels
        canvas.FillColor = p.IsDark ? Color.FromArgb("#101418") : Color.FromArgb("#37474F");
        canvas.FillCircle(vx - 26f, vy + 58f, 13f);
        canvas.FillCircle(vx + 78f, vy + 58f, 13f);
        canvas.FillColor = p.Muted;
        canvas.FillCircle(vx - 26f, vy + 58f, 5f);
        canvas.FillCircle(vx + 78f, vy + 58f, 5f);
    }

    // ---------------------------------------------------------------- cloud

    private void DrawCloud(ICanvas canvas, Palette p)
    {
        var cloudColor = p.IsDark ? p.SurfaceAlt : Color.FromArgb("#E3ECF5");

        canvas.FillColor = cloudColor;
        canvas.FillCircle(CloudCx - 20f, CloudCy + 4f, 16f);
        canvas.FillCircle(CloudCx, CloudCy - 8f, 21f);
        canvas.FillCircle(CloudCx + 22f, CloudCy + 2f, 17f);
        canvas.FillRoundedRectangle(CloudCx - 34f, CloudCy + 2f, 70f, 20f, 10f);

        // Green tick once the sync completes.
        var s = Spring(Phase(CloudTickS, CloudTickE));
        if (s <= 0.01f) return;

        canvas.SaveState();
        canvas.Translate(CloudCx + 26f, CloudCy + 18f);
        canvas.Scale(s, s);
        canvas.SetShadow(new SizeF(0f, 2f), 8f, p.Success.WithAlpha(0.5f));
        TickBadge(canvas, 0f, 0f, 13f, p.Success, Colors.White);
        canvas.SetShadow(SizeF.Zero, 0f, Colors.Transparent);
        canvas.RestoreState();
    }

    // ---------------------------------------------------------------- sync line

    private void DrawSyncLine(ICanvas canvas, Palette p)
    {
        var progress = EaseInOutCubic(Phase(SyncS, SyncE));
        if (progress <= 0.005f) return;

        var (headX, headY) = SyncPoint(progress);

        // Reveal the pre-built path by clipping to the head. The path is monotonic
        // in x, so an x-clip grows the line toward the cloud — and the dash phase
        // stays continuous, which a per-segment redraw would break.
        canvas.SaveState();
        canvas.ClipRectangle(0f, 0f, headX, DesignHeight);

        canvas.StrokeColor = p.PrimaryLight;
        canvas.StrokeSize = 2.5f;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeDashPattern = _dash;
        canvas.StrokeDashOffset = -Phase(SyncS, SyncE) * 60f;   // marching dashes
        canvas.DrawPath(_syncLine);
        canvas.StrokeDashPattern = null;
        canvas.StrokeDashOffset = 0f;

        canvas.RestoreState();

        DrawSyncArrow(canvas, p, progress, headX, headY);
    }

    /// <summary>The rotating sync glyph, riding the head of the dashed line.</summary>
    private void DrawSyncArrow(ICanvas canvas, Palette p, float progress, float x, float y)
    {
        // Fades out as it merges into the cloud.
        var alpha = progress < 0.9f ? 1f : Clamp01((1f - progress) / 0.1f);
        if (alpha <= 0.01f) return;

        canvas.SaveState();
        canvas.Alpha = alpha;
        canvas.Translate(x, y);
        canvas.Rotate(Phase(SyncS, SyncE) * 720f);

        canvas.FillColor = p.Primary;
        canvas.FillCircle(0f, 0f, 11f);

        canvas.StrokeColor = Colors.White;
        canvas.StrokeSize = 1.8f;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.DrawArc(-6f, -6f, 12f, 12f, 40f, 300f, true, false);

        canvas.FillColor = Colors.White;
        canvas.FillPath(_arrowHead);

        canvas.RestoreState();
    }

    // ---------------------------------------------------------------- phone

    private void DrawPhone(ICanvas canvas, Palette p)
    {
        canvas.SetShadow(new SizeF(0f, 6f), 14f, Colors.Black.WithAlpha(p.IsDark ? 0.5f : 0.18f));
        FillRoundRect(canvas, PhoneX, PhoneY, PhoneW, PhoneH, 16f, p.IsDark ? Color.FromArgb("#101418") : Color.FromArgb("#37474F"));
        canvas.SetShadow(SizeF.Zero, 0f, Colors.Transparent);

        FillRoundRect(canvas, ScrX, ScrY, ScrW, ScrH, 10f, p.Surface);

        DrawSignalBars(canvas, p);
        DrawOfflineBadge(canvas, p);
        DrawOrderRows(canvas, p);
        DrawFooter(canvas, p);
    }

    private void DrawSignalBars(ICanvas canvas, Palette p)
    {
        // Bars grey out; then a slash is struck through them.
        var dead = EaseOutCubic(Phase(SignalS, SignalE));
        var barColor = Mix(p.Primary, p.Muted, dead);

        var bx = ScrX + 8f;
        for (var i = 0; i < 4; i++)
        {
            var h = 4f + i * 3f;
            canvas.FillColor = barColor;
            canvas.FillRoundedRectangle(bx + i * 6f, ScrY + 18f - h, 4f, h, 1.2f);
        }

        if (dead > 0.15f)
        {
            canvas.StrokeColor = p.Danger.WithAlpha(dead);
            canvas.StrokeSize = 2f;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.DrawLine(bx - 3f, ScrY + 19f, bx + 3f + 22f * dead, ScrY + 19f - 17f * dead);
        }

        Text(canvas, "12:41", ScrX + ScrW - 34f, ScrY + 4f, 28f, 12f, p.TextSecondary, 7f, bold: true, HorizontalAlignment.Right);
    }

    private void DrawOfflineBadge(ICanvas canvas, Palette p)
    {
        var s = EaseOutBack(Phase(BadgeS, BadgeE));
        if (s <= 0.01f) return;

        var w = 84f;
        var cx = ScrX + ScrW / 2f;

        canvas.SaveState();
        canvas.Translate(cx, ScrY + 34f);
        canvas.Scale(s, s);

        FillRoundRect(canvas, -w / 2f, -9f, w, 18f, 9f, p.Warning.WithAlpha(0.18f));
        canvas.FillColor = p.Warning;
        canvas.FillCircle(-w / 2f + 10f, 0f, 3f);
        Text(canvas, "Offline mode", -w / 2f + 17f, -9f, w - 22f, 18f, p.Warning, 7.5f, bold: true, HorizontalAlignment.Left);

        canvas.RestoreState();
    }

    private void DrawOrderRows(ICanvas canvas, Palette p)
    {
        for (var i = 0; i < 3; i++)
        {
            var rowY = ScrY + 54f + i * 34f;
            var (shop, items, total) = Orders[i];

            var tick = Spring(Phase(RowTicks[i].S, RowTicks[i].E));
            var done = tick > 0.02f;

            FillRoundRect(canvas, ScrX + 6f, rowY, ScrW - 12f, 30f, 6f,
                done ? Mix(p.SurfaceAlt, p.Success.WithAlpha(0.14f), Clamp01(tick)) : p.SurfaceAlt);

            Text(canvas, shop, ScrX + 14f, rowY + 4f, 76f, 11f, p.Text, 7.5f, bold: true, HorizontalAlignment.Left);
            Text(canvas, items, ScrX + 14f, rowY + 16f, 40f, 10f, p.TextSecondary, 6.5f, align: HorizontalAlignment.Left);
            Text(canvas, total, ScrX + 56f, rowY + 16f, 48f, 10f, p.TextSecondary, 6.5f, align: HorizontalAlignment.Left);

            // Saved-offline tick
            if (tick <= 0.01f)
            {
                canvas.StrokeColor = p.Muted;
                canvas.StrokeSize = 1.4f;
                canvas.DrawCircle(ScrX + ScrW - 20f, rowY + 15f, 7f);
            }
            else
            {
                canvas.SaveState();
                canvas.Translate(ScrX + ScrW - 20f, rowY + 15f);
                canvas.Scale(tick, tick);
                TickBadge(canvas, 0f, 0f, 8f, p.Success, Colors.White);
                canvas.RestoreState();
            }
        }
    }

    private void DrawFooter(ICanvas canvas, Palette p)
    {
        // The status line tells the whole story in words.
        string status;
        Color color;

        if (T < SyncS)
        {
            status = "3 orders saved offline";
            color = p.TextSecondary;
        }
        else if (T < CloudTickE)
        {
            status = "Syncing 3 orders…";
            color = p.Primary;
        }
        else
        {
            status = "All 3 orders synced";
            color = p.Success;
        }

        var fy = ScrY + ScrH - 22f;
        FillRoundRect(canvas, ScrX + 6f, fy, ScrW - 12f, 18f, 6f, p.SurfaceAlt);

        canvas.FillColor = color;
        canvas.FillCircle(ScrX + 16f, fy + 9f, 3f);
        Text(canvas, status, ScrX + 23f, fy, ScrW - 30f, 18f, color, 7f, bold: true, HorizontalAlignment.Left);
    }
}
