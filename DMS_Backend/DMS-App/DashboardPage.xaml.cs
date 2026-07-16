using DMS_App.Services;

namespace DMS_App;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(ISessionStore session)
    {
        InitializeComponent();

        var name = session.FullName;
        if (string.IsNullOrWhiteSpace(name))
            name = session.Username;

        if (!string.IsNullOrWhiteSpace(name))
            WelcomeLabel.Text = $"Welcome, {name}";
    }
}
