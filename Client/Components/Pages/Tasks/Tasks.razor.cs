using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Trinity.Shared.DTOs.Task;

namespace Client.Components.Pages.Tasks;
public partial class Tasks
{
    private class TaskRow
    {
        public string Name = "";
        public string Client = "";
        public string Campaign = "Not provided";
        public string Priority = "Not provided";
        public string Status = "Pending";
        public string Assignee = "Not provided";
        public string DueDate = "Not provided";
        public int Progress;
        public int AgentId;
        public string CommandType = "";
        public DateTime Timestamp;
    }

    private record UpcomingRow(string Priority, string Name, string Client, string Campaign, string Assignee, string DueDate, string Badge, string BadgeColor);
    private record StatVm(string Label, string Value, string Trend, bool Up, string Icon);
    private record SliceVm(string Name, int Count, int Percent, string Color);
    private record ActionVm(string Label, string Icon, Action OnClick);

    private readonly List<TaskRow> _tasks = new();
    private readonly List<UpcomingRow> _upcoming = new();
    private TaskRow? _selected;

    private string _search = "";
    private string _statusFilter = "All Status";
    private int _page = 1;
    private const int PageSize = 6;

    private string? _toast;
    private string? _loadError;
    private bool _isLoading;

    private string _detailIdInput = "";
    private GetTaskDetailsResponse? _detail;
    private string? _detailError;
    private bool _detailLoading;

