using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Trinity.Shared.DTOs.Task;

namespace Client.Components.Pages.Logs;
public partial class Logs
{
    private class LogRow
    {
        public string Timestamp = "";
        public string Level = "INFO";
        public string Source = "";
        public string Event = "";
        public string User = "";
        public string Ip = "Not provided";
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
    private string? _loadError;
    private bool _isLoading;

    [Inject]
    private TaskApiClient TaskApiClient { get; set; } = default!;

    [Inject]
    private TeamServerConnection Connection { get; set; } = default!;

    private void GoToLogin() => Navigation.NavigateTo("/login");

    private string _search = "";
    private string _levelFilter = "All Levels";
    private string _sourceFilter = "All Sources";
    private int _page = 1;
    private const int PageSize = 5;

    private string? _toast;

    private bool _showAlert;
    private string _alertName = "";
    private string _alertLevel = "WARN";

    protected override async Task OnInitializedAsync() => await LoadTasksAsync();

    private async Task LoadTasksAsync()
    {
        _isLoading = true;
        _loadError = null;
        _logs.Clear();
        _topClients.Clear();
        _criticalLogs.Clear();
        _levelBreakdown.Clear();
        _sourceBreakdown.Clear();

        try
        {
            var tasks = await TaskApiClient.GetTasksAsync();
            var orderedTasks = tasks.OrderByDescending(task => task.timeStamp).ToList();
            _logs.AddRange(orderedTasks.Select(ToLogRow));
            var agentGroups = orderedTasks.GroupBy(task => task.agentID).ToList();
            var total = Math.Max(1, orderedTasks.Count);
            _topClients.AddRange(agentGroups
                .OrderByDescending(group => group.Count())
                .Take(5)
                .Select(group => new ClientCount(
                    $"Agent {group.Key}",
                    group.Count(),
                    group.Count() * 100.0 / total)));
            _criticalLogs.AddRange(orderedTasks
                .Where(task => task.status.Equals("Failure", StringComparison.OrdinalIgnoreCase))
                .Take(5)
                .Select(task => new CriticalLog(
                    $"{task.commandType} reported failure",
                    $"Agent {task.agentID}",
                    task.timeStamp.ToLocalTime().ToString("g"))));
            _levelBreakdown.AddRange(BuildBreakdown(orderedTasks
                .GroupBy(task => task.status)
                .Select(group => (group.Key, group.Count(), LevelColor(group.Key)))));
            var sourcePalette = new[] { "blue", "green", "purple", "yellow", "red", "gray" };
            _sourceBreakdown.AddRange(BuildBreakdown(orderedTasks
                .GroupBy(task => task.commandType)
                .Select((group, index) => (group.Key, group.Count(), sourcePalette[index % sourcePalette.Length]))));
            _page = 1;
        }
        catch (InvalidOperationException exception)
        {
            _loadError = exception.Message;
        }
        catch (HttpRequestException exception)
        {
            _loadError = $"Could not load task records from TeamServer: {exception.Message}";
        }
        catch (TaskCanceledException exception)
        {
            _loadError = $"The TeamServer task request timed out: {exception.Message}";
        }
        catch (JsonException exception)
        {
            _loadError = $"TeamServer returned an invalid task response: {exception.Message}";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private static LogRow ToLogRow(TaskResponse task) => new()
    {
        Timestamp = task.timeStamp.ToLocalTime().ToString("g"),
        Level = task.status,
        Source = task.commandType,
        Event = $"{task.commandType} reported status: {task.status}",
        User = $"Agent {task.agentID}"
    };

    private IEnumerable<string> Sources => _logs.Select(l => l.Source).Distinct().OrderBy(s => s);
    private IEnumerable<string> Levels => _logs.Select(l => l.Level).Distinct().OrderBy(level => level);

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
            if (_isLoading) return "Loading task history...";
            if (_loadError is not null) return "Task history unavailable";
            if (FilteredCount == 0) return "No task history to show";
            var start = (CurrentPage - 1) * PageSize + 1;
            var end = Math.Min(CurrentPage * PageSize, FilteredCount);
            return $"Show {start}-{end} of {FilteredCount:N0} logs";
        }
    }

