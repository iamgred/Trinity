using System.Globalization;

namespace Client.Components.Pages.Logs;
//All of this is mock data for UI testing, can be removed without issue
public partial class Logs
{
    private class LogRow
    {
        public string Timestamp = "31/01/2026  12:00";
        public string Level = "INFO";
        public string Source = "";
        public string Event = "";
        public string User = "";
        public string Ip = "";
    }

    private record ClientCount(string Name, int Count, double Percent);
    private record CriticalLog(string Message, string Source, string Ago);
    private record StatVm(string Label, string Value, string Trend, bool Up, string Icon);
    private record SliceVm(string Name, int Count, int Percent, string Color);
    private record ActionVm(string Label, string Icon, Action OnClick);

    private readonly List<LogRow> _logs = new();
    private readonly List<ClientCount> _topClients = new();
    private readonly List<CriticalLog> _criticalLogs = new();
    private readonly List<SliceVm> _levelBreakdown = new();
    private readonly List<SliceVm> _sourceBreakdown = new();
    private LogRow? _selected;

    private string _search = "";
    private string _levelFilter = "All Levels";
    private string _sourceFilter = "All Sources";
    private int _page = 1;
    private const int PageSize = 5;

    private string? _toast;

    private bool _showAlert;
    private string _alertName = "";
    private string _alertLevel = "WARN";

    protected override void OnInitialized()
    {
        _logs.AddRange(new[]
        {
            new LogRow { Level="INFO",     Source="System",      Event="User Login",            User="Tony Stark",     Ip="192.168.1.10" },
            new LogRow { Level="INFO",     Source="Client",      Event="Client Connected",      User="CLIENT-001",     Ip="192.168.1.11" },
            new LogRow { Level="WARN",     Source="Campaign",    Event="Campaign Paused",       User="Phishing",       Ip="192.168.1.12" },
            new LogRow { Level="ERROR",    Source="File System", Event="File Uploaded",         User="Elton John",     Ip="192.168.1.13" },
            new LogRow { Level="WARN",     Source="Task",        Event="Task Failed",           User="Elton John",     Ip="192.168.1.14" },
            new LogRow { Level="INFO",     Source="Security",    Event="Failed Login Attempt",  User="Siya Kholisi",   Ip="192.168.1.15" },
            new LogRow { Level="ERROR",    Source="System",      Event="Config Updated",        User="Trevor Belmont", Ip="192.168.1.16" },
            new LogRow { Level="CRITICAL", Source="Client",      Event="Client Disconnected",   User="Trevor Belmont", Ip="192.168.1.17" },
            new LogRow { Level="INFO",     Source="System",      Event="Database Error",        User="Trevor Belmont", Ip="192.168.1.18" },
        });

        _topClients.AddRange(new[]
        {
            new ClientCount("CLIENT-001", 5342, 21.8),
            new ClientCount("CLIENT-002", 4125, 16.8),
            new ClientCount("CLIENT-003", 3876, 15.8),
        });

        _criticalLogs.AddRange(new[]
        {
            new CriticalLog("Database connection lost",       "System",   "2m ago"),
            new CriticalLog("Client authentication failure",  "Security", "15m ago"),
            new CriticalLog("Critical system resource low",   "System",   "32m ago"),
            new CriticalLog("Malware detected on CLIENT-001", "Security", "1h ago"),
            new CriticalLog("Backup process failed",          "System",   "2h ago"),
        });

        _levelBreakdown.AddRange(BuildBreakdown(new (string, int, string)[]
        {
            ("Informational", 15342, "green"),
            ("Warning",        4283, "yellow"),
            ("Error",          2156, "red"),
            ("Critical",        751, "purple"),
            ("Debug",          2000, "blue"),
        }));

        _sourceBreakdown.AddRange(BuildBreakdown(new (string, int, string)[]
        {
            ("System",      9856, "blue"),
            ("Client",      6742, "green"),
            ("Security",    3245, "purple"),
            ("Task",        2891, "yellow"),
            ("File System", 1798, "red"),
            ("Others",      1000, "gray"),
        }));
    }

    private IEnumerable<string> Sources => _logs.Select(l => l.Source).Distinct().OrderBy(s => s);

    private IEnumerable<LogRow> Filtered => _logs.Where(l =>
        (string.IsNullOrWhiteSpace(_search)
            || l.Event.Contains(_search, StringComparison.OrdinalIgnoreCase)
            || l.Source.Contains(_search, StringComparison.OrdinalIgnoreCase)
            || l.User.Contains(_search, StringComparison.OrdinalIgnoreCase)
            || l.Ip.Contains(_search, StringComparison.OrdinalIgnoreCase))
        && (_levelFilter == "All Levels" || l.Level == _levelFilter)
        && (_sourceFilter == "All Sources" || l.Source == _sourceFilter));

