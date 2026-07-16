using System.ComponentModel;
using System.Runtime.CompilerServices;
using DMS_App.Onboarding.Drawables;

namespace DMS_App.Onboarding;

public sealed class OnboardingSlide
{
    public required string Headline { get; init; }
    public required string Subtext { get; init; }
    public required SlideDrawableBase Drawable { get; init; }
}

public sealed class OnboardingViewModel : INotifyPropertyChanged
{
    public OnboardingViewModel()
    {
        Slides =
        [
            new OnboardingSlide
            {
                Headline = "Sell on the go",
                Subtext = "Create bills in seconds — right at the shop counter.",
                Drawable = new SlideOneDrawable()
            },
            new OnboardingSlide
            {
                Headline = "Collect payments instantly",
                Subtext = "Record cash on the spot. Ledgers update themselves.",
                Drawable = new SlideTwoDrawable()
            },
            new OnboardingSlide
            {
                Headline = "Know your numbers",
                Subtext = "Track today's sales, collections and route progress — live.",
                Drawable = new SlideThreeDrawable()
            },
        ];
    }

    public IReadOnlyList<OnboardingSlide> Slides { get; }

    private int _position;
    public int Position
    {
        get => _position;
        set
        {
            if (_position == value) return;
            _position = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CtaText));
            OnPropertyChanged(nameof(IsLastSlide));
            OnPropertyChanged(nameof(IsSkipVisible));
        }
    }

    public bool IsLastSlide => Position >= Slides.Count - 1;

    /// <summary>Slide 3 is the final frame, so the label flips on slide change — never mid-slide.</summary>
    public string CtaText => IsLastSlide ? "Get Started" : "Next";

    /// <summary>Skip hides itself on the last slide — there is nothing left to skip.</summary>
    public bool IsSkipVisible => !IsLastSlide;

    public OnboardingSlide Current => Slides[Math.Clamp(Position, 0, Slides.Count - 1)];

    /// <summary>A GraphicsView does not inherit AppThemeBinding, so the theme is pushed in.</summary>
    public void ApplyPalette(Palette palette)
    {
        foreach (var slide in Slides)
            slide.Drawable.Palette = palette;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
