using Microsoft.Extensions.DependencyInjection;

namespace DMS_App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Light is the shipping theme. Dark is fully built (Colors.xaml tokens +
            // Palette.Dark for the canvases) but stays OPTIONAL: it is not switched on
            // by the system theme. To enable it later, drop this line to follow the OS,
            // or set UserAppTheme from a user setting.
            UserAppTheme = AppTheme.Light;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}