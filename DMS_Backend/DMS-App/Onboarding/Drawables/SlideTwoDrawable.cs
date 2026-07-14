using GFont = Microsoft.Maui.Graphics.Font;

namespace DMS_App.Onboarding.Drawables;

/// <summary>
/// Slide 2 — "Collect payments instantly".
///
/// The phone shows a customer ledger (shop, route, outstanding, an overdue chip and
/// ledger rows). An open cash box sits to the left with stylised rupee notes fanning
/// out; a receipt with a green PAID stamp comes in from the right.
///
/// Notes are deliberately stylised — an authentic rupee *feeling*, never a
/// reproduction of a real banknote.
///
/// Loop (2.8s): coin drops and bounces into the box -> outstanding counts
/// Rs 85,000 -> Rs 60,000 -> the amber chip swaps to a green "Paid" chip -> the
/// receipt slides in -> a green tick badge springs onto the phone corner -> the
/// "Cash received" row highlights once. T = 1 is the settled, fully-paid frame.
/// </summary>
public sealed class SlideTwoDrawable : SlideDrawableBase
{
    public override double LoopSeconds => 2.8;

    private const float PhoneX = 110f, PhoneY = 26f, PhoneW = 142f, PhoneH = 252f;
    private const float ScrX = PhoneX + 10f, ScrY = PhoneY + 12f, ScrW = PhoneW - 20f, ScrH = PhoneH - 24f;

    private const float OutstandingStart = 85000f;
    private const float OutstandingEnd = 60000f;
    private const float Collected = 25000f;

    // Phase windows
    private const float CoinS = 0.00f, CoinE = 0.18f;
    private const float CountS = 0.18f, CountE = 0.42f;
    private const float ChipS = 0.42f, ChipE = 0.52f;
    private const float RcptS = 0.52f, RcptE = 0.66f;
    private const float TickS = 0.66f, TickE = 0.80f;
    private const float RowS = 0.80f, RowE = 0.95f;

    protected override void DrawSlide(ICanvas canvas)
    {
        var p = Palette;

        DrawReceipt(canvas, p);   // behind the phone's right edge
        DrawCashBox(canvas, p);
        DrawCoin(canvas, p);
        DrawPhone(canvas, p);
    }

    // ---------------------------------------------------------------- cash box

    private void DrawCashBox(ICanvas canvas, Palette p)
    {
        const float bx = 6f, by = 262f, bw = 102f, bh = 74f;

        // Open lid, hinged back and tilted away.
        canvas.SaveState();
        canvas.Rotate(-18f, bx + 6f, by + 6f);
        FillRoundRect(canvas, bx + 2f, by - 30f, bw - 6f, 30f, 4f, p.SurfaceAlt);
        StrokeRoundRect(canvas, bx + 2f, by - 30f, bw - 6f, 30f, 4f, p.Outline);
        canvas.RestoreState();

        // Notes fanning out of the box.
        Note(canvas, p, bx + 16f, by - 16f, -26f);
        Note(canvas, p, bx + 34f, by - 22f, -6f);
        Note(canvas, p, bx + 54f, by - 16f, 16f);

        // Box body
        FillRoundRect(canvas, bx, by, bw, bh, 6f, p.Surface);
        StrokeRoundRect(canvas, bx, by, bw, bh, 6f, p.Outline);

        // Cash tray divider + a couple of settled notes inside
        canvas.FillColor = p.SurfaceAlt;
        canvas.FillRoundedRectangle(bx + 6f, by + 8f, bw - 12f, 26f, 3f);
        canvas.FillColor = p.Cash.WithAlpha(0.75f);
        canvas.FillRoundedRectangle(bx + 10f, by + 12f, 36f, 16f, 2f);
        canvas.FillRoundedRectangle(bx + 52f, by + 12f, 36f, 16f, 2f);

        // Coin well — where the dropped coin lands.
        canvas.FillColor = p.SurfaceAlt;
        canvas.FillRoundedRectangle(bx + 6f, by + 40f, bw - 12f, 26f, 3f);
        canvas.FillColor = p.Coin.WithAlpha(0.5f);
        canvas.FillCircle(bx + 26f, by + 53f, 8f);
        canvas.FillCircle(bx + 44f, by + 53f, 8f);
    }

