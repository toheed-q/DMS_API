namespace DMS_App.Dashboard.Drawables;

public enum AppIcon
{
    Menu,
    Dashboard,
    Route,
    Products,
    NewBill,
    Collect,
    Returns,
    Ledger,
    Performance,
    Profile,
    Logout,
    Bell,
    ChevronLeft
}

/// <summary>
/// One small, crisp line-icon set drawn with Microsoft.Maui.Graphics so it scales and
/// re-colours for free (a GraphicsView can't inherit theme, so <see cref="Color"/> is
/// set explicitly). Everything is authored in a 24 x 24 box and letterboxed.
/// </summary>
public sealed class AppIconDrawable : IDrawable
{
    private const float Box = 24f;

    public AppIcon Icon { get; set; }
    public Color Color { get; set; } = Colors.Black;
    public float StrokeWidth { get; set; } = 2f;

    public AppIconDrawable() { }

    public AppIconDrawable(AppIcon icon, Color color)
    {
        Icon = icon;
        Color = color;
    }

    public void Draw(ICanvas canvas, RectF rect)
    {
        var scale = Math.Min(rect.Width, rect.Height) / Box;
        var dx = rect.X + (rect.Width - Box * scale) / 2f;
        var dy = rect.Y + (rect.Height - Box * scale) / 2f;

        canvas.SaveState();
        canvas.Translate(dx, dy);
        canvas.Scale(scale, scale);

        canvas.StrokeColor = Color;
        canvas.FillColor = Color;
        canvas.StrokeSize = StrokeWidth;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;

        switch (Icon)
        {
            case AppIcon.Menu: DrawMenu(canvas); break;
            case AppIcon.Dashboard: DrawDashboard(canvas); break;
            case AppIcon.Route: DrawRoute(canvas); break;
            case AppIcon.Products: DrawProducts(canvas); break;
            case AppIcon.NewBill: DrawNewBill(canvas); break;
            case AppIcon.Collect: DrawCollect(canvas); break;
            case AppIcon.Returns: DrawReturns(canvas); break;
            case AppIcon.Ledger: DrawLedger(canvas); break;
            case AppIcon.Performance: DrawPerformance(canvas); break;
            case AppIcon.Profile: DrawProfile(canvas); break;
            case AppIcon.Logout: DrawLogout(canvas); break;
            case AppIcon.Bell: DrawBell(canvas); break;
            case AppIcon.ChevronLeft: DrawChevronLeft(canvas); break;
        }

        canvas.RestoreState();
    }

    private static void Line(ICanvas c, float x1, float y1, float x2, float y2) => c.DrawLine(x1, y1, x2, y2);

    private void DrawMenu(ICanvas c)
    {
        Line(c, 3.5f, 7f, 20.5f, 7f);
        Line(c, 3.5f, 12f, 20.5f, 12f);
        Line(c, 3.5f, 17f, 20.5f, 17f);
    }

    private void DrawDashboard(ICanvas c)
    {
        c.DrawRoundedRectangle(3.5f, 3.5f, 7f, 7f, 1.6f);
        c.DrawRoundedRectangle(13.5f, 3.5f, 7f, 7f, 1.6f);
        c.DrawRoundedRectangle(3.5f, 13.5f, 7f, 7f, 1.6f);
        c.DrawRoundedRectangle(13.5f, 13.5f, 7f, 7f, 1.6f);
    }

    private void DrawRoute(ICanvas c)
    {
        // A location pin with an inner dot.
        var p = new PathF();
        p.MoveTo(12f, 21f);
        p.CurveTo(12f, 21f, 5f, 14.5f, 5f, 9.5f);
        p.CurveTo(5f, 5.9f, 8.1f, 3f, 12f, 3f);
        p.CurveTo(15.9f, 3f, 19f, 5.9f, 19f, 9.5f);
        p.CurveTo(19f, 14.5f, 12f, 21f, 12f, 21f);
        p.Close();
        c.DrawPath(p);
        c.FillCircle(12f, 9.3f, 2.4f);
    }

    private void DrawProducts(ICanvas c)
    {
        // Parcel: box with a lid seam and tape.
        c.DrawRoundedRectangle(4f, 6.5f, 16f, 13f, 2f);
        Line(c, 4f, 10.5f, 20f, 10.5f);
        Line(c, 12f, 10.5f, 12f, 19.5f);
    }

