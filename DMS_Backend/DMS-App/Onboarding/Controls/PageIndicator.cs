using Microsoft.Maui.Controls.Shapes;

namespace DMS_App.Onboarding.Controls;

/// <summary>
/// Page dots that morph the active one into a 24 x 8 pill.
///
/// This is a custom control because <see cref="IndicatorView"/> cannot do it: it can
/// recolour and resize a dot, but it cannot animate one dot's width independently
/// while the others hold. So we own the boxes and animate WidthRequest directly.
///
/// Each dot sits inside a 48dp-tall hit area — the dots are tappable, and a 8dp
/// target is not.
/// </summary>
public sealed class PageIndicator : ContentView
{
    private const double DotSize = 8d;
    private const double PillWidth = 24d;
    private const double MorphMs = 380d;

    private readonly HorizontalStackLayout _row;
    private readonly List<BoxView> _dots = [];

    public event EventHandler<int>? DotTapped;

    public PageIndicator()
    {
        _row = new HorizontalStackLayout
        {
            Spacing = 0,
            HorizontalOptions = LayoutOptions.Center
        };

        Content = _row;
    }

    // ---- bindable state -------------------------------------------------

    public static readonly BindableProperty CountProperty = BindableProperty.Create(
        nameof(Count), typeof(int), typeof(PageIndicator), 0,
        propertyChanged: (b, _, _) => ((PageIndicator)b).BuildDots());

    public int Count
    {
        get => (int)GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    public static readonly BindableProperty PositionProperty = BindableProperty.Create(
        nameof(Position), typeof(int), typeof(PageIndicator), 0,
        propertyChanged: (b, o, n) => ((PageIndicator)b).OnPositionChanged((int)o, (int)n));

    public int Position
    {
        get => (int)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    /// <summary>When true, the dot snaps instead of springing.</summary>
    public bool ReduceMotion { get; set; }

    // ---- construction ---------------------------------------------------

    private void BuildDots()
    {
        _row.Clear();
        _dots.Clear();

        for (var i = 0; i < Count; i++)
        {
            var index = i;
            var isActive = i == Position;

            var dot = new BoxView
            {
                HeightRequest = DotSize,
                WidthRequest = isActive ? PillWidth : DotSize,
                CornerRadius = (float)(DotSize / 2d),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            ApplyDotColor(dot, isActive);

            // 48dp minimum tap target, regardless of how small the dot looks.
            var hit = new Grid
            {
                HeightRequest = 48,
                WidthRequest = 34,
                BackgroundColor = Colors.Transparent,
                Children = { dot }
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => DotTapped?.Invoke(this, index);
            hit.GestureRecognizers.Add(tap);

            _dots.Add(dot);
            _row.Add(hit);
        }
    }

    private static void ApplyDotColor(BoxView dot, bool isActive)
    {
        if (isActive)
        {
            dot.SetAppThemeColor(BoxView.ColorProperty,
                Color.FromArgb("#1976D2"), Color.FromArgb("#42A5F5"));
        }
        else
        {
            // #757575 at 28% (light) / #F5F7FA at 24% (dark)
            dot.SetAppThemeColor(BoxView.ColorProperty,
                Color.FromArgb("#47757575"), Color.FromArgb("#3DF5F7FA"));
        }
    }

    // ---- morph ----------------------------------------------------------

    private void OnPositionChanged(int previous, int current)
    {
        if (_dots.Count == 0)
        {
            BuildDots();
            return;
        }

        if (previous >= 0 && previous < _dots.Count && previous != current)
            MorphDot(_dots[previous], toPill: false, key: $"dot{previous}");

        if (current >= 0 && current < _dots.Count)
            MorphDot(_dots[current], toPill: true, key: $"dot{current}");
    }

    private void MorphDot(BoxView dot, bool toPill, string key)
    {
        ApplyDotColor(dot, toPill);

        var from = dot.WidthRequest;
        var to = toPill ? PillWidth : DotSize;

        dot.AbortAnimation(key);

        if (ReduceMotion)
        {
            dot.WidthRequest = to;
            return;
        }

        new Animation(v => dot.WidthRequest = v, from, to, Easing.SpringOut)
            .Commit(dot, key, 16, (uint)MorphMs);
    }
}
