using DMS_App.Auth;
using DMS_App.Onboarding;
using DMS_App.Onboarding.Services;
using DMS_App.Services;
using Microsoft.Extensions.Logging;

namespace DMS_App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Strip the platform's built-in Entry chrome so our own field container
            // (the rounded Border) is the only frame. Otherwise Android draws its
            // underline and iOS its border *inside* ours — the double-frame look.
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("DmsNoNativeChrome", (handler, _) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif IOS || MACCATALYST
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
            });

            // Onboarding
            builder.Services.AddSingleton<IMotionSettings, MotionSettings>();
            builder.Services.AddTransient<OnboardingViewModel>();
            builder.Services.AddTransient<OnboardingPage>();

            // Auth / API
            builder.Services.AddSingleton<ISessionStore, SessionStore>();
            builder.Services.AddSingleton(_ => new HttpClient
            {
                BaseAddress = new Uri(ApiConfig.BaseUrl),
                Timeout = TimeSpan.FromSeconds(20)
            });
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<DashboardPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
