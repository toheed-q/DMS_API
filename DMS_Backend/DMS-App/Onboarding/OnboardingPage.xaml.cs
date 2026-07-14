using System.ComponentModel;
using System.Diagnostics;
using DMS_App.Onboarding.Drawables;
using DMS_App.Onboarding.Services;

namespace DMS_App.Onboarding;

public partial class OnboardingPage : ContentPage
{
    private readonly OnboardingViewModel _vm;
    private readonly IMotionSettings _motion;

    // One timer drives every loop on this page. It ticks ONLY the active slide's
    // GraphicsView — off-screen slides do not tick. That is a battery decision: the
    // user is on a bike, in the sun, for eight hours.
    private IDispatcherTimer? _timer;
    private readonly Stopwatch _clock = new();
    private double _lastTick;

    private readonly List<GraphicsView> _canvases = [];
    private VisualElement[] _copyBlocks = [];
    private VisualElement[] _blobs = [];
    private Label[] _headlines = [];
    private Label[] _subtexts = [];

    private bool _reduceMotion;
    private bool _splashDone;

    /// <summary>Debug-only: proves the acceptance criterion "one GraphicsView invalidates per frame".</summary>
    private int _invalidationsThisTick;

    public OnboardingPage(OnboardingViewModel vm, IMotionSettings motion)
    {
        InitializeComponent();

        _vm = vm;
        _motion = motion;
        BindingContext = vm;

        _copyBlocks = [Copy0, Copy1, Copy2];
        _blobs = [Blob0, Blob1, Blob2];
        _headlines = [Head0, Head1, Head2];
        _subtexts = [Sub0, Sub1, Sub2];

        Carousel.SizeChanged += (_, _) => ApplyParallax(_vm.Position);
        Indicator.DotTapped += (_, index) => _vm.Position = index;
    }

    // ---------------------------------------------------------------- lifecycle

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _reduceMotion = _motion.ReduceMotion;
        Indicator.ReduceMotion = _reduceMotion;
        Cta.ReduceMotion = _reduceMotion;

        ApplyPalette();

        if (Application.Current is { } app)
            app.RequestedThemeChanged += OnRequestedThemeChanged;

        _vm.PropertyChanged += OnViewModelPropertyChanged;

        // Reduce motion: freeze every drawable at T = 1. Each was authored so that
        // T = 1 IS its correct static end state — cart filled, ledger settled, all
        // three orders synced — so this yields a complete frame, never a blank one.
        if (_reduceMotion)
        {
            foreach (var slide in _vm.Slides)
                slide.Drawable.T = 1f;
        }

        ApplySlideState(_vm.Position, animate: false);

        await Splash.PlayAsync(_reduceMotion);
        _splashDone = true;

        InvalidateAll();

        if (!_reduceMotion)
            StartTimer();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        StopTimer();
        Splash.Stop();

        _vm.PropertyChanged -= OnViewModelPropertyChanged;

