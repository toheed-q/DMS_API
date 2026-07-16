using DMS_App.Mvvm;
using DMS_App.Services;

namespace DMS_App.Dashboard;

public sealed record ActivityItem(string Title, string Subtitle, string Amount, string Time, string Kind);

/// <summary>
/// Dashboard state. KPI figures and activity are SAMPLE data for now — the API has no
/// single dashboard-summary endpoint yet, so these are placeholders to be wired to
/// /api/bills, /api/receivings and the ledger once that aggregate lands.
/// </summary>
public sealed class DashboardViewModel : ObservableObject
{
    private readonly ISessionStore _session;

    public DashboardViewModel(ISessionStore session)
    {
        _session = session;

        FullName = string.IsNullOrWhiteSpace(session.FullName) ? "Salesman" : session.FullName!;
        Role = string.IsNullOrWhiteSpace(session.Role) ? "Salesman" : session.Role!;
        Greeting = $"{TimeGreeting()},";
    }

    public string FullName { get; }
    public string Role { get; }
    public string Greeting { get; }

    // ---- KPIs (sample) --------------------------------------------------
    public string TodaySales => "Rs 24,850";
    public string TodaySalesDelta => "+12% vs yesterday";
    public string Collected => "Rs 18,600";
    public string CollectedSub => "9 payments";
    public string Outstanding => "Rs 6,250";
    public string OutstandingSub => "4 shops due";
    public string RouteProgress => "8 / 12";
    public string RouteSub => "shops visited";

    // ---- chart (sample) -------------------------------------------------
    public float[] WeekValues { get; } = [12000f, 18500f, 15000f, 22000f, 19500f, 24850f];
    public string[] WeekLabels { get; } = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
    public int TodayIndex => 5;

    // ---- recent activity (sample) --------------------------------------
    public IReadOnlyList<ActivityItem> RecentActivity { get; } =
    [
        new("Bill #1042 · Ahmed Kiryana", "6 items", "Rs 24,850", "12m ago", "bill"),
        new("Payment · Bilal Store", "Cash received", "Rs 9,400", "40m ago", "collect"),
        new("Return · Rehman General", "1 item", "Rs 1,200", "1h ago", "return"),
        new("Bill #1041 · Karim Store", "4 items", "Rs 6,300", "2h ago", "bill"),
    ];

    private static string TimeGreeting()
    {
        var hour = DateTime.Now.Hour;
        return hour switch
        {
            < 12 => "Good morning",
            < 17 => "Good afternoon",
            _ => "Good evening"
        };
    }
}
