namespace DMS_App.Onboarding.Drawables;

/// <summary>
/// Slide 3 — "Know your numbers".
///
/// The phone shows the salesman's live performance dashboard: today's sales, a weekly
/// bar chart, collected vs outstanding, and route progress. A daily-target ring floats
/// to the left; an upward trend arrow to the right reinforces the growth story.
///
/// Loop (3s): today's sales count up to Rs 24,850 -> the weekly bars grow in one by
/// one -> collected/outstanding cards fade in -> the route bar fills to 8/12 shops ->
/// the target ring sweeps to 92% and a star pops. T = 1 is the settled dashboard,
/// which is exactly the reduce-motion static frame.
/// </summary>
public sealed class SlideThreeDrawable : SlideDrawableBase
{
    public override double LoopSeconds => 3.0;

    // Cached geometry — Draw() must not allocate, and PathF has no Clear().
    private readonly PathF _trendLine;
    private readonly PathF _trendHead;
    private readonly PathF _star;

    private const float PhoneX = 104f, PhoneY = 16f, PhoneW = 150f, PhoneH = 348f;
    private const float ScrX = PhoneX + 10f, ScrY = PhoneY + 12f, ScrW = PhoneW - 20f, ScrH = PhoneH - 24f;

    private const float TodaysSales = 24850f;
    private const float Collected = 18600f;
    private const float Outstanding = 6250f;
    private const int ShopsVisited = 8, ShopsTotal = 12;
    private const float TargetPercent = 0.92f;

    // Weekly bars: relative heights, last one = today (highlighted).
    private static readonly float[] BarHeights = [0.45f, 0.62f, 0.50f, 0.74f, 0.66f, 0.92f];
    private static readonly string[] BarDays = ["M", "T", "W", "T", "F", "S"];

    // Target-ring centre (floats left of the phone).
    private const float RingCx = 52f, RingCy = 232f, RingR = 40f;

    // Phase windows
    private const float CountS = 0.00f, CountE = 0.30f;
    private const float StatsS = 0.30f, StatsE = 0.45f;
    private const float RouteS = 0.42f, RouteE = 0.68f;
    private const float RingS = 0.30f, RingE = 0.72f;
    private const float StarS = 0.72f, StarE = 0.86f;
    private const float TrendS = 0.15f, TrendE = 0.58f;

    public SlideThreeDrawable()
    {
        // Upward trend polyline, floating right of the phone.
        _trendLine = new PathF();
        _trendLine.MoveTo(272f, 300f);
        _trendLine.LineTo(298f, 268f);
        _trendLine.LineTo(316f, 286f);
        _trendLine.LineTo(348f, 232f);

        _trendHead = new PathF();
        _trendHead.MoveTo(348f, 232f);
        _trendHead.LineTo(332f, 238f);
        _trendHead.MoveTo(348f, 232f);
        _trendHead.LineTo(344f, 250f);

        _star = BuildStar(5, 7.5f, 3.4f);
    }

    protected override void DrawSlide(ICanvas canvas)
    {
        var p = Palette;

        DrawTrend(canvas, p);
        DrawTargetRing(canvas, p);
        DrawPhone(canvas, p);
    }

    // ---------------------------------------------------------------- trend

    private void DrawTrend(ICanvas canvas, Palette p)
    {
        var grow = EaseOutCubic(Phase(TrendS, TrendE));
        if (grow <= 0.01f) return;

        canvas.SaveState();
        // Reveal left-to-right by clipping (the polyline is monotonic in x).
        canvas.ClipRectangle(260f, 210f, (348f - 272f + 8f) * grow + 12f, 110f);

        canvas.StrokeColor = p.Success;
        canvas.StrokeSize = 3f;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;
        canvas.DrawPath(_trendLine);
        canvas.DrawPath(_trendHead);
        canvas.RestoreState();

        // Dots at the vertices, appearing with the line.
        if (grow > 0.85f)
        {
            canvas.FillColor = p.Success;
            canvas.FillCircle(272f, 300f, 2.5f);
            canvas.FillCircle(298f, 268f, 2.5f);
            canvas.FillCircle(316f, 286f, 2.5f);
        }
    }

    // ---------------------------------------------------------------- target ring

