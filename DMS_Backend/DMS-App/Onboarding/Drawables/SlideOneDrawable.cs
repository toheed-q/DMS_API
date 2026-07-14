namespace DMS_App.Onboarding.Drawables;

/// <summary>
/// Slide 1 — "Sell on the go".
///
/// A field salesman (lanyard + ID badge — he is a visiting rep, not the shopkeeper)
/// stands at a kiryana counter, phone raised, building a bill. Behind him, shelving
/// stocked with FMCG goods. A receipt emerges from under the phone.
///
/// Loop (2.4s): three product cards lift off the shelf in turn, arc into the cart
/// chip, the badge pops and counts 1 -> 2 -> 3, then the receipt glows once.
/// At T = 1 the cart is full, all three rows are on screen and the total has landed —
/// which is exactly the reduce-motion static frame.
/// </summary>
public sealed class SlideOneDrawable : SlideDrawableBase
{
    public override double LoopSeconds => 2.4;

    // Cached geometry — Draw() must not allocate. PathF has no Clear(), so every
    // static path is built once here and only ever replayed.
    private readonly PathF _torso;
    private readonly PathF _arm;
    private readonly PathF _lanyard;
    private readonly PathF _hair;
    private readonly PathF _collar;
    private readonly PathF _cart;

    // The gradient paint depends on the palette, so it is rebuilt only on a theme swap.
    private LinearGradientPaint? _btnPaint;
    private Palette? _btnPaintFor;

    // Phone / screen frame
    private const float PhoneX = 180f, PhoneY = 110f, PhoneW = 145f, PhoneH = 225f;
    private const float ScrX = PhoneX + 10f, ScrY = PhoneY + 12f, ScrW = PhoneW - 20f, ScrH = PhoneH - 24f;

    // Cart chip centre (flight destination)
    private const float CartCx = ScrX + ScrW - 22f, CartCy = ScrY + 24f;

    // The three products that fly, with their shelf origins.
    private static readonly (float X, float Y, string Label, string Hex)[] Flyers =
    [
        (58f,  44f,  "Biscuits",  "#E8A33D"),
        (124f, 92f,  "Shampoo",   "#7E57C2"),
        (44f,  120f, "Soap",      "#26A69A"),
    ];

    // Line totals as each product lands — the running total counts to Rs 24,850.
    private static readonly float[] LineTotals = [8200f, 8250f, 8400f];

    private static readonly (float Start, float End)[] FlyWindows =
    [
        (0.06f, 0.28f),
        (0.30f, 0.52f),
        (0.54f, 0.76f),
    ];

    public SlideOneDrawable()
    {
        _hair = new PathF();
        _hair.MoveTo(54f, 176f);
        _hair.QuadTo(58f, 150f, 78f, 152f);
        _hair.QuadTo(98f, 150f, 102f, 176f);
        _hair.QuadTo(94f, 164f, 78f, 164f);
        _hair.QuadTo(62f, 164f, 54f, 176f);
        _hair.Close();

        _torso = new PathF();
        _torso.MoveTo(48f, 322f);
        _torso.LineTo(52f, 218f);
        _torso.QuadTo(56f, 202f, 78f, 200f);
        _torso.QuadTo(100f, 202f, 106f, 218f);
        _torso.LineTo(112f, 322f);
        _torso.Close();

        _collar = new PathF();
        _collar.MoveTo(66f, 202f);
        _collar.LineTo(78f, 218f);
        _collar.LineTo(90f, 202f);
        _collar.QuadTo(78f, 198f, 66f, 202f);
        _collar.Close();

        _arm = new PathF();
        _arm.MoveTo(104f, 222f);
        _arm.QuadTo(140f, 226f, 168f, 208f);

        _lanyard = new PathF();
        _lanyard.MoveTo(68f, 206f);
        _lanyard.QuadTo(78f, 244f, 88f, 252f);
        _lanyard.MoveTo(90f, 206f);
        _lanyard.QuadTo(94f, 240f, 90f, 252f);

        _cart = new PathF();
        _cart.MoveTo(CartCx - 8f, CartCy - 4f);
        _cart.LineTo(CartCx - 5f, CartCy - 4f);
        _cart.LineTo(CartCx - 2.5f, CartCy + 3f);
        _cart.LineTo(CartCx + 6f, CartCy + 3f);
        _cart.LineTo(CartCx + 8f, CartCy - 2f);
        _cart.LineTo(CartCx - 4f, CartCy - 2f);
    }

