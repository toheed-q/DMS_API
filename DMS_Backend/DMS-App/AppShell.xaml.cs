namespace DMS_App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Pages reached by navigation (not tabs) are registered as routes. Keeping
            // them out of the Shell tree is what stops Shell from building a tab bar.
            Routing.RegisterRoute("login", typeof(LoginPage));
            Routing.RegisterRoute("dashboard", typeof(Dashboard.DashboardPage));
        }
    }
}
