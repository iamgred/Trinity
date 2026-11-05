using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using Client.Components.UI;
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

    private readonly DialogOptions _dialogOptions = new() { MaxWidth = MaxWidth.Small, FullWidth = true, CloseOnEscapeKey = true };

    private readonly List<TaskRow> _tasks = new();
    private readonly List<UpcomingRow> _upcoming = new();
    private TaskRow? _selected;

    private string _search = "";
    private string _statusFilter = "All Status";
    private int _page; // MudTable pages start at 0
    private const int PageSize = 6;

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

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

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
            _page = 0;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException or InvalidOperationException)
        {
            _loadError = ServerDown.Is(ex) ? ServerDown.Message : $"Couldn't load the task records: {ex.Message}";
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

    // filters go back to the first page
    private void OnSearch(string? value)
    {
        _search = value ?? "";
        _page = 0;
    }

    private void OnStatusFilter(string value)
    {
        _statusFilter = value;
        _page = 0;
    }

    private void SelectRow(TaskRow row) => _selected = row == _selected ? null : row;

    private string RowStyle(TaskRow row, int index) => row == _selected ? "cursor:pointer;background:rgba(232,119,34,0.13);" : "cursor:pointer;";

    private async Task OnDetailKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") await LookupTaskDetailAsync();
    }

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
            _detailError = ServerDown.Is(exception) ? ServerDown.Message : $"Couldn't load task detail from TeamServer: {exception.Message}";
        }
        catch (TaskCanceledException exception)
        {
            _detailError = ServerDown.Is(exception) ? ServerDown.Message : $"The TeamServer task-detail request timed out: {exception.Message}";
        }
        catch (JsonException exception)
        {
            _detailError = ServerDown.Is(exception) ? ServerDown.Message : $"TeamServer returned an invalid task-detail response: {exception.Message}";
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

    private static string StatusColor(string status) => TaskStatuses.Kind(status) switch
    {
        TaskStatusKind.Success => "green",
        TaskStatusKind.Failed => "red",
        TaskStatusKind.Running => "blue",
        TaskStatusKind.Queued => "yellow",
        _ => "gray",
    };

    private List<StatVm> BuildStats()
    {
        int total = _tasks.Count;
        int completed = _tasks.Count(t => TaskStatuses.Kind(t.Status) == TaskStatusKind.Success);
        int failed = _tasks.Count(t => TaskStatuses.Kind(t.Status) == TaskStatusKind.Failed);
        int running = _tasks.Count(t => TaskStatuses.Kind(t.Status) == TaskStatusKind.Running);
        int queued = _tasks.Count(t => TaskStatuses.Kind(t.Status) == TaskStatusKind.Queued);

        return new List<StatVm>
        {
            new("Total Tasks", total.ToString(), "From TeamServer", true, Icons.Material.Outlined.Assignment),
            new("Successful Tasks", completed.ToString(), "Reported status", true, Icons.Material.Outlined.CheckCircle),
            new("Running", running.ToString(), "Reported status", true, Icons.Material.Outlined.Schedule),
            new("Queued", queued.ToString(), "Reported status", true, Icons.Material.Outlined.PauseCircle),
            new("Failed Tasks", failed.ToString(), "Reported status", false, Icons.Material.Outlined.Cancel),
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

    // one series, one slice per status
    private static List<ChartSeries<double>> DonutSeries(List<SliceVm> slices) =>
        [new ChartSeries<double> { Name = "Tasks", Data = slices.Select(x => (double)x.Count).ToArray() }];

    private static DonutChartOptions DonutOptions(List<SliceVm> slices) => new()
    {
        ShowLegend = false,
        ChartPalette = slices.Select(x => TrinityTheme.Hex(x.Color)).ToArray(),
    };

    private ActionVm[] Actions => new[]
    {
        new ActionVm("Create New Task", Icons.Material.Filled.Add, OpenCreate),
        new ActionVm("Schedule Task", Icons.Material.Filled.Schedule, OpenSchedule),
        new ActionVm("Assign Task", Icons.Material.Filled.PersonAdd, OpenAssign),
        new ActionVm("Import Tasks", Icons.Material.Filled.Upload, () => ShowToast("Imported tasks from file (mock)")),
        new ActionVm("Task Templates", Icons.Material.Filled.ListAlt, () => ShowToast("Opened task templates (mock)")),
        new ActionVm("Bulk Update", Icons.Material.Filled.DynamicFeed, () => ShowToast($"Bulk update queued for {FilteredCount} tasks (mock)")),
        new ActionVm("Task Reports", Icons.Material.Filled.BarChart, () => ShowToast("Generated task report (mock)")),
        new ActionVm("View All Tasks", Icons.Material.Filled.FormatListBulleted, ClearFilters),
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
        _page = 0;
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
        _page = 0;
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
            Snackbar.Add($"Task record CSV download failed: {exception.Message}", Severity.Error);
        }
    }

    private static void AppendCsvRow(StringBuilder csv, params string[] values) =>
        csv.AppendJoin(',', values.Select(EscapeCsvValue)).AppendLine();

    private static string EscapeCsvValue(string value)
    {
        var safeValue = value.TrimStart() is ['=', '+', '-', '@', ..] ? $"'{value}" : value;
        return $"\"{safeValue.Replace("\"", "\"\"")}\"";
    }

    private void ShowToast(string msg) => Snackbar.Add(msg, Severity.Info);

    private void CloseModals()
    {
        _showCreate = _showAssign = _showSchedule = false;
    }
}