    private void DrawNewBill(ICanvas c)
    {
        // Receipt with a torn bottom edge and two text lines.
        var p = new PathF();
        p.MoveTo(6f, 3.5f);
        p.LineTo(18f, 3.5f);
        p.LineTo(18f, 20.5f);
        p.LineTo(15.5f, 19f);
        p.LineTo(13f, 20.5f);
        p.LineTo(10.5f, 19f);
        p.LineTo(8f, 20.5f);
        p.LineTo(6f, 19f);
        p.Close();
        c.DrawPath(p);
        Line(c, 9f, 8f, 15f, 8f);
        Line(c, 9f, 11.5f, 15f, 11.5f);
    }

    private void DrawCollect(ICanvas c)
    {
        // Banknote.
        c.DrawRoundedRectangle(3f, 7f, 18f, 10f, 2f);
        c.DrawCircle(12f, 12f, 2.6f);
        Line(c, 6f, 9.5f, 6f, 9.5f);
    }

    private void DrawReturns(ICanvas c)
    {
        // Curved back-arrow.
        var p = new PathF();
        p.MoveTo(8f, 8f);
        p.LineTo(4f, 8f);
        p.LineTo(4f, 4f);
        c.DrawPath(p);
        var arc = new PathF();
        arc.MoveTo(4f, 8f);
        arc.CurveTo(8f, 5f, 14f, 5f, 17f, 9f);
        arc.CurveTo(20f, 13f, 19f, 18f, 14f, 20f);
        c.DrawPath(arc);
    }

    private void DrawLedger(ICanvas c)
    {
        c.DrawRoundedRectangle(4.5f, 3.5f, 15f, 17f, 2f);
        Line(c, 8f, 8f, 16f, 8f);
        Line(c, 8f, 12f, 16f, 12f);
        Line(c, 8f, 16f, 13f, 16f);
    }

    private void DrawPerformance(ICanvas c)
    {
        Line(c, 5f, 20f, 5f, 13f);
        Line(c, 12f, 20f, 12f, 8f);
        Line(c, 19f, 20f, 19f, 4f);
    }

    private void DrawProfile(ICanvas c)
    {
        c.DrawCircle(12f, 8.5f, 4f);
        var p = new PathF();
        p.MoveTo(4.5f, 20f);
        p.CurveTo(4.5f, 15.5f, 8f, 13.5f, 12f, 13.5f);
        p.CurveTo(16f, 13.5f, 19.5f, 15.5f, 19.5f, 20f);
        c.DrawPath(p);
    }

    private void DrawLogout(ICanvas c)
    {
        var p = new PathF();
        p.MoveTo(13f, 4f);
        p.LineTo(6f, 4f);
        p.LineTo(6f, 20f);
        p.LineTo(13f, 20f);
        c.DrawPath(p);
        Line(c, 11f, 12f, 21f, 12f);
        var head = new PathF();
        head.MoveTo(17.5f, 8.5f);
        head.LineTo(21f, 12f);
        head.LineTo(17.5f, 15.5f);
        c.DrawPath(head);
    }

    private void DrawBell(ICanvas c)
    {
        var p = new PathF();
        p.MoveTo(6f, 17f);
        p.CurveTo(6f, 17f, 7f, 15.5f, 7f, 11f);
        p.CurveTo(7f, 7.5f, 9f, 5f, 12f, 5f);
        p.CurveTo(15f, 5f, 17f, 7.5f, 17f, 11f);
        p.CurveTo(17f, 15.5f, 18f, 17f, 18f, 17f);
        p.Close();
        c.DrawPath(p);
        var clap = new PathF();
        clap.MoveTo(10.5f, 19.5f);
        clap.CurveTo(11f, 20.5f, 13f, 20.5f, 13.5f, 19.5f);
        c.DrawPath(clap);
    }

    private void DrawChevronLeft(ICanvas c)
    {
        var p = new PathF();
        p.MoveTo(14.5f, 6f);
        p.LineTo(9f, 12f);
        p.LineTo(14.5f, 18f);
        c.DrawPath(p);
    }
}