        if (Application.Current is { } app)
            app.RequestedThemeChanged -= OnRequestedThemeChanged;
    }

    // ---------------------------------------------------------------- timer

    private void StartTimer()
    {
        if (_timer is not null) return;

        _clock.Restart();
        _lastTick = 0;

        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.Tick += OnTick;
        _timer.Start();
    }

    private void StopTimer()
    {
        if (_timer is null) return;

        _timer.Stop();
        _timer.Tick -= OnTick;
        _timer = null;
        _clock.Stop();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (!_splashDone) return;

        var now = _clock.Elapsed.TotalSeconds;
        var dt = now - _lastTick;
        _lastTick = now;

        if (dt <= 0 || dt > 0.25) return;   // first tick, or we were backgrounded

        var slide = _vm.Current;
        var drawable = slide.Drawable;

        var t = drawable.T + (float)(dt / drawable.LoopSeconds);
        drawable.T = t >= 1f ? t - MathF.Floor(t) : t;

        _invalidationsThisTick = 0;
        FindCanvas(slide)?.Invalidate();
        _invalidationsThisTick++;

        Debug.Assert(_invalidationsThisTick == 1,
            $"Exactly one GraphicsView must invalidate per frame; got {_invalidationsThisTick}.");
    }

    private GraphicsView? FindCanvas(OnboardingSlide slide)
    {
        foreach (var canvas in _canvases)
            if (ReferenceEquals(canvas.BindingContext, slide))
                return canvas;

        return null;
    }

    private void InvalidateAll()
    {
        foreach (var canvas in _canvases)
            canvas.Invalidate();
    }

    // ---------------------------------------------------------------- canvases

    private void OnSlideCanvasLoaded(object? sender, EventArgs e)
    {
        if (sender is not GraphicsView canvas) return;

        if (!_canvases.Contains(canvas))
            _canvases.Add(canvas);

        canvas.Invalidate();
    }

    private void OnSlideCanvasUnloaded(object? sender, EventArgs e)
    {
        if (sender is GraphicsView canvas)
            _canvases.Remove(canvas);
    }

    // ---------------------------------------------------------------- theme

    private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        ApplyPalette();
        InvalidateAll();
    }

    private void ApplyPalette() => _vm.ApplyPalette(Palette.Current());

    // ---------------------------------------------------------------- parallax

    /// <summary>
    /// One number drives everything. The CarouselView already moves the illustrations
    /// at 1.0x; the copy and blobs are counter-translated inside their own slots so
    /// they read as 0.6x and 0.3x while still landing centred.
    /// </summary>
    private void OnCarouselScrolled(object? sender, ItemsViewScrolledEventArgs e)
    {
        if (_reduceMotion) return;

        var width = Carousel.Width;
        if (width <= 0) return;

        // Verified on Android: HorizontalOffset is reported in DIP, same units as
        // Carousel.Width — so this ratio is the slide progress, 0 to 2.
        ApplyParallax(e.HorizontalOffset / width);
    }

    private void ApplyParallax(double progress)
    {
        var width = Carousel.Width;
        if (width <= 0) return;

        if (_reduceMotion)
        {
            // No parallax: the copy and blob simply belong to the current slide.
            for (var i = 0; i < _copyBlocks.Length; i++)
            {
                var isCurrent = i == _vm.Position;

                _copyBlocks[i].TranslationX = 0;
                _copyBlocks[i].Opacity = isCurrent ? 1 : 0;

                _blobs[i].TranslationX = 0;
                _blobs[i].Opacity = isCurrent ? 1 : 0;
            }
            return;
        }

        // The net offset is applied directly to each block. (Translating the track and
        // counter-translating the children lands in the same place, but leaves each
        // child sitting outside its parent's LOCAL bounds — and a clipping parent then
        // culls it entirely. That cost us every slide but the first.)
        for (var i = 0; i < _copyBlocks.Length; i++)
        {
            var off = (i - progress) * width;

            _copyBlocks[i].TranslationX = 0.6 * off;
            _blobs[i].TranslationX = 0.3 * off;

            // Fade with distance, so a half-swiped slide reads as half-arrived — and
            // so the neighbours a 0.6x/0.3x parallax leaves on screen stay invisible.
            var nearness = Math.Clamp(1 - Math.Abs(i - progress), 0, 1);
            _copyBlocks[i].Opacity = nearness;
            _blobs[i].Opacity = nearness;
        }
    }

    // ---------------------------------------------------------------- slide state

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OnboardingViewModel.Position))
            ApplySlideState(_vm.Position, animate: true);
    }

    private void ApplySlideState(int position, bool animate)
    {
        // Restart the incoming slide's story from the top so it always reads whole.
        if (!_reduceMotion && position >= 0 && position < _vm.Slides.Count)
            _vm.Slides[position].Drawable.T = 0f;

        ApplyParallax(position);

        var isLast = position >= _vm.Slides.Count - 1;

        if (_reduceMotion)
        {
            BottomStack.TranslationY = isLast ? 0 : 40;
            LoginLink.Opacity = isLast ? 1 : 0;
            TrustLine.Opacity = isLast ? 1 : 0;
            return;
        }

        if (animate)
            AnimateCopyIn(position);

        if (isLast)
        {
            // The single translate that IS the "final CTA rises" behaviour.
            BottomStack.TranslateTo(0, 0, 320, Easing.CubicOut);
            LoginLink.FadeTo(1, 260, Easing.CubicOut);
            TrustLine.FadeTo(1, 260, Easing.CubicOut);

            if (animate)
                Cta.PulseOnce();
        }
        else
        {
            BottomStack.TranslateTo(0, 40, 320, Easing.CubicOut);
            LoginLink.FadeTo(0, 200, Easing.CubicOut);
            TrustLine.FadeTo(0, 200, Easing.CubicOut);
        }
    }

    /// <summary>Text fade-up: 260ms, 16dp of travel, subtext staggered 80ms behind.</summary>
    private void AnimateCopyIn(int position)
    {
        if (position < 0 || position >= _headlines.Length) return;

        var headline = _headlines[position];
        var subtext = _subtexts[position];

        headline.TranslationY = 16;
        subtext.TranslationY = 16;

        headline.TranslateTo(0, 0, 260, Easing.CubicOut);

        // 80ms stagger — the subtext follows the headline up, it does not race it.
        Dispatcher.DispatchDelayed(
            TimeSpan.FromMilliseconds(80),
            () => subtext.TranslateTo(0, 0, 260, Easing.CubicOut));
    }

    // ---------------------------------------------------------------- actions

    private void OnCtaClicked(object? sender, EventArgs e)
    {
        if (_vm.IsLastSlide)
        {
            GoToLogin();
            return;
        }

        _vm.Position += 1;
    }

    private void OnSkipClicked(object? sender, EventArgs e) => GoToLogin();

    private void OnLoginClicked(object? sender, EventArgs e) => GoToLogin();

    private async void GoToLogin()
    {
        StopTimer();
        await Shell.Current.GoToAsync("//login");
    }
}
