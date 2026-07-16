using DMS_App.Dashboard.Drawables;

namespace DMS_App.Dashboard.Controls;

/// <summary>
/// The collapsible navigation sidebar. Expanded it shows icon + label (264dp); the
/// hamburger collapses it to an icon rail (76dp) on wide screens, or slides it fully
/// off-screen as an overlay drawer on phones. One control drives all three.
/// </summary>
public sealed class AppSidebar : ContentView
{
    public const double ExpandedWidth = 264;
    public const double RailWidth = 76;

    private static readonly Color Accent = Color.FromArgb("#1976D2");
    private static readonly Color AccentTint = Color.FromArgb("#141976D2"); // ~8% primary
    private static readonly Color InactiveText = Color.FromArgb("#455A64");
    private static readonly Color Surface = Color.FromArgb("#FFFFFF");

    private readonly List<NavRow> _rows = [];
    private readonly Label _brandWord;
    private readonly VerticalStackLayout _userText;
    private readonly Label _userName;
    private readonly Label _userRole;
    private readonly Label _avatarInitials;

    private bool _isCollapsed;

    public event EventHandler<string>? ItemSelected;
    public event EventHandler? LogoutRequested;

    private static readonly (string Key, string Label, AppIcon Icon)[] Items =
    [
        ("dashboard",   "Dashboard",        AppIcon.Dashboard),
        ("route",       "My Route",         AppIcon.Route),
        ("products",    "Products",         AppIcon.Products),
        ("newbill",     "New Bill",         AppIcon.NewBill),
        ("collect",     "Collect Payment",  AppIcon.Collect),
        ("returns",     "Returns",          AppIcon.Returns),
        ("ledger",      "Customer Ledger",  AppIcon.Ledger),
        ("performance", "My Performance",   AppIcon.Performance),
    ];