    protected override void DrawSlide(ICanvas canvas)
    {
        var p = Palette;

        DrawShelving(canvas, p);
        DrawCounter(canvas, p);
        DrawSalesman(canvas, p);
        DrawReceipt(canvas, p);
        DrawPhone(canvas, p);
        DrawFlyingCards(canvas, p);
    }

    // ---------------------------------------------------------------- shelving

    private void DrawShelving(ICanvas canvas, Palette p)
    {
        // Back wall unit behind the salesman.
        FillRoundRect(canvas, 14f, 8f, 158f, 146f, 8f, p.SurfaceAlt);
        StrokeRoundRect(canvas, 14f, 8f, 158f, 146f, 8f, p.Outline);

        // Top-right unit, sitting above the phone.
        FillRoundRect(canvas, 182f, 8f, 162f, 88f, 8f, p.SurfaceAlt);
        StrokeRoundRect(canvas, 182f, 8f, 162f, 88f, 8f, p.Outline);

        canvas.StrokeColor = p.Outline;
        canvas.StrokeSize = 2f;
        canvas.DrawLine(14f, 66f, 172f, 66f);
        canvas.DrawLine(14f, 112f, 172f, 112f);
        canvas.DrawLine(182f, 54f, 344f, 54f);

        // --- left unit, shelf 1: biscuit packs + a bottle
        BiscuitPack(canvas, 44f, 30f, "#E8A33D");
        BiscuitPack(canvas, 76f, 30f, "#D2603A");
        Bottle(canvas, 112f, 24f, "#42A5F5");
        Bottle(canvas, 136f, 24f, "#66BB6A");

        // --- left unit, shelf 2: detergent boxes + shampoo
        DetergentBox(canvas, 26f, 76f, "#2196F3");
        DetergentBox(canvas, 62f, 76f, "#EF5350");
        Bottle(canvas, 112f, 70f, "#7E57C2");
        Bottle(canvas, 136f, 70f, "#FFA726");

        // --- left unit, shelf 3: soap bars + sachet strip
        SoapBar(canvas, 26f, 122f, "#26A69A");
        SoapBar(canvas, 62f, 122f, "#EC407A");
        SachetStrip(canvas, 104f, 124f, p);

        // --- top-right unit
        Bottle(canvas, 196f, 14f, "#66BB6A");
        BiscuitPack(canvas, 224f, 20f, "#E8A33D");
        DetergentBox(canvas, 262f, 18f, "#2196F3");
        SachetStrip(canvas, 300f, 26f, p);

        SoapBar(canvas, 196f, 62f, "#EC407A");
        BiscuitPack(canvas, 236f, 60f, "#D2603A");
        Bottle(canvas, 276f, 56f, "#7E57C2");
        DetergentBox(canvas, 302f, 60f, "#EF5350");
    }

    private static void BiscuitPack(ICanvas canvas, float x, float y, string hex)
    {
        var c = Color.FromArgb(hex);
        FillRoundRect(canvas, x, y, 24f, 30f, 3f, c);
        canvas.FillColor = c.WithLuminosity(0.85f);
        canvas.FillRoundedRectangle(x + 4f, y + 8f, 16f, 5f, 2f);
    }

    private static void Bottle(ICanvas canvas, float x, float y, string hex)
    {
        var c = Color.FromArgb(hex);
        canvas.FillColor = c;
        canvas.FillRoundedRectangle(x + 3f, y + 4f, 6f, 6f, 1.5f);   // neck
        canvas.FillRoundedRectangle(x, y + 9f, 12f, 27f, 3f);        // body
        canvas.FillColor = c.WithLuminosity(0.86f);
        canvas.FillRoundedRectangle(x + 1.5f, y + 18f, 9f, 7f, 1.5f); // label
    }

    private static void DetergentBox(ICanvas canvas, float x, float y, string hex)
    {
        var c = Color.FromArgb(hex);
        FillRoundRect(canvas, x, y, 28f, 32f, 2.5f, c);
        canvas.FillColor = c.WithLuminosity(0.88f);
        canvas.FillRoundedRectangle(x + 5f, y + 10f, 18f, 8f, 2f);
    }

