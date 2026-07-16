namespace DMS_App.Dashboard.Drawables;

/// <summary>
/// A clean weekly sales bar chart for the dashboard: rounded bars with a subtle
/// baseline and a highlighted "today" bar. Values and colours are pushed in so it
/// stays theme-agnostic. Static (no animation) — it's a dashboard, not a loop.
/// </summary>
public sealed class WeeklySalesDrawable : IDrawable
{
    public float[] Values { get; set; } = [];
    public string[] Labels { get; set; } = [];
    public int HighlightIndex { get; set; } = -1;

    public Color BarColor { get; set; } = Color.FromArgb("#BBD9F5");
    public Color HighlightColor { get; set; } = Color.FromArgb("#1976D2");
    public Color GridColor { get; set; } = Color.FromArgb("#ECEFF3");
    public Color LabelColor { get; set; } = Color.FromArgb("#90A4AE");

    public void Draw(ICanvas canvas, RectF rect)
    {
        if (Values.Length == 0) return;

        var max = Values.Max();
        if (max <= 0) max = 1;

        const float labelH = 20f;
        const float topPad = 8f;
        var chartH = rect.Height - labelH - topPad;
        var baseY = rect.Y + topPad + chartH;

        // Two faint gridlines.
        canvas.StrokeColor = GridColor;
        canvas.StrokeSize = 1f;
        canvas.DrawLine(rect.X, baseY, rect.Right, baseY);
        canvas.DrawLine(rect.X, rect.Y + topPad + chartH * 0.5f, rect.Right, rect.Y + topPad + chartH * 0.5f);

        var slot = rect.Width / Values.Length;
        var barW = MathF.Min(slot * 0.46f, 22f);

        for (var i = 0; i < Values.Length; i++)
        {
            var h = chartH * (Values[i] / max);
            var cx = rect.X + slot * i + slot / 2f;
            var x = cx - barW / 2f;
            var y = baseY - h;
            var isToday = i == HighlightIndex;

            canvas.FillColor = isToday ? HighlightColor : BarColor;
            canvas.FillRoundedRectangle(x, y, barW, h, 5f);

            if (i < Labels.Length)
            {
                canvas.FontColor = isToday ? HighlightColor : LabelColor;
                canvas.FontSize = 11f;
                canvas.Font = isToday ? Microsoft.Maui.Graphics.Font.DefaultBold : Microsoft.Maui.Graphics.Font.Default;
                canvas.DrawString(Labels[i], cx - slot / 2f, baseY + 4f, slot, labelH,
                    HorizontalAlignment.Center, VerticalAlignment.Center);
            }
        }
    }
}
