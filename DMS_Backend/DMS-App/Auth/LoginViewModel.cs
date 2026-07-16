using System.Windows.Input;
using DMS_App.Mvvm;
using DMS_App.Services;

namespace DMS_App.Auth;

public sealed class LoginViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly ISessionStore _session;

    public LoginViewModel(IAuthService auth, ISessionStore session)
    {
        _auth = auth;
        _session = session;

        LoginCommand = new Command(async () => await LoginAsync(), () => !IsBusy);
        TogglePasswordCommand = new Command(() => IsPasswordHidden = !IsPasswordHidden);
    }

    // ---- inputs ---------------------------------------------------------

    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                UsernameError = null;
                ClearBanner();
            }
        }
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                PasswordError = null;
                ClearBanner();
            }
        }
    }

    private bool _isPasswordHidden = true;
    public bool IsPasswordHidden
    {
        get => _isPasswordHidden;
        set
        {
            if (SetProperty(ref _isPasswordHidden, value))
                OnPropertyChanged(nameof(PasswordToggleText));
        }
    }

    public string PasswordToggleText => IsPasswordHidden ? "Show" : "Hide";

    // ---- validation feedback -------------------------------------------

    private string? _usernameError;
    public string? UsernameError
    {
        get => _usernameError;
        set { if (SetProperty(ref _usernameError, value)) OnPropertyChanged(nameof(HasUsernameError)); }
    }
    public bool HasUsernameError => !string.IsNullOrEmpty(UsernameError);

    private string? _passwordError;
    public string? PasswordError
    {
        get => _passwordError;
        set { if (SetProperty(ref _passwordError, value)) OnPropertyChanged(nameof(HasPasswordError)); }
    }
    public bool HasPasswordError => !string.IsNullOrEmpty(PasswordError);

    // ---- server / banner error -----------------------------------------

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set { if (SetProperty(ref _errorMessage, value)) OnPropertyChanged(nameof(HasError)); }
    }
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    // ---- busy -----------------------------------------------------------

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsNotBusy));
                OnPropertyChanged(nameof(PrimaryButtonText));
                ((Command)LoginCommand).ChangeCanExecute();
            }
        }
    }
    public bool IsNotBusy => !IsBusy;

    public string PrimaryButtonText => IsBusy ? "Signing in…" : "Sign in";

    // ---- commands -------------------------------------------------------

    public ICommand LoginCommand { get; }
    public ICommand TogglePasswordCommand { get; }

    /// <summary>Raised on a successful sign-in so the page can navigate away.</summary>
    public event EventHandler<AuthUser>? LoggedIn;

    private async Task LoginAsync()
    {
        if (IsBusy) return;
        if (!Validate()) return;

        IsBusy = true;
        ClearBanner();

        try
        {
            var result = await _auth.LoginAsync(Username, Password);

            if (!result.Success)
            {
                ErrorMessage = result.Error ?? "Sign in failed. Please try again.";
                return;
            }

            await _session.SaveAsync(result.Token!, result.ExpiresAt, result.User!);
            LoggedIn?.Invoke(this, result.User!);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool Validate()
    {
        UsernameError = string.IsNullOrWhiteSpace(Username) ? "Enter your username." : null;
        PasswordError = string.IsNullOrWhiteSpace(Password) ? "Enter your password." : null;
        return !HasUsernameError && !HasPasswordError;
    }

    private void ClearBanner()
    {
        if (HasError) ErrorMessage = null;
    }
}