    public AppSidebar()
    {
        WidthRequest = ExpandedWidth;
        BackgroundColor = Surface;

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),  // brand
                new RowDefinition(GridLength.Auto),  // user chip
                new RowDefinition(GridLength.Star),  // nav
                new RowDefinition(GridLength.Auto),  // footer
            }
        };

        // ---- brand row ----
        var logo = new GraphicsView
        {
            Drawable = new Auth.Drawables.LogoDrawable(),
            WidthRequest = 34,
            HeightRequest = 34
        };
        _brandWord = new Label
        {
            Text = "DMS Field",
            FontSize = 17,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#212121"),
            VerticalOptions = LayoutOptions.Center
        };
        var brand = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
            Padding = new Thickness(22, 48, 16, 10), // top clears the status bar
            ColumnSpacing = 10
        };
        brand.Add(logo, 0, 0);
        brand.Add(_brandWord, 1, 0);
        root.Add(brand, 0, 0);

        // ---- user chip ----
        _avatarInitials = new Label
        {
            Text = "–",
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
        var avatar = new Border
        {
            WidthRequest = 42,
            HeightRequest = 42,
            BackgroundColor = Accent,
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 21 },
            Content = _avatarInitials
        };
        _userName = new Label { FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#212121"), LineBreakMode = LineBreakMode.TailTruncation };
        _userRole = new Label { FontSize = 11.5, TextColor = InactiveText };
        _userText = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
        _userText.Add(_userName);
        _userText.Add(_userRole);

        var userChip = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
            Padding = new Thickness(17, 6, 16, 14),
            ColumnSpacing = 12
        };
        userChip.Add(avatar, 0, 0);
        userChip.Add(_userText, 1, 0);
        root.Add(userChip, 0, 1);

        // ---- nav items ----
        var nav = new VerticalStackLayout { Spacing = 2, Padding = new Thickness(0, 6) };
        foreach (var (key, label, icon) in Items)
        {
            var row = new NavRow(key, label, icon, InactiveText, Accent, AccentTint);
            row.Tapped += (_, k) => ItemSelected?.Invoke(this, k);
            _rows.Add(row);
            nav.Add(row.View);
        }
        root.Add(new ScrollView { Content = nav, VerticalScrollBarVisibility = ScrollBarVisibility.Never }, 0, 2);

        // ---- footer ----
        var footer = new VerticalStackLayout { Spacing = 2, Padding = new Thickness(0, 8, 0, 18) };
        var divider = new BoxView { HeightRequest = 1, Color = Color.FromArgb("#ECEFF3"), Margin = new Thickness(18, 4, 18, 8) };
        footer.Add(divider);

        var profileRow = new NavRow("profile", "Profile", AppIcon.Profile, InactiveText, Accent, AccentTint);
        profileRow.Tapped += (_, k) => ItemSelected?.Invoke(this, k);
        _rows.Add(profileRow);
        footer.Add(profileRow.View);

        var logoutRow = new NavRow("logout", "Logout", AppIcon.Logout, Color.FromArgb("#E53935"), Color.FromArgb("#E53935"), Color.FromArgb("#14F44336"));
        logoutRow.Tapped += (_, _) => LogoutRequested?.Invoke(this, EventArgs.Empty);
        _rows.Add(logoutRow);
        footer.Add(logoutRow.View);

        root.Add(footer, 0, 3);

        Content = root;
        SetActive("dashboard");
    }

    public void SetUser(string? fullName, string? role)
    {
        var name = string.IsNullOrWhiteSpace(fullName) ? "Salesman" : fullName!;
        _userName.Text = name;
        _userRole.Text = string.IsNullOrWhiteSpace(role) ? "Field agent" : role;
        _avatarInitials.Text = Initials(name);
    }

    public void SetActive(string key)
    {
        foreach (var row in _rows)
            row.SetActive(row.Key == key);
    }

    public bool IsCollapsed
    {
        get => _isCollapsed;
        set
        {
            if (_isCollapsed == value) return;
            _isCollapsed = value;

            _brandWord.IsVisible = !value;
            _userText.IsVisible = !value;
            foreach (var row in _rows)
                row.SetCollapsed(value);
        }
    }

    private static string Initials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "–";
        if (parts.Length == 1) return parts[0][..1].ToUpperInvariant();
        return (parts[0][..1] + parts[^1][..1]).ToUpperInvariant();
    }

    /// <summary>A single tappable nav row: an active pill + icon + label.</summary>
    private sealed class NavRow
    {
        public string Key { get; }
        public View View => _pill;

        public event EventHandler<string>? Tapped;

        private readonly Border _pill;
        private readonly Label _label;
        private readonly GraphicsView _iconView;
        private readonly AppIconDrawable _icon;
        private readonly Color _inactive;
        private readonly Color _active;
        private readonly Color _activeTint;

        public NavRow(string key, string label, AppIcon icon, Color inactive, Color active, Color activeTint)
        {
            Key = key;
            _inactive = inactive;
            _active = active;
            _activeTint = activeTint;

            _icon = new AppIconDrawable(icon, inactive);
            _iconView = new GraphicsView
            {
                Drawable = _icon,
                WidthRequest = 24,
                HeightRequest = 24,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                InputTransparent = true
            };

            _label = new Label
            {
                Text = label,
                FontSize = 14.5,
                TextColor = inactive,
                VerticalOptions = LayoutOptions.Center,
                InputTransparent = true
            };

            var grid = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
                ColumnSpacing = 14,
                Padding = new Thickness(16, 0)
            };
            grid.Add(_iconView, 0, 0);
            grid.Add(_label, 1, 0);

            _pill = new Border
            {
                Content = grid,
                HeightRequest = 48,
                Margin = new Thickness(10, 2),
                Padding = 0,
                BackgroundColor = Colors.Transparent,
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 }
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => Tapped?.Invoke(this, Key);
            _pill.GestureRecognizers.Add(tap);
        }

        public void SetActive(bool active)
        {
            _pill.BackgroundColor = active ? _activeTint : Colors.Transparent;
            _label.TextColor = active ? _active : _inactive;
            _label.FontAttributes = active ? FontAttributes.Bold : FontAttributes.None;
            _icon.Color = active ? _active : _inactive;
            _icon.StrokeWidth = active ? 2.2f : 2f;
            _iconView.Invalidate();
        }

        public void SetCollapsed(bool collapsed) => _label.IsVisible = !collapsed;
    }
}