    private void DrawTargetRing(ICanvas canvas, Palette p)
    {
        var sweep = EaseOutCubic(Phase(RingS, RingE));
        var frac = TargetPercent * sweep;

        // Track
        canvas.StrokeColor = p.Outline;
        canvas.StrokeSize = 7f;
        canvas.DrawCircle(RingCx, RingCy, RingR);

        // Progress arc — starts at the top, sweeps clockwise.
        if (frac > 0.001f)
        {
            canvas.StrokeColor = p.Primary;
            canvas.StrokeSize = 7f;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.DrawArc(
                RingCx - RingR, RingCy - RingR, RingR * 2f, RingR * 2f,
                90f, 90f - 360f * frac, clockwise: true, closed: false);
        }

        // Percentage + label
        var shown = (int)MathF.Round(TargetPercent * sweep * 100f);
        Text(canvas, $"{shown}%", RingCx - RingR, RingCy - 12f, RingR * 2f, 18f, p.Text, 16f, bold: true);
        Text(canvas, "of target", RingCx - RingR, RingCy + 6f, RingR * 2f, 12f, p.TextSecondary, 7f);

        // Star pops when the ring lands.
        var s = Spring(Phase(StarS, StarE));
        if (s <= 0.01f) return;

        canvas.SaveState();
        canvas.Translate(RingCx + RingR * 0.72f, RingCy - RingR * 0.72f);
        canvas.Scale(s, s);
        canvas.SetShadow(new SizeF(0f, 2f), 8f, p.Warning.WithAlpha(0.5f));
        canvas.FillColor = p.Warning;
        canvas.FillCircle(0f, 0f, 12f);
        canvas.SetShadow(SizeF.Zero, 0f, Colors.Transparent);
        canvas.FillColor = Colors.White;
        canvas.FillPath(_star);
        canvas.RestoreState();
    }

    // ---------------------------------------------------------------- phone

    private void DrawPhone(ICanvas canvas, Palette p)
    {
        canvas.SetShadow(new SizeF(0f, 6f), 14f, Colors.Black.WithAlpha(p.IsDark ? 0.5f : 0.18f));
        FillRoundRect(canvas, PhoneX, PhoneY, PhoneW, PhoneH, 16f, p.IsDark ? Color.FromArgb("#101418") : Color.FromArgb("#37474F"));
        canvas.SetShadow(SizeF.Zero, 0f, Colors.Transparent);

        FillRoundRect(canvas, ScrX, ScrY, ScrW, ScrH, 10f, p.Surface);

        // Header
        Text(canvas, "My Performance", ScrX + 8f, ScrY + 6f, 110f, 12f, p.Text, 9f, bold: true, HorizontalAlignment.Left);
        Text(canvas, "Today · Route 4", ScrX + 8f, ScrY + 18f, 90f, 10f, p.TextSecondary, 7f, align: HorizontalAlignment.Left);

        DrawHeroSales(canvas, p);
        DrawBarChart(canvas, p);
        DrawMiniStats(canvas, p);
        DrawRouteProgress(canvas, p);
    }

    private void DrawHeroSales(ICanvas canvas, Palette p)
    {
        var t = EaseOutCubic(Phase(CountS, CountE));
        var value = Lerp(0f, TodaysSales, t);

        var y = ScrY + 32f;
        FillRoundRect(canvas, ScrX + 6f, y, ScrW - 12f, 46f, 8f, p.Primary.WithAlpha(0.12f));

        Text(canvas, "TODAY'S SALES", ScrX + 14f, y + 7f, 90f, 10f, p.TextSecondary, 6.5f, bold: true, HorizontalAlignment.Left);
        Text(canvas, $"Rs {value:N0}", ScrX + 14f, y + 21f, ScrW - 28f, 20f, p.Text, 16f, bold: true, HorizontalAlignment.Left);

        // "+12%" up chip, top-right of the hero card.
        var chipX = ScrX + ScrW - 44f;
        FillRoundRect(canvas, chipX, y + 8f, 34f, 15f, 7.5f, p.Success.WithAlpha(0.18f));
        canvas.StrokeColor = p.Success;
        canvas.StrokeSize = 1.4f;
        canvas.StrokeLineCap = LineCap.Round;
        var arrow = new PathF();
        arrow.MoveTo(chipX + 8f, y + 19f);
        arrow.LineTo(chipX + 8f, y + 12f);
        arrow.MoveTo(chipX + 5.5f, y + 14.5f);
        arrow.LineTo(chipX + 8f, y + 12f);
        arrow.LineTo(chipX + 10.5f, y + 14.5f);
        canvas.DrawPath(arrow);
        Text(canvas, "12%", chipX + 11f, y + 8f, 22f, 15f, p.Success, 7f, bold: true, HorizontalAlignment.Left);
    }