    private void ResetPage() => _page = 1;
    private void GoToPage(int p) => _page = Math.Clamp(p, 1, TotalPages);
    private void SelectRow(LogRow row) => _selected = row == _selected ? null : row;

    private static string LevelColor(string level) => level.ToLowerInvariant() switch
    {
        "successful" => "green",
        "queued" or "running" or "pending" => "yellow",
        "failure" => "red",
        _ => "gray",
    };

    private List<StatVm> BuildStats() => new()
    {
        new("Task Records", _logs.Count.ToString("N0"), "From TeamServer", true,
            "<path d='M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z'/><polyline points='14 2 14 8 20 8'/>"),
        new("Successful", _logs.Count(log => log.Level.Equals("Successful", StringComparison.OrdinalIgnoreCase)).ToString("N0"), "Reported status", true,
            "<path d='M22 11.08V12a10 10 0 1 1-5.93-9.14'/><polyline points='22 4 12 14.01 9 11.01'/>"),
        new("Queued", _logs.Count(log => log.Level.Equals("Queued", StringComparison.OrdinalIgnoreCase)).ToString("N0"), "Reported status", true,
            "<rect x='6' y='5' width='4' height='14' rx='1'/><rect x='14' y='5' width='4' height='14' rx='1'/>"),
        new("Running", _logs.Count(log => log.Level.Equals("Running", StringComparison.OrdinalIgnoreCase)).ToString("N0"), "Reported status", true,
            "<circle cx='12' cy='12' r='9'/><polyline points='12 7 12 12 15 14'/>"),
        new("Failed", _logs.Count(log => log.Level.Equals("Failure", StringComparison.OrdinalIgnoreCase)).ToString("N0"), "Reported status", false,
            "<circle cx='12' cy='12' r='9'/><line x1='15' y1='9' x2='9' y2='15'/><line x1='9' y1='9' x2='15' y2='15'/>"),
    };

    private static List<SliceVm> BuildBreakdown(IEnumerable<(string Name, int Count, string Color)> items)
    {
        var slices = items.ToArray();
        int total = Math.Max(1, slices.Sum(i => i.Count));
        return slices.Select(i => new SliceVm(i.Name, i.Count, (int)Math.Round(i.Count * 100.0 / total), i.Color)).ToList();
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
            async () => await ExportLogs()),
        new ActionVm("Clear Old Logs",
            "<polyline points='3 6 5 6 21 6'/><path d='M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2'/>",
            () => ShowToast("Archived logs older than 90 days (mock)")),
        new ActionVm("View Log Settings",
            "<circle cx='12' cy='12' r='3'/><path d='M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z'/>",
            () => Navigation.NavigateTo("/settings")),
    };

    private async Task ExportLogs()
    {
        var rows = Filtered.ToList();
        if (rows.Count == 0)
        {
            ShowToast("There are no task records to export.");
            return;
        }

        var csv = new StringBuilder();
        AppendCsvRow(csv, "Timestamp", "Status", "Command Type", "Event", "Agent", "IP Address");
        foreach (var row in rows)
            AppendCsvRow(csv, row.Timestamp, row.Level, row.Source, row.Event, row.User, row.Ip);
        try
        {
            await using var module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/clients.js");
            await module.InvokeVoidAsync("downloadCsv", "task-history.csv", csv.ToString());
        }
        catch (JSException exception)
        {
            ShowToast($"Task history CSV download failed: {exception.Message}");
        }
    }

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private static void AppendCsvRow(StringBuilder csv, params string[] values) =>
        csv.AppendJoin(',', values.Select(EscapeCsvValue)).AppendLine();

    private static string EscapeCsvValue(string value)
    {
        var safeValue = value.TrimStart() is ['=', '+', '-', '@', ..] ? $"'{value}" : value;
        return $"\"{safeValue.Replace("\"", "\"\"")}\"";
    }

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