    [Inject]
    private TaskApiClient TaskApiClient { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private TeamServerConnection Connection { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private void GoToLogin() => Navigation.NavigateTo("/login");

    private bool _showCreate, _showAssign, _showSchedule;
    private TaskRow _form = new();
    private string _assignName = "";
    private string _scheduleDate = "";

    protected override async Task OnInitializedAsync() => await LoadTasksAsync();

    private async Task LoadTasksAsync()
    {
        _isLoading = true;
        _loadError = null;
        _tasks.Clear();
        _upcoming.Clear();

        try
        {
            var tasks = await TaskApiClient.GetTasksAsync();
            _tasks.AddRange(tasks
                .OrderByDescending(task => task.timeStamp)
                .Select(ToTaskRow));
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

    private static TaskRow ToTaskRow(TaskResponse task) => new()
    {
        Name = task.commandType,
        Client = $"Agent {task.agentID}",
        Status = task.status,
        AgentId = task.agentID,
        CommandType = task.commandType,
        Timestamp = task.timeStamp
    };

    private IEnumerable<string> Assignees => [];
    private IEnumerable<string> Statuses => _tasks.Select(t => t.Status).Distinct().OrderBy(status => status);

    private IEnumerable<TaskRow> Filtered => _tasks.Where(t =>
        (string.IsNullOrWhiteSpace(_search)
            || t.Name.Contains(_search, StringComparison.OrdinalIgnoreCase)
            || t.Client.Contains(_search, StringComparison.OrdinalIgnoreCase)
            || t.Status.Contains(_search, StringComparison.OrdinalIgnoreCase))
        && (_statusFilter == "All Status" || t.Status.Equals(_statusFilter, StringComparison.OrdinalIgnoreCase)));

    private int FilteredCount => Filtered.Count();
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(FilteredCount / (double)PageSize));
    private int CurrentPage => Math.Clamp(_page, 1, TotalPages);
    private IEnumerable<TaskRow> PageItems => Filtered.Skip((CurrentPage - 1) * PageSize).Take(PageSize);

    private string ShowingText
    {
        get
        {
            if (_isLoading) return "Loading task records...";
            if (_loadError is not null) return "Task records unavailable";
            if (FilteredCount == 0) return "No tasks to show";
            var start = (CurrentPage - 1) * PageSize + 1;
            var end = Math.Min(CurrentPage * PageSize, FilteredCount);
            return $"Showing {start}–{end} of {FilteredCount} Tasks";
        }
    }

    private void ResetPage() => _page = 1;
    private void GoToPage(int p) => _page = Math.Clamp(p, 1, TotalPages);
    private void SelectRow(TaskRow row) => _selected = row == _selected ? null : row;

    private async Task LookupTaskDetailAsync()
    {
        _detailError = null;
        _detail = null;

        if (!int.TryParse(_detailIdInput?.Trim(), out var id) || id <= 0)
        {
            _detailError = "Enter a numeric task ID.";
            return;
        }

        _detailLoading = true;
        try
        {
            _detail = await TaskApiClient.GetTaskDetailAsync(id);
            if (_detail is null)
            {
                _detailError = $"No task found with ID {id}.";
            }
        }
        catch (InvalidOperationException exception)
        {
            _detailError = exception.Message;
        }
        catch (HttpRequestException exception)
        {
            _detailError = $"Could not load task detail from TeamServer: {exception.Message}";
        }
        catch (TaskCanceledException exception)
        {
            _detailError = $"The TeamServer task-detail request timed out: {exception.Message}";
        }
        catch (JsonException exception)
        {
            _detailError = $"TeamServer returned an invalid task-detail response: {exception.Message}";
        }
        finally
        {
            _detailLoading = false;
        }
    }

    private static string FormatCommand(JsonDocument? command)
    {
        if (command is null) return "No command payload returned.";
        try
        {
            return JsonSerializer.Serialize(command, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return command.RootElement.ToString();
        }
    }

    private static string PriorityColor(string priority) => priority switch
    {
        "High" => "red",
        "Medium" => "yellow",
        "Low" => "green",
        _ => "gray",
    };

    private static string StatusColor(string status) => status switch
    {
        "Successful" => "green",
        "Failure" => "red",
        "Running" or "Pending" => "blue",
        "Queued" => "yellow",
        _ => "gray",
    };

    private List<StatVm> BuildStats()
    {
        int total = _tasks.Count;
        int completed = _tasks.Count(t => t.Status.Equals("Successful", StringComparison.OrdinalIgnoreCase));
        int failed = _tasks.Count(t => t.Status.Equals("Failure", StringComparison.OrdinalIgnoreCase));
        int running = _tasks.Count(t => t.Status.Equals("Running", StringComparison.OrdinalIgnoreCase));
        int queued = _tasks.Count(t => t.Status.Equals("Queued", StringComparison.OrdinalIgnoreCase));

        return new List<StatVm>
        {
            new("Total Tasks", total.ToString(), "From TeamServer", true,
                "<rect x='8' y='2' width='8' height='4' rx='1'/><path d='M9 4H6a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2h-3'/>"),
            new("Successful Tasks", completed.ToString(), "Reported status", true,
                "<path d='M22 11.08V12a10 10 0 1 1-5.93-9.14'/><polyline points='22 4 12 14.01 9 11.01'/>"),
            new("Running", running.ToString(), "Reported status", true,
                "<circle cx='12' cy='12' r='9'/><polyline points='12 7 12 12 15 14'/>"),
            new("Queued", queued.ToString(), "Reported status", true,
                "<rect x='6' y='5' width='4' height='14' rx='1'/><rect x='14' y='5' width='4' height='14' rx='1'/>"),
            new("Failed Tasks", failed.ToString(), "Reported status", false,
                "<circle cx='12' cy='12' r='9'/><line x1='15' y1='9' x2='9' y2='15'/><line x1='9' y1='9' x2='15' y2='15'/>"),
        };
    }

    private List<SliceVm> BuildPriorityBreakdown() => [];

    private List<SliceVm> BuildOverviewBreakdown()
    {
        return BuildBreakdown(_tasks.GroupBy(task => task.Status)
            .Select(group => (group.Key, group.Count(), StatusColor(group.Key))));
    }

    private List<SliceVm> BuildBreakdown(IEnumerable<(string Name, int Count, string Color)> items)
    {
        var slices = items.ToArray();
        var total = Math.Max(1, slices.Sum(item => item.Count));
        return slices.Select(item => new SliceVm(
            item.Name,
            item.Count,
            (int)Math.Round(item.Count * 100.0 / total),
            item.Color)).ToList();
    }

    private static readonly Dictionary<string, string> _colorVar = new()
    {
        ["green"] = "var(--status-green)", ["blue"] = "var(--status-blue)",
        ["yellow"] = "var(--status-yellow)", ["red"] = "var(--status-red)", ["gray"] = "var(--text-muted)"
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
        new ActionVm("Create New Task",
            "<line x1='12' y1='5' x2='12' y2='19'/><line x1='5' y1='12' x2='19' y2='12'/>",
            OpenCreate),
        new ActionVm("Schedule Task",
            "<circle cx='12' cy='12' r='9'/><polyline points='12 7 12 12 15 14'/>",
            OpenSchedule),
        new ActionVm("Assign Task",
            "<circle cx='12' cy='8' r='4'/><path d='M4 21v-1a6 6 0 0 1 6-6h4a6 6 0 0 1 6 6v1'/>",
            OpenAssign),
        new ActionVm("Import Tasks",
            "<path d='M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4'/><polyline points='17 8 12 3 7 8'/><line x1='12' y1='3' x2='12' y2='15'/>",
            () => ShowToast("Imported tasks from file (mock)")),
        new ActionVm("Task Templates",
            "<line x1='8' y1='6' x2='21' y2='6'/><line x1='8' y1='12' x2='21' y2='12'/><line x1='8' y1='18' x2='21' y2='18'/><line x1='3' y1='6' x2='3.01' y2='6'/><line x1='3' y1='12' x2='3.01' y2='12'/><line x1='3' y1='18' x2='3.01' y2='18'/>",
            () => ShowToast("Opened task templates (mock)")),
        new ActionVm("Bulk Update",
            "<path d='M4 6h16M4 12h16M4 18h10'/><polyline points='16 15 19 18 22 15'/>",
            () => ShowToast($"Bulk update queued for {FilteredCount} tasks (mock)")),
        new ActionVm("Task Reports",
            "<line x1='18' y1='20' x2='18' y2='10'/><line x1='12' y1='20' x2='12' y2='4'/><line x1='6' y1='20' x2='6' y2='14'/>",
            () => ShowToast("Generated task report (mock)")),
        new ActionVm("View All Tasks",
            "<line x1='3' y1='6' x2='21' y2='6'/><line x1='3' y1='12' x2='21' y2='12'/><line x1='3' y1='18' x2='21' y2='18'/>",
            ClearFilters),
    };

    private void OpenCreate()
    {
        _form = new TaskRow { Priority = "Medium", Status = "Pending", DueDate = "31/01/2026" };
        _showCreate = true;
    }

    private void CreateTask()
    {
        if (string.IsNullOrWhiteSpace(_form.Name)) { ShowToast("Task name is required"); return; }
        if (string.IsNullOrWhiteSpace(_form.Client)) _form.Client = "CLIENT-000";
        if (string.IsNullOrWhiteSpace(_form.Campaign)) _form.Campaign = "Unassigned";
        if (string.IsNullOrWhiteSpace(_form.Assignee)) _form.Assignee = "Unassigned";
        _form.Progress = 0;
        _tasks.Insert(0, _form);
        ShowToast($"Task \"{_form.Name}\" created");
        _showCreate = false;
        _page = 1;
    }

    private void OpenAssign()
    {
        if (_selected is null) { ShowToast("Select a task in the table first"); return; }
        _assignName = _selected.Assignee;
        _showAssign = true;
    }

    private void AssignTask()
    {
        if (_selected is null) return;
        var name = string.IsNullOrWhiteSpace(_assignName) ? "Unassigned" : _assignName.Trim();
        _selected.Assignee = name;
        ShowToast($"\"{_selected.Name}\" assigned to {name}");
        _showAssign = false;
    }

    private void OpenSchedule()
    {
        if (_selected is null) { ShowToast("Select a task in the table first"); return; }
        _scheduleDate = "";
        _showSchedule = true;
    }

    private void ScheduleTask()
    {
        if (_selected is null) return;
        var when = string.IsNullOrWhiteSpace(_scheduleDate) ? "next run window" : _scheduleDate.Trim();
        ShowToast($"\"{_selected.Name}\" scheduled for {when}");
        _showSchedule = false;
    }

    private void ClearFilters()
    {
        _search = "";
        _statusFilter = "All Status";
        _page = 1;
        ShowToast("Showing all tasks");
    }

    private async Task ExportTasks()
    {
        var rows = Filtered.ToList();
        if (rows.Count == 0)
        {
            ShowToast("There are no task records to export.");
            return;
        }

        var csv = new StringBuilder();
        AppendCsvRow(csv, "Timestamp", "Agent ID", "Command Type", "Status");
        foreach (var row in rows)
        {
            AppendCsvRow(csv,
                row.Timestamp.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                row.AgentId.ToString(CultureInfo.InvariantCulture),
                row.CommandType,
                row.Status);
        }

        try
        {
            await using var module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/clients.js");
            await module.InvokeVoidAsync("downloadCsv", "task-records.csv", csv.ToString());
        }
        catch (JSException exception)
        {
            _toast = $"Task record CSV download failed: {exception.Message}";
            StateHasChanged();
        }
    }

    private static void AppendCsvRow(StringBuilder csv, params string[] values) =>
        csv.AppendJoin(',', values.Select(EscapeCsvValue)).AppendLine();

    private static string EscapeCsvValue(string value)
    {
        var safeValue = value.TrimStart() is ['=', '+', '-', '@', ..] ? $"'{value}" : value;
        return $"\"{safeValue.Replace("\"", "\"\"")}\"";
    }

    private async void ShowToast(string msg)
    {
        _toast = msg;
        StateHasChanged();
        await Task.Delay(2600);
        if (_toast == msg) { _toast = null; StateHasChanged(); }
    }

    private void CloseModals()
    {
        _showCreate = _showAssign = _showSchedule = false;
    }
}