    private static void SoapBar(ICanvas canvas, float x, float y, string hex)
    {
        var c = Color.FromArgb(hex);
        FillRoundRect(canvas, x, y, 28f, 16f, 4f, c);
        canvas.FillColor = c.WithLuminosity(0.9f);
        canvas.FillRoundedRectangle(x + 6f, y + 5f, 16f, 5f, 2f);
    }

    private static void SachetStrip(ICanvas canvas, float x, float y, Palette p)
    {
        // A hanging strip of shampoo sachets — the kiryana signature.
        canvas.StrokeColor = p.Outline;
        canvas.StrokeSize = 1f;
        canvas.DrawLine(x + 8f, y, x + 8f, y + 4f);

        for (var i = 0; i < 4; i++)
        {
            canvas.FillColor = i % 2 == 0 ? Color.FromArgb("#EF5350") : Color.FromArgb("#42A5F5");
            canvas.FillRoundedRectangle(x, y + 4f + i * 7f, 16f, 6f, 1.5f);
        }
    }

    // ---------------------------------------------------------------- counter

    private void DrawCounter(ICanvas canvas, Palette p)
    {
        FillRoundRect(canvas, 0f, 318f, 358f, 12f, 4f, p.Outline);      // counter top edge
        FillRoundRect(canvas, 0f, 328f, 358f, 52f, 6f, p.SurfaceAlt);   // counter front
        canvas.StrokeColor = p.Outline;
        canvas.StrokeSize = 1f;
        canvas.DrawLine(0f, 352f, 358f, 352f);
    }

    // ---------------------------------------------------------------- salesman

    private void DrawSalesman(ICanvas canvas, Palette p)
    {
        // Head
        canvas.FillColor = p.Skin;
        canvas.FillCircle(78f, 178f, 24f);

        // Hair
        canvas.FillColor = p.Hair;
        canvas.FillPath(_hair);

        // Torso
        canvas.FillColor = p.Shirt;
        canvas.FillPath(_torso);

        // Collar
        canvas.FillColor = p.Shirt.WithLuminosity(p.IsDark ? 0.42f : 0.78f);
        canvas.FillPath(_collar);

        // Raised arm — shoulder to the hand under the phone.
        canvas.StrokeColor = p.Shirt;
        canvas.StrokeSize = 17f;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.DrawPath(_arm);

        // Hand gripping the phone's lower-left corner.
        canvas.FillColor = p.Skin;
        canvas.FillCircle(174f, 206f, 10f);

        // Lanyard + ID badge — reads as a visiting rep.
        canvas.StrokeColor = p.PrimaryDeep;
        canvas.StrokeSize = 3f;
        canvas.DrawPath(_lanyard);

        FillRoundRect(canvas, 76f, 252f, 28f, 20f, 3f, p.Card);
        StrokeRoundRect(canvas, 76f, 252f, 28f, 20f, 3f, p.Primary);
        canvas.FillColor = p.Primary;
        canvas.FillRoundedRectangle(80f, 256f, 8f, 8f, 1.5f);
        canvas.FillColor = p.Muted;
        canvas.FillRoundedRectangle(90f, 257f, 10f, 2f, 1f);
        canvas.FillRoundedRectangle(90f, 262f, 8f, 2f, 1f);
    }

    // ---------------------------------------------------------------- receipt

    private void DrawReceipt(ICanvas canvas, Palette p)
    {
        // Emerges from under the phone. Glows once late in the loop.
        var glow = Pulse(0.80f, 0.96f);

        canvas.SaveState();
        canvas.Rotate(-4f, 262f, 348f);

        if (glow > 0.01f)
        {
            canvas.SetShadow(new SizeF(0f, 0f), 14f * glow, p.Success.WithAlpha(0.85f * glow));
        }

        FillRoundRect(canvas, 214f, 300f, 96f, 74f, 4f, p.Card);
        canvas.SetShadow(SizeF.Zero, 0f, Colors.Transparent);
        StrokeRoundRect(canvas, 214f, 300f, 96f, 74f, 4f, Mix(p.Outline, p.Success, glow));

        canvas.FillColor = p.Muted.WithAlpha(0.55f);
        canvas.FillRoundedRectangle(222f, 310f, 48f, 4f, 2f);
        canvas.FillRoundedRectangle(222f, 322f, 80f, 3f, 1.5f);
        canvas.FillRoundedRectangle(222f, 331f, 80f, 3f, 1.5f);
        canvas.FillRoundedRectangle(222f, 340f, 64f, 3f, 1.5f);

        canvas.FillColor = Mix(p.Muted, p.Success, glow);
        canvas.FillRoundedRectangle(222f, 354f, 40f, 6f, 2f);

        canvas.RestoreState();
    }