    private int FilteredCount => Filtered.Count();
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(FilteredCount / (double)PageSize));
    private int CurrentPage => Math.Clamp(_page, 1, TotalPages);
    private IEnumerable<LogRow> PageItems => Filtered.Skip((CurrentPage - 1) * PageSize).Take(PageSize);

    private int TotalLevelLogs => _levelBreakdown.Sum(s => s.Count);
    private int TotalSourceLogs => _sourceBreakdown.Sum(s => s.Count);

    private string ShowingText
    {
        get
        {
            if (FilteredCount == 0) return "No logs to show";
            var start = (CurrentPage - 1) * PageSize + 1;
            var end = Math.Min(CurrentPage * PageSize, FilteredCount);
            return $"Show {start}-{end} of {FilteredCount:N0} logs";
        }
    }

    private void ResetPage() => _page = 1;
    private void GoToPage(int p) => _page = Math.Clamp(p, 1, TotalPages);
    private void SelectRow(LogRow row) => _selected = row == _selected ? null : row;

    private static string LevelColor(string level) => level switch
    {
        "INFO" => "green",
        "WARN" => "yellow",
        "ERROR" => "red",
        "CRITICAL" => "purple",
        _ => "gray",
    };

    private List<StatVm> BuildStats() => new()
    {
        new("Total Logs", "24,532", "18% from last 24h", true,
            "<path d='M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z'/><polyline points='14 2 14 8 20 8'/>"),
        new("Informational", "15,342", "12% from last 24h", true,
            "<path d='M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z'/>"),
        new("Warnings", "4,283", "9% from last 24h", true,
            "<path d='M10.29 3.86 1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z'/><line x1='12' y1='9' x2='12' y2='13'/><line x1='12' y1='17' x2='12.01' y2='17'/>"),
        new("Errors", "2,156", "25% from last 24h", true,
            "<circle cx='12' cy='12' r='9'/><line x1='15' y1='9' x2='9' y2='15'/><line x1='9' y1='9' x2='15' y2='15'/>"),
        new("Critical", "751", "33% from last 24h", false,
            "<path d='M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9'/><path d='M13.73 21a2 2 0 0 1-3.46 0'/>"),
    };

    private static List<SliceVm> BuildBreakdown((string Name, int Count, string Color)[] items)
    {
        int total = Math.Max(1, items.Sum(i => i.Count));
        return items.Select(i => new SliceVm(i.Name, i.Count, (int)Math.Round(i.Count * 100.0 / total), i.Color)).ToList();
    }

    private static readonly Dictionary<string, string> _colorVar = new()
    {
        ["green"] = "var(--status-green)", ["blue"] = "var(--status-blue)",
        ["yellow"] = "var(--status-yellow)", ["red"] = "var(--status-red)",
        ["purple"] = "var(--status-purple)", ["gray"] = "var(--text-muted)"
    };

    private string Gradient(List<SliceVm> slices)
    {
        int total = Math.Max(1, slices.Sum(s => s.Count));
        var stops = new List<string>();
        double acc = 0;
        foreach (var s in slices)
        {
            if (s.Count == 0) continue;
            double start = acc / total * 100;
            acc += s.Count;
            double end = acc / total * 100;
            stops.Add($"{_colorVar[s.Color]} {start.ToString("0.##", CultureInfo.InvariantCulture)}% {end.ToString("0.##", CultureInfo.InvariantCulture)}%");
        }
        return stops.Count == 0 ? "var(--text-muted) 0% 100%" : string.Join(", ", stops);
    }

    private ActionVm[] Actions => new[]
    {
        new ActionVm("Search Logs",
            "<circle cx='11' cy='11' r='8'/><line x1='21' y1='21' x2='16.65' y2='16.65'/>",
            () => ShowToast("Search the All Logs table above (mock)")),
        new ActionVm("Create Log Alert",
            "<path d='M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9'/><path d='M13.73 21a2 2 0 0 1-3.46 0'/>",
            OpenAlert),
        new ActionVm("Export Logs",
            "<path d='M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4'/><polyline points='7 10 12 15 17 10'/><line x1='12' y1='15' x2='12' y2='3'/>",
            ExportLogs),
        new ActionVm("Clear Old Logs",
            "<polyline points='3 6 5 6 21 6'/><path d='M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2'/>",
            () => ShowToast("Archived logs older than 90 days (mock)")),
        new ActionVm("View Log Settings",
            "<circle cx='12' cy='12' r='3'/><path d='M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z'/>",
            () => Navigation.NavigateTo("/settings")),
    };

    private void ExportLogs() => ShowToast($"Exported {FilteredCount} logs to CSV (mock)");

    private void OpenAlert()
    {
        _alertName = "";
        _alertLevel = "WARN";
        _showAlert = true;
    }

    private void CreateAlert()
    {
        var name = string.IsNullOrWhiteSpace(_alertName) ? "Untitled alert" : _alertName.Trim();
        ShowToast($"Log alert \"{name}\" created for {_alertLevel} (mock)");
        _showAlert = false;
    }

    private void CloseModals() => _showAlert = false;

    private async void ShowToast(string msg)
    {
        _toast = msg;
        StateHasChanged();
        await Task.Delay(2600);
        if (_toast == msg) { _toast = null; StateHasChanged(); }
    }
}
