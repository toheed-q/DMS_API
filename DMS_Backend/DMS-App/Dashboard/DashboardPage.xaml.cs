using DMS_App.Dashboard.Controls;
using DMS_App.Dashboard.Drawables;
using DMS_App.Services;

namespace DMS_App.Dashboard;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _vm;
    private readonly ISessionStore _session;

    // Layout mode: docked rail on wide screens, overlay drawer on phones.
    private const double WideBreakpoint = 900;
    private bool _isWide;
    private bool _drawerOpen;
    private bool _railCollapsed;

    private static readonly Color Accent = Color.FromArgb("#1976D2");

    public DashboardPage(DashboardViewModel vm, ISessionStore session)
    {
        InitializeComponent();
        _vm = vm;
        _session = session;

        BindData();
        BuildQuickActions();
        BuildActivity();
        SetupChart();

        Sidebar.SetUser(_vm.FullName, _vm.Role);
        Sidebar.ItemSelected += OnSidebarItemSelected;
        Sidebar.LogoutRequested += OnLogoutRequested;

        // A ContentView with VerticalOptions=Fill does not receive a height from the
        // Grid (its inner ScrollView collapses to 0), so give it a fixed height taller
        // than any phone. SizeChanged refines it to the exact page height.
        Sidebar.HeightRequest = 1400;

        // The sidebar stays ARRANGED at x=0 the whole time and is hidden with Opacity —
        // a ContentView first arranged off-screen (via TranslationX) fails to render
        // when translated back, so we never arrange it off-screen. Closed = transparent
        // + input-transparent; the slide is an opacity-1 translate.
        Sidebar.Opacity = 0;
        Sidebar.InputTransparent = true;
        Sidebar.TranslationX = 0;
        SizeChanged += (_, _) => ApplyResponsiveState();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        HideKeyboard();   // the sign-in keyboard must not linger onto the dashboard
    }

#if ANDROID
    private static void HideKeyboard()
    {
        var activity = Platform.CurrentActivity;
        if (activity is null) return;

        if (activity.GetSystemService(Android.Content.Context.InputMethodService)
            is Android.Views.InputMethods.InputMethodManager imm)
        {
            var token = activity.CurrentFocus?.WindowToken;
            imm.HideSoftInputFromWindow(token, Android.Views.InputMethods.HideSoftInputFlags.None);
        }
        activity.CurrentFocus?.ClearFocus();
    }
#else
    private static void HideKeyboard() { }