    // ---------------------------------------------------------------- phone

    private void DrawPhone(ICanvas canvas, Palette p)
    {
        // How many products have landed in the cart by now.
        var landed = LandedCount();

        // Body
        canvas.SetShadow(new SizeF(0f, 6f), 12f, Colors.Black.WithAlpha(p.IsDark ? 0.5f : 0.18f));
        FillRoundRect(canvas, PhoneX, PhoneY, PhoneW, PhoneH, 16f, p.IsDark ? Color.FromArgb("#101418") : Color.FromArgb("#37474F"));
        canvas.SetShadow(SizeF.Zero, 0f, Colors.Transparent);

        // Screen
        FillRoundRect(canvas, ScrX, ScrY, ScrW, ScrH, 10f, p.Surface);

        // --- header
        Text(canvas, "New Bill", ScrX + 8f, ScrY + 6f, 70f, 12f, p.Text, 9f, bold: true, HorizontalAlignment.Left);
        Text(canvas, "Ahmed Kiryana", ScrX + 8f, ScrY + 18f, 80f, 10f, p.TextSecondary, 7f, align: HorizontalAlignment.Left);

        DrawCartChip(canvas, p, landed);

        // --- product rows (one appears as each card lands)
        for (var i = 0; i < 3; i++)
        {
            var rowY = ScrY + 44f + i * 30f;
            float appear;

            if (i < landed)
            {
                appear = 1f;
            }
            else if (i == landed)
            {
                // Row slides in exactly as its card is absorbed.
                var (s, e) = FlyWindows[i];
                appear = EaseOutCubic(Phase(e - 0.04f, e + 0.05f));
            }
            else
            {
                appear = 0f;
            }

            if (appear <= 0.01f) continue;

            canvas.SaveState();
            canvas.Alpha = appear;
            canvas.Translate((1f - appear) * 12f, 0f);

            FillRoundRect(canvas, ScrX + 6f, rowY, ScrW - 12f, 25f, 5f, p.SurfaceAlt);

            // thumbnail
            canvas.FillColor = Color.FromArgb(Flyers[i].Hex);
            canvas.FillRoundedRectangle(ScrX + 10f, rowY + 4f, 17f, 17f, 3f);

            Text(canvas, Flyers[i].Label, ScrX + 32f, rowY + 4f, 52f, 9f, p.Text, 7.5f, bold: true, HorizontalAlignment.Left);
            Text(canvas, $"Rs {LineTotals[i]:N0}", ScrX + 32f, rowY + 14f, 52f, 8f, p.TextSecondary, 6.5f, align: HorizontalAlignment.Left);

            // quantity chip
            var qx = ScrX + ScrW - 30f;
            FillRoundRect(canvas, qx, rowY + 6f, 22f, 13f, 6.5f, p.Primary.WithAlpha(0.16f));
            Text(canvas, $"x{i + 2}", qx, rowY + 6f, 22f, 13f, p.Primary, 7f, bold: true);

            canvas.RestoreState();
        }

        // --- running total
        var total = 0f;
        for (var i = 0; i < landed; i++) total += LineTotals[i];

        canvas.StrokeColor = p.Outline;
        canvas.StrokeSize = 1f;
        canvas.DrawLine(ScrX + 6f, ScrY + 136f, ScrX + ScrW - 6f, ScrY + 136f);

        Text(canvas, "Total", ScrX + 8f, ScrY + 142f, 40f, 14f, p.TextSecondary, 8f, align: HorizontalAlignment.Left);
        Text(canvas, $"Rs {total:N0}", ScrX + ScrW - 74f, ScrY + 142f, 66f, 14f, p.Text, 11f, bold: true, HorizontalAlignment.Right);

        // --- save bill button
        var btnY = ScrY + ScrH - 26f;
        var btnRect = new RectF(ScrX + 6f, btnY, ScrW - 12f, 20f);

        if (_btnPaint is null || !ReferenceEquals(_btnPaintFor, p))
        {
            _btnPaint = new LinearGradientPaint(new Point(0, 0), new Point(1, 0))
            {
                GradientStops =
                [
                    new PaintGradientStop(0f, p.PrimaryDeep),
                    new PaintGradientStop(1f, p.PrimaryLight),
                ]
            };
            _btnPaintFor = p;
        }

        canvas.SetFillPaint(_btnPaint, btnRect);
        canvas.FillRoundedRectangle(btnRect, 6f);
        Text(canvas, "Save bill", btnRect.X, btnRect.Y, btnRect.Width, btnRect.Height, Colors.White, 8f, bold: true);
    }

