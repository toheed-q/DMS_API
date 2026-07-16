using DMS_App.Auth;
using DMS_App.Services;

namespace DMS_App;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _vm;

    private static readonly Color FocusStroke = Color.FromArgb("#1976D2");

    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;

        _vm.LoggedIn += OnLoggedIn;

        // Lift the field's border to primary while it's focused.
        HookFocus(UsernameEntry, UsernameBorder);
        HookFocus(PasswordEntry, PasswordBorder);
    }

    private static void HookFocus(Entry entry, Border border)
    {
        entry.Focused += (_, _) =>
        {
            border.Stroke = FocusStroke;
            border.StrokeThickness = 2;
        };
        entry.Unfocused += (_, _) =>
        {
            border.Stroke = Colors.Transparent;
            border.StrokeThickness = 1.5;
        };
    }

    private async void OnLoggedIn(object? sender, AuthUser user)
    {
        // Drop focus so the soft keyboard doesn't linger onto the dashboard.
        UsernameEntry.Unfocus();
        PasswordEntry.Unfocus();

        await Shell.Current.GoToAsync("dashboard");
    }

    private async void OnForgotPassword(object? sender, EventArgs e)
    {
        await DisplayAlert(
            "Forgot password",
            "Please contact your distributor's admin to reset your password.",
            "OK");
    }
}