#endif

    // ---- data ----------------------------------------------------------

    private void BindData()
    {
        GreetingLabel.Text = _vm.Greeting;
        NameLabel.Text = _vm.FullName;
        DateLabel.Text = DateTime.Now.ToString("dddd, d MMM");

        KpiSales.Text = _vm.TodaySales;
        KpiSalesDelta.Text = _vm.TodaySalesDelta;
        KpiCollected.Text = _vm.Collected;
        KpiCollectedSub.Text = _vm.CollectedSub;
        KpiOutstanding.Text = _vm.Outstanding;
        KpiOutstandingSub.Text = _vm.OutstandingSub;
        KpiRoute.Text = _vm.RouteProgress;
        KpiRouteSub.Text = _vm.RouteSub;

        TopAvatarInitials.Text = Initials(_vm.FullName);
    }

    private void SetupChart()
    {
        ChartView.Drawable = new WeeklySalesDrawable
        {
            Values = _vm.WeekValues,
            Labels = _vm.WeekLabels,
            HighlightIndex = _vm.TodayIndex
        };
    }

    // ---- quick actions -------------------------------------------------

    private void BuildQuickActions()
    {
        (string Key, string Label, AppIcon Icon, string Hex)[] actions =
        [
            ("newbill", "New Bill", AppIcon.NewBill, "#1976D2"),
            ("collect", "Collect", AppIcon.Collect, "#2E7D32"),
            ("returns", "Return", AppIcon.Returns, "#EF6C00"),
            ("ledger", "Ledger", AppIcon.Ledger, "#5E35B1"),
        ];

        for (var i = 0; i < actions.Length; i++)
        {
            var (key, label, icon, hex) = actions[i];
            var color = Color.FromArgb(hex);

            var chip = new Border
            {
                WidthRequest = 52,
                HeightRequest = 52,
                BackgroundColor = color.WithAlpha(0.12f),
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                HorizontalOptions = LayoutOptions.Center,
                Content = new GraphicsView
                {
                    Drawable = new AppIconDrawable(icon, color),
                    WidthRequest = 24,
                    HeightRequest = 24,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    InputTransparent = true
                }
            };

            var stack = new VerticalStackLayout { Spacing = 7, HorizontalOptions = LayoutOptions.Center };
            stack.Add(chip);
            stack.Add(new Label
            {
                Text = label,
                FontSize = 12,
                TextColor = Color.FromArgb("#455A64"),
                HorizontalTextAlignment = TextAlignment.Center
            });

            var tap = new TapGestureRecognizer();
            var k = key;
            tap.Tapped += async (_, _) => await ShowComingSoon(k);
            stack.GestureRecognizers.Add(tap);

            QuickActions.Add(stack, i, 0);
        }
    }

    // ---- recent activity ----------------------------------------------

    private void BuildActivity()
    {
        var items = _vm.RecentActivity;
        for (var i = 0; i < items.Count; i++)
        {
            var a = items[i];
            var kindColor = a.Kind switch
            {
                "collect" => Color.FromArgb("#2E7D32"),
                "return" => Color.FromArgb("#EF6C00"),
                _ => Accent
            };
            var kindIcon = a.Kind switch
            {
                "collect" => AppIcon.Collect,
                "return" => AppIcon.Returns,
                _ => AppIcon.NewBill
            };

            var row = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                ColumnSpacing = 12,
                Padding = new Thickness(10, 12)
            };

            var iconChip = new Border
            {
                WidthRequest = 38,
                HeightRequest = 38,
                BackgroundColor = kindColor.WithAlpha(0.12f),
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 11 },
                VerticalOptions = LayoutOptions.Center,
                Content = new GraphicsView
                {
                    Drawable = new AppIconDrawable(kindIcon, kindColor),
                    WidthRequest = 19,
                    HeightRequest = 19,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };
            row.Add(iconChip, 0, 0);

            var text = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
            text.Add(new Label { Text = a.Title, FontSize = 13.5, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#212121"), LineBreakMode = LineBreakMode.TailTruncation });
            text.Add(new Label { Text = a.Subtitle, FontSize = 11.5, TextColor = Color.FromArgb("#78909C") });
            row.Add(text, 1, 0);

            var right = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center };
            right.Add(new Label { Text = a.Amount, FontSize = 13.5, FontAttributes = FontAttributes.Bold, TextColor = kindColor, HorizontalTextAlignment = TextAlignment.End });
            right.Add(new Label { Text = a.Time, FontSize = 11, TextColor = Color.FromArgb("#90A4AE"), HorizontalTextAlignment = TextAlignment.End });
            row.Add(right, 2, 0);

            ActivityList.Add(row);

            if (i < items.Count - 1)
                ActivityList.Add(new BoxView { HeightRequest = 1, Color = Color.FromArgb("#F0F3F6"), Margin = new Thickness(10, 0) });
        }
    }

    // ---- responsive layout --------------------------------------------

    private void ApplyResponsiveState()
    {
        if (Width <= 0) return;

        // Refine the sidebar height to the exact page height once it's known.
        if (Height > 0)
            Sidebar.HeightRequest = Height;

        var wide = Width >= WideBreakpoint;
        _isWide = wide;

        if (wide)
        {
            // Docked rail: sidebar always visible, content sits beside it.
            Scrim.IsVisible = false;
            Scrim.Opacity = 0;
            _drawerOpen = false;

            Sidebar.IsCollapsed = _railCollapsed;
            Sidebar.WidthRequest = _railCollapsed ? AppSidebar.RailWidth : AppSidebar.ExpandedWidth;
            Sidebar.TranslationX = 0;
            Sidebar.Opacity = 1;
            Sidebar.InputTransparent = false;
            ContentHost.Margin = new Thickness(Sidebar.WidthRequest, 0, 0, 0);
        }
        else
        {
            // Drawer: overlay, arranged at x=0, hidden with opacity when closed.
            Sidebar.IsCollapsed = false;
            Sidebar.WidthRequest = AppSidebar.ExpandedWidth;
            ContentHost.Margin = new Thickness(0);
            Sidebar.TranslationX = 0;

            if (_drawerOpen)
            {
                Sidebar.Opacity = 1;
                Sidebar.InputTransparent = false;
                Scrim.IsVisible = true;
                Scrim.Opacity = 0.45;
            }
            else
            {
                Sidebar.Opacity = 0;
                Sidebar.InputTransparent = true;
                Scrim.IsVisible = false;
                Scrim.Opacity = 0;
            }
        }
    }

    private async void OnToggleSidebar(object? sender, EventArgs e)
    {
        if (_isWide)
            await ToggleRailAsync();
        else
            await ToggleDrawerAsync();
    }

    private async Task ToggleDrawerAsync()
    {
        if (_drawerOpen)
        {
            // Flip state first so a relayout mid-animation doesn't fight the close.
            _drawerOpen = false;
            Scrim.FadeTo(0, 200);
            await Sidebar.TranslateTo(-AppSidebar.ExpandedWidth, 0, 220, Easing.CubicIn);
            Sidebar.Opacity = 0;
            Sidebar.InputTransparent = true;
            Sidebar.TranslationX = 0;   // rest arranged on-screen but transparent
            Scrim.IsVisible = false;
        }
        else
        {
            _drawerOpen = true;
            Sidebar.InputTransparent = false;
            Sidebar.Opacity = 1;
            Sidebar.TranslationX = -AppSidebar.ExpandedWidth;
            Scrim.IsVisible = true;
            Scrim.FadeTo(0.45, 220);
            await Sidebar.TranslateTo(0, 0, 240, Easing.CubicOut);
        }
    }

    private async Task ToggleRailAsync()
    {
        _railCollapsed = !_railCollapsed;
        var from = Sidebar.Width;
        var to = _railCollapsed ? AppSidebar.RailWidth : AppSidebar.ExpandedWidth;

        if (_railCollapsed)
            Sidebar.IsCollapsed = true;   // hide labels before shrinking

        var anim = new Animation(v =>
        {
            Sidebar.WidthRequest = v;
            ContentHost.Margin = new Thickness(v, 0, 0, 0);
        }, from, to, Easing.CubicInOut);

        var tcs = new TaskCompletionSource();
        anim.Commit(this, "rail", 16, 260, finished: (_, _) =>
        {
            if (!_railCollapsed)
                Sidebar.IsCollapsed = false;   // reveal labels once expanded
            tcs.TrySetResult();
        });
        await tcs.Task;
    }

    private async void OnScrimTapped(object? sender, EventArgs e)
    {
        if (_drawerOpen)
            await ToggleDrawerAsync();
    }

    // ---- navigation ----------------------------------------------------

    private async void OnSidebarItemSelected(object? sender, string key)
    {
        Sidebar.SetActive(key);

        if (!_isWide && _drawerOpen)
            await ToggleDrawerAsync();

        if (key != "dashboard")
            await ShowComingSoon(key);
    }

    private async void OnLogoutRequested(object? sender, EventArgs e)
    {
        var ok = await DisplayAlert("Log out", "Are you sure you want to log out?", "Log out", "Cancel");
        if (!ok) return;

        _session.Clear();
        await Shell.Current.GoToAsync("//onboarding");
    }

    private Task ShowComingSoon(string key) =>
        DisplayAlert("Coming soon", $"“{Pretty(key)}” isn't built yet — it's next on the list.", "OK");

    private static string Pretty(string key) => key switch
    {
        "route" => "My Route",
        "products" => "Products",
        "newbill" => "New Bill",
        "collect" => "Collect Payment",
        "returns" => "Returns",
        "ledger" => "Customer Ledger",
        "performance" => "My Performance",
        "profile" => "Profile",
        _ => key
    };

    private static string Initials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "–";
        if (parts.Length == 1) return parts[0][..1].ToUpperInvariant();
        return (parts[0][..1] + parts[^1][..1]).ToUpperInvariant();
    }
}