    private void DrawCartChip(ICanvas canvas, Palette p, int landed)
    {
        // The chip itself
        FillRoundRect(canvas, CartCx - 15f, CartCy - 11f, 30f, 22f, 7f, p.Primary.WithAlpha(0.14f));

        // Cart glyph
        canvas.StrokeColor = p.Primary;
        canvas.StrokeSize = 1.4f;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.DrawPath(_cart);
        canvas.FillColor = p.Primary;
        canvas.FillCircle(CartCx - 1.5f, CartCy + 6.5f, 1.4f);
        canvas.FillCircle(CartCx + 5f, CartCy + 6.5f, 1.4f);

        if (landed == 0) return;

        // Count badge — pops on each absorption.
        var pop = 1f;
        var justLanded = landed - 1;
        var (_, end) = FlyWindows[justLanded];
        var since = Phase(end, end + 0.09f);
        if (since < 1f)
            pop = 0.55f + 0.45f * EaseOutBack(since);

        canvas.SaveState();
        canvas.Translate(CartCx + 11f, CartCy - 9f);
        canvas.Scale(pop, pop);

        canvas.FillColor = p.Danger;
        canvas.FillCircle(0f, 0f, 7f);
        Text(canvas, landed.ToString(), -7f, -7f, 14f, 14f, Colors.White, 8f, bold: true);

        canvas.RestoreState();
    }

    // ---------------------------------------------------------------- flight

    private int LandedCount()
    {
        var n = 0;
        for (var i = 0; i < FlyWindows.Length; i++)
            if (T >= FlyWindows[i].End) n++;
        return n;
    }

    private void DrawFlyingCards(ICanvas canvas, Palette p)
    {
        for (var i = 0; i < Flyers.Length; i++)
        {
            var (start, end) = FlyWindows[i];
            if (T < start || T >= end) continue;   // not in flight

            var t = EaseInOutCubic(Phase(start, end));

            var (sx, sy, _, hex) = Flyers[i];
            // Quadratic arc, bowing upward so the card lifts off the shelf.
            var cx = (sx + CartCx) / 2f;
            var cy = MathF.Min(sy, CartCy) - 58f;

            var mt = 1f - t;
            var x = mt * mt * sx + 2f * mt * t * cx + t * t * CartCx;
            var y = mt * mt * sy + 2f * mt * t * cy + t * t * CartCy;

            // Shrinks as it is absorbed; fades out on the last few percent.
            var scale = Lerp(1f, 0.35f, t);
            var alpha = t < 0.88f ? 1f : Clamp01((1f - t) / 0.12f);

            canvas.SaveState();
            canvas.Alpha = alpha;
            canvas.Translate(x, y);
            canvas.Rotate(Lerp(-8f, 14f, t));
            canvas.Scale(scale, scale);

            canvas.SetShadow(new SizeF(0f, 3f), 8f, Colors.Black.WithAlpha(0.22f));
            FillRoundRect(canvas, -14f, -14f, 28f, 28f, 5f, p.Card);
            canvas.SetShadow(SizeF.Zero, 0f, Colors.Transparent);

            canvas.FillColor = Color.FromArgb(hex);
            canvas.FillRoundedRectangle(-10f, -10f, 20f, 14f, 3f);
            canvas.FillColor = p.Muted.WithAlpha(0.6f);
            canvas.FillRoundedRectangle(-10f, 6f, 14f, 3f, 1.5f);

            canvas.RestoreState();
        }
    }
}