    private void DrawBarChart(ICanvas canvas, Palette p)
    {
        var baseline = ScrY + 150f;
        const float maxH = 52f;
        var slot = (ScrW - 20f) / BarHeights.Length;
        var barW = slot * 0.52f;

        for (var i = 0; i < BarHeights.Length; i++)
        {
            // Staggered grow-up, last bar (today) emphasised.
            var start = 0.10f + i * 0.06f;
            var g = EaseOutCubic(Phase(start, start + 0.20f));
            var h = maxH * BarHeights[i] * g;
            var x = ScrX + 12f + i * slot + (slot - barW) / 2f;
            var isToday = i == BarHeights.Length - 1;

            canvas.FillColor = isToday ? p.Primary : p.Primary.WithAlpha(0.32f);
            canvas.FillRoundedRectangle(x, baseline - h, barW, h, 2.5f);

            Text(canvas, BarDays[i], x - (slot - barW) / 2f, baseline + 3f, slot, 10f,
                isToday ? p.Primary : p.TextSecondary, 6.5f, bold: isToday);
        }
    }

    private void DrawMiniStats(ICanvas canvas, Palette p)
    {
        var t = EaseOutCubic(Phase(StatsS, StatsE));
        if (t <= 0.01f) return;

        canvas.SaveState();
        canvas.Alpha = t;

        var y = ScrY + 182f;
        var cardW = (ScrW - 12f - 6f) / 2f;

        // Collected
        FillRoundRect(canvas, ScrX + 6f, y, cardW, 34f, 6f, p.Success.WithAlpha(0.14f));
        canvas.FillColor = p.Success;
        canvas.FillCircle(ScrX + 14f, y + 11f, 3f);
        Text(canvas, "Collected", ScrX + 20f, y + 5f, cardW - 18f, 11f, p.TextSecondary, 6.5f, align: HorizontalAlignment.Left);
        Text(canvas, $"Rs {Collected:N0}", ScrX + 12f, y + 17f, cardW - 14f, 12f, p.Text, 8.5f, bold: true, HorizontalAlignment.Left);

        // Outstanding
        var x2 = ScrX + 6f + cardW + 6f;
        FillRoundRect(canvas, x2, y, cardW, 34f, 6f, p.Warning.WithAlpha(0.14f));
        canvas.FillColor = p.Warning;
        canvas.FillCircle(x2 + 8f, y + 11f, 3f);
        Text(canvas, "Outstanding", x2 + 14f, y + 5f, cardW - 12f, 11f, p.TextSecondary, 6.5f, align: HorizontalAlignment.Left);
        Text(canvas, $"Rs {Outstanding:N0}", x2 + 6f, y + 17f, cardW - 10f, 12f, p.Text, 8.5f, bold: true, HorizontalAlignment.Left);

        canvas.RestoreState();
    }

    private void DrawRouteProgress(ICanvas canvas, Palette p)
    {
        var y = ScrY + 226f;

        Text(canvas, "Route 4 progress", ScrX + 8f, y, 90f, 12f, p.Text, 7.5f, bold: true, HorizontalAlignment.Left);
        Text(canvas, $"{ShopsVisited} / {ShopsTotal} shops", ScrX + ScrW - 66f, y, 58f, 12f, p.TextSecondary, 7f, align: HorizontalAlignment.Right);

        var barY = y + 16f;
        var barW = ScrW - 16f;
        FillRoundRect(canvas, ScrX + 8f, barY, barW, 8f, 4f, p.SurfaceAlt);

        var fill = EaseOutCubic(Phase(RouteS, RouteE)) * ((float)ShopsVisited / ShopsTotal);
        if (fill > 0.001f)
            FillRoundRect(canvas, ScrX + 8f, barY, barW * fill, 8f, 4f, p.Primary);
    }

    // ---------------------------------------------------------------- helpers

    /// <summary>A filled five-point star, centred on the origin.</summary>
    private static PathF BuildStar(int points, float outer, float inner)
    {
        var path = new PathF();
        var step = MathF.PI / points;
        var angle = -MathF.PI / 2f;

        for (var i = 0; i < points * 2; i++)
        {
            var r = (i % 2 == 0) ? outer : inner;
            var x = MathF.Cos(angle) * r;
            var y = MathF.Sin(angle) * r;
            if (i == 0) path.MoveTo(x, y);
            else path.LineTo(x, y);
            angle += step;
        }

        path.Close();
        return path;
    }
}