    /// <summary>A stylised rupee note — green, with an abstract portrait medallion. Not a real banknote.</summary>
    private static void Note(ICanvas canvas, Palette p, float x, float y, float rotation)
    {
        canvas.SaveState();
        canvas.Rotate(rotation, x + 22f, y + 12f);

        FillRoundRect(canvas, x, y, 44f, 24f, 2.5f, p.Cash);
        StrokeRoundRect(canvas, x, y, 44f, 24f, 2.5f, p.Cash.WithLuminosity(0.32f));

        canvas.FillColor = p.Cash.WithLuminosity(0.82f);
        canvas.FillCircle(x + 12f, y + 12f, 6f);            // medallion
        canvas.FillRoundedRectangle(x + 24f, y + 7f, 14f, 3f, 1.5f);
        canvas.FillRoundedRectangle(x + 24f, y + 14f, 10f, 3f, 1.5f);

        canvas.FontColor = p.Cash.WithLuminosity(0.25f);
        canvas.FontSize = 6f;
        canvas.Font = GFont.DefaultBold;
        canvas.DrawString("Rs", x + 3f, y + 15f, 12f, 8f, HorizontalAlignment.Left, VerticalAlignment.Center);

        canvas.RestoreState();
    }

    private void DrawCoin(ICanvas canvas, Palette p)
    {
        // Drops from above the box and bounces to rest in the coin well.
        var t = Phase(CoinS, CoinE);
        var y = Lerp(150f, 315f, EaseOutBounce(t));
        var x = 62f;

        // Squash a touch at the moment of impact.
        var squash = t > 0.62f ? 1f - 0.12f * MathF.Sin((t - 0.62f) / 0.38f * MathF.PI) : 1f;

        canvas.SaveState();
        canvas.Translate(x, y);
        canvas.Scale(1f / squash, squash);

        canvas.FillColor = p.Coin;
        canvas.FillCircle(0f, 0f, 10f);
        canvas.StrokeColor = p.Coin.WithLuminosity(0.36f);
        canvas.StrokeSize = 1.5f;
        canvas.DrawCircle(0f, 0f, 10f);
        canvas.DrawCircle(0f, 0f, 6f);

        canvas.FontColor = p.Coin.WithLuminosity(0.3f);
        canvas.FontSize = 8f;
        canvas.Font = GFont.DefaultBold;
        canvas.DrawString("Rs", -10f, -6f, 20f, 12f, HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.RestoreState();
    }

    // ---------------------------------------------------------------- receipt

    private void DrawReceipt(ICanvas canvas, Palette p)
    {
        var t = EaseOutCubic(Phase(RcptS, RcptE));
        if (t <= 0.01f) return;

        // Slides in from off the right edge.
        var x = Lerp(372f, 250f, t);

        canvas.SaveState();
        canvas.Alpha = t;
        canvas.Rotate(6f, x + 50f, 220f);

        canvas.SetShadow(new SizeF(0f, 4f), 10f, Colors.Black.WithAlpha(p.IsDark ? 0.45f : 0.16f));
        FillRoundRect(canvas, x, 158f, 100f, 124f, 5f, p.Card);
        canvas.SetShadow(SizeF.Zero, 0f, Colors.Transparent);
        StrokeRoundRect(canvas, x, 158f, 100f, 124f, 5f, p.Outline);

        canvas.FillColor = p.Muted.WithAlpha(0.5f);
        canvas.FillRoundedRectangle(x + 10f, 170f, 46f, 4f, 2f);
        canvas.FillRoundedRectangle(x + 10f, 182f, 80f, 3f, 1.5f);
        canvas.FillRoundedRectangle(x + 10f, 191f, 80f, 3f, 1.5f);
        canvas.FillRoundedRectangle(x + 10f, 200f, 58f, 3f, 1.5f);

        // PAID stamp — rotated, outlined, slightly transparent like real ink.
        canvas.SaveState();
        canvas.Rotate(-14f, x + 50f, 240f);
        canvas.StrokeColor = p.Success.WithAlpha(0.9f);
        canvas.StrokeSize = 2f;
        canvas.DrawRoundedRectangle(x + 16f, 224f, 68f, 30f, 4f);
        canvas.FontColor = p.Success;
        canvas.FontSize = 16f;
        canvas.Font = GFont.DefaultBold;
        canvas.DrawString("PAID", x + 16f, 224f, 68f, 30f, HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.RestoreState();

        canvas.RestoreState();
    }

    // ---------------------------------------------------------------- phone

    private void DrawPhone(ICanvas canvas, Palette p)
    {
        canvas.SetShadow(new SizeF(0f, 6f), 14f, Colors.Black.WithAlpha(p.IsDark ? 0.5f : 0.18f));
        FillRoundRect(canvas, PhoneX, PhoneY, PhoneW, PhoneH, 16f, p.IsDark ? Color.FromArgb("#101418") : Color.FromArgb("#37474F"));
        canvas.SetShadow(SizeF.Zero, 0f, Colors.Transparent);

        FillRoundRect(canvas, ScrX, ScrY, ScrW, ScrH, 10f, p.Surface);

        // --- customer header
        Text(canvas, "Ahmed Kiryana", ScrX + 8f, ScrY + 6f, 100f, 12f, p.Text, 9f, bold: true, HorizontalAlignment.Left);
        Text(canvas, "Route 4", ScrX + 8f, ScrY + 18f, 60f, 10f, p.TextSecondary, 7f, align: HorizontalAlignment.Left);

        // --- outstanding, counting down
        var countT = EaseOutCubic(Phase(CountS, CountE));
        var outstanding = Lerp(OutstandingStart, OutstandingEnd, countT);

        FillRoundRect(canvas, ScrX + 6f, ScrY + 34f, ScrW - 12f, 46f, 7f, p.SurfaceAlt);
        Text(canvas, "OUTSTANDING", ScrX + 12f, ScrY + 39f, 80f, 10f, p.TextSecondary, 6.5f, bold: true, HorizontalAlignment.Left);
        Text(canvas, $"Rs {outstanding:N0}", ScrX + 12f, ScrY + 52f, ScrW - 24f, 20f, p.Text, 15f, bold: true, HorizontalAlignment.Left);

        DrawStatusChip(canvas, p);

        // --- ledger rows
        string[] labels = ["Bill #1042", "Return", "Cash received"];
        string[] amounts = ["Rs 85,000", "- Rs 0", $"- Rs {Collected:N0}"];

        for (var i = 0; i < 3; i++)
        {
            var rowY = ScrY + 116f + i * 24f;
            var isCashRow = i == 2;

            // The cash row highlights once, right at the end of the loop.
            var highlight = isCashRow ? Pulse(RowS, RowE) : 0f;
            var rowFill = isCashRow
                ? Mix(p.SurfaceAlt, p.Success.WithAlpha(0.30f), highlight)
                : p.SurfaceAlt;

            FillRoundRect(canvas, ScrX + 6f, rowY, ScrW - 12f, 20f, 5f, rowFill);

            // A cash row only exists once the money has been taken.
            var rowAlpha = isCashRow ? EaseOutCubic(Phase(CountS, CountS + 0.10f)) : 1f;

            canvas.SaveState();
            canvas.Alpha = rowAlpha;

            var dot = isCashRow ? p.Success : i == 1 ? p.Warning : p.Primary;
            canvas.FillColor = dot;
            canvas.FillCircle(ScrX + 14f, rowY + 10f, 3f);

            Text(canvas, labels[i], ScrX + 22f, rowY, 60f, 20f, p.Text, 7f, align: HorizontalAlignment.Left);
            Text(canvas, amounts[i], ScrX + ScrW - 66f, rowY, 58f, 20f,
                isCashRow ? p.Success : p.TextSecondary, 7f, bold: isCashRow, HorizontalAlignment.Right);

            canvas.RestoreState();
        }

        DrawTickBadge(canvas, p);
    }

    private void DrawStatusChip(ICanvas canvas, Palette p)
    {
        // The amber "Overdue" chip cross-fades into a green "Paid" chip.
        var swap = EaseInOutCubic(Phase(ChipS, ChipE));
        var cx = ScrX + 6f;
        var cy = ScrY + 88f;

        if (swap < 1f)
        {
            canvas.SaveState();
            canvas.Alpha = 1f - swap;
            FillRoundRect(canvas, cx, cy, 92f, 18f, 9f, p.Warning.WithAlpha(0.18f));
            canvas.FillColor = p.Warning;
            canvas.FillCircle(cx + 10f, cy + 9f, 3f);
            Text(canvas, "Overdue · 12 days", cx + 17f, cy, 72f, 18f, p.Warning, 7f, bold: true, HorizontalAlignment.Left);
            canvas.RestoreState();
        }

        if (swap > 0f)
        {
            canvas.SaveState();
            canvas.Alpha = swap;
            FillRoundRect(canvas, cx, cy, 100f, 18f, 9f, p.Success.WithAlpha(0.18f));
            canvas.FillColor = p.Success;
            canvas.FillCircle(cx + 10f, cy + 9f, 3f);
            Text(canvas, $"Paid · Rs {Collected:N0}", cx + 17f, cy, 80f, 18f, p.Success, 7f, bold: true, HorizontalAlignment.Left);
            canvas.RestoreState();
        }
    }

    private void DrawTickBadge(ICanvas canvas, Palette p)
    {
        var s = Spring(Phase(TickS, TickE));
        if (s <= 0.01f) return;

        canvas.SaveState();
        canvas.Translate(PhoneX + PhoneW - 6f, PhoneY + 6f);
        canvas.Scale(s, s);

        canvas.SetShadow(new SizeF(0f, 2f), 8f, p.Success.WithAlpha(0.5f));
        TickBadge(canvas, 0f, 0f, 16f, p.Success, Colors.White);
        canvas.SetShadow(SizeF.Zero, 0f, Colors.Transparent);

        canvas.RestoreState();
    }
}
