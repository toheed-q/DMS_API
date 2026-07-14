namespace DMS_App.Onboarding.Drawables;

/// <summary>
/// Canvas-side mirror of the Colors.xaml tokens. A GraphicsView draws through
/// Microsoft.Maui.Graphics and does not inherit AppThemeBinding, so the page
/// hands the drawables a Palette and swaps it on RequestedThemeChanged.
/// </summary>
public sealed class Palette
{
    public required bool IsDark { get; init; }

    // Surfaces
    public required Color Bg { get; init; }
    public required Color Card { get; init; }          // illustration card fill
    public required Color Surface { get; init; }       // phone body / panels
    public required Color SurfaceAlt { get; init; }    // rows, wells, shelf backs
    public required Color Outline { get; init; }

    // Text
    public required Color Text { get; init; }
    public required Color TextSecondary { get; init; }
    public required Color TextOnPrimary { get; init; }

    // Brand
    public required Color Primary { get; init; }
    public required Color PrimaryDeep { get; init; }
    public required Color PrimaryLight { get; init; }

    // Status
    public required Color Success { get; init; }
    public required Color Warning { get; init; }
    public required Color Danger { get; init; }
    public required Color Info { get; init; }

    // Illustration props
    public required Color Skin { get; init; }
    public required Color Shirt { get; init; }
    public required Color Hair { get; init; }
    public required Color Cash { get; init; }
    public required Color Coin { get; init; }
    public required Color Muted { get; init; }         // greyed-out / disabled marks

    public static Palette Light { get; } = new()
    {
        IsDark = false,
        Bg = Color.FromArgb("#F5F7FA"),
        Card = Color.FromArgb("#FFFFFF"),
        Surface = Color.FromArgb("#FFFFFF"),
        SurfaceAlt = Color.FromArgb("#EEF2F7"),
        Outline = Color.FromArgb("#D8E0E9"),
        Text = Color.FromArgb("#212121"),
        TextSecondary = Color.FromArgb("#616161"),
        TextOnPrimary = Color.FromArgb("#FFFFFF"),
        Primary = Color.FromArgb("#1976D2"),
        PrimaryDeep = Color.FromArgb("#1565C0"),
        PrimaryLight = Color.FromArgb("#42A5F5"),
        Success = Color.FromArgb("#4CAF50"),
        Warning = Color.FromArgb("#FF9800"),
        Danger = Color.FromArgb("#F44336"),
        Info = Color.FromArgb("#2196F3"),
        Skin = Color.FromArgb("#C68642"),
        Shirt = Color.FromArgb("#1976D2"),
        Hair = Color.FromArgb("#2B2118"),
        Cash = Color.FromArgb("#7CB342"),
        Coin = Color.FromArgb("#FFB300"),
        Muted = Color.FromArgb("#B0BEC5"),
    };

    public static Palette Dark { get; } = new()
    {
        IsDark = true,
        Bg = Color.FromArgb("#263238"),
        Card = Color.FromArgb("#212121"),
        Surface = Color.FromArgb("#2E3B43"),
        SurfaceAlt = Color.FromArgb("#37474F"),
        Outline = Color.FromArgb("#455A64"),
        Text = Color.FromArgb("#FFFFFF"),
        TextSecondary = Color.FromArgb("#BFF5F7FA"),
        TextOnPrimary = Color.FromArgb("#FFFFFF"),
        Primary = Color.FromArgb("#42A5F5"),
        PrimaryDeep = Color.FromArgb("#1976D2"),
        PrimaryLight = Color.FromArgb("#82C7F8"),
        Success = Color.FromArgb("#66BB6A"),
        Warning = Color.FromArgb("#FFB74D"),
        Danger = Color.FromArgb("#EF5350"),
        Info = Color.FromArgb("#42A5F5"),
        Skin = Color.FromArgb("#B87333"),
        Shirt = Color.FromArgb("#42A5F5"),
        Hair = Color.FromArgb("#1A1410"),
        Cash = Color.FromArgb("#8BC34A"),
        Coin = Color.FromArgb("#FFC107"),
        Muted = Color.FromArgb("#607D8B"),
    };

    public static Palette For(AppTheme theme) => theme == AppTheme.Dark ? Dark : Light;

    public static Palette Current()
    {
        var theme = Application.Current?.RequestedTheme ?? AppTheme.Light;
        return For(theme);
    }
}
