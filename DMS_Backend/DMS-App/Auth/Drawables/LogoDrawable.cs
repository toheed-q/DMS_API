namespace DMS_App.Auth.Drawables;

/// <summary>
/// The DMS mark: an isometric delivery box built from three panels, matching the
/// splash logo. Static — no animation. Drawn in a 120 x 124 design space and
/// letterboxed into whatever the GraphicsView is given.
/// </summary>
public sealed class LogoDrawable : IDrawable
{
    private const float DesignW = 120f, DesignH = 124f;

    private readonly PathF _left;
    private readonly PathF _right;
    private readonly PathF _top;

    public LogoDrawable()
    {
        _left = new PathF();
        _left.MoveTo(8f, 34f);
        _left.LineTo(60f, 64f);
        _left.LineTo(60f, 120f);
        _left.LineTo(8f, 90f);
        _left.Close();

        _right = new PathF();
        _right.MoveTo(112f, 34f);
        _right.LineTo(60f, 64f);
        _right.LineTo(60f, 120f);
        _right.LineTo(112f, 90f);
        _right.Close();

        _top = new PathF();
        _top.MoveTo(60f, 4f);
        _top.LineTo(112f, 34f);
        _top.LineTo(60f, 64f);
        _top.LineTo(8f, 34f);
        _top.Close();
    }

    public void Draw(ICanvas canvas, RectF rect)
    {
        var scale = Math.Min(rect.Width / DesignW, rect.Height / DesignH);
        var dx = rect.X + (rect.Width - DesignW * scale) / 2f;
        var dy = rect.Y + (rect.Height - DesignH * scale) / 2f;

        canvas.SaveState();
        canvas.Translate(dx, dy);
        canvas.Scale(scale, scale);

        canvas.FillColor = Color.FromArgb("#64B5F6");
        canvas.FillPath(_left);

        canvas.FillColor = Color.FromArgb("#1E88E5");
        canvas.FillPath(_right);

        canvas.FillColor = Color.FromArgb("#BBDEFB");
        canvas.FillPath(_top);

        canvas.RestoreState();
    }
}
