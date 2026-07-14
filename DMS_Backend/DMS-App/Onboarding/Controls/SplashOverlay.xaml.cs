namespace DMS_App.Onboarding.Controls;

/// <summary>
/// The brand splash: a three-piece isometric box that assembles itself, then hands
/// the screen to the carousel.
///
/// Timeline (1.2s, Easing.CubicOut throughout):
///   0–250    mesh fades in
///   250–650  the 3 pieces translate + rotate into place
///   650–900  the assembled mark settles with a soft bounce (0.94 → 1.05 → 1.0)
///   900–1200 brand text fades up 12dp
/// then the whole overlay fades out over 420ms.
/// </summary>
public partial class SplashOverlay : ContentView
{
    private CancellationTokenSource? _drift;

    public SplashOverlay()
    {
        InitializeComponent();
    }

    /// <summary>Runs the splash and resolves once the overlay is gone.</summary>
    public async Task PlayAsync(bool reduceMotion)
    {
        if (reduceMotion)
        {
            // Reduce motion: show the assembled mark, hold briefly, leave. No drift,
            // no assembly, no bounce — but never a blank or half-drawn frame.
            SettleFinalState();
            await Task.Delay(500);
            await this.FadeTo(0, 200, Easing.CubicOut);
            IsVisible = false;
            return;
        }

        StartMeshDrift();

        // 0–250 — mesh in
        await Mesh.FadeTo(1, 250, Easing.CubicOut);

        // 250–650 — the three pieces fly into place together
        await Task.WhenAll(
            AssemblePiece(PieceLeft),
            AssemblePiece(PieceRight),
            AssemblePiece(PieceTop));

        // 650–900 — settle: 0.94 → 1.05 → 1.0
        await LogoBox.ScaleTo(1.05, 140, Easing.CubicOut);
        await LogoBox.ScaleTo(1.00, 110, Easing.CubicOut);

        // 900–1200 — brand text fades up 12dp
        await Task.WhenAll(
            BrandText.FadeTo(1, 300, Easing.CubicOut),
            BrandText.TranslateTo(0, 0, 300, Easing.CubicOut));

        await this.FadeTo(0, 420, Easing.CubicOut);

        StopMeshDrift();
        IsVisible = false;
    }

    private static Task AssemblePiece(VisualElement piece) =>
        Task.WhenAll(
            piece.FadeTo(1, 220, Easing.CubicOut),
            piece.TranslateTo(0, 0, 400, Easing.CubicOut),
            piece.RotateTo(0, 400, Easing.CubicOut));

    private void SettleFinalState()
    {
        Mesh.Opacity = 1;

        foreach (var piece in new VisualElement[] { PieceLeft, PieceRight, PieceTop })
        {
            piece.Opacity = 1;
            piece.TranslationX = 0;
            piece.TranslationY = 0;
            piece.Rotation = 0;
        }

        LogoBox.Scale = 1;
        BrandText.Opacity = 1;
        BrandText.TranslationY = 0;
    }

    // ---- slow background drift (20s ping-pong) --------------------------

    private void StartMeshDrift()
    {
        _drift = new CancellationTokenSource();
        var token = _drift.Token;

        _ = DriftAsync(Blob1, 26, 18, token);
        _ = DriftAsync(Blob2, -22, -14, token);
        _ = DriftAsync(Blob3, 16, -20, token);
    }

    private static async Task DriftAsync(VisualElement blob, double dx, double dy, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await blob.TranslateTo(dx, dy, 10_000, Easing.SinInOut);
                if (token.IsCancellationRequested) return;
                await blob.TranslateTo(0, 0, 10_000, Easing.SinInOut);
            }
        }
        catch (TaskCanceledException)
        {
            // The page went away mid-drift. Nothing to do.
        }
    }

    private void StopMeshDrift()
    {
        _drift?.Cancel();
        _drift?.Dispose();
        _drift = null;

        Blob1.AbortAnimation("TranslateTo");
        Blob2.AbortAnimation("TranslateTo");
        Blob3.AbortAnimation("TranslateTo");
    }

    public void Stop() => StopMeshDrift();
}
