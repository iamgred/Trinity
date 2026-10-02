using System.Globalization;

namespace Client.Components.Pages.Tasks;
//All of this is mock data for UI testing, can be removed without issue
public partial class Tasks
{
    private class TaskRow
    {
        public string Name = "";
        public string Client = "";
        public string Campaign = "";
        public string Priority = "Medium";
        public string Status = "Pending";
        public string Assignee = "";
        public string DueDate = "31/01/2026";
        public int Progress;
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
    private string _priorityFilter = "All Priority";
    private string _assigneeFilter = "All Assignees";
    private int _page = 1;
    private const int PageSize = 6;

    private string? _toast;

    private bool _showCreate, _showAssign, _showSchedule;
    private TaskRow _form = new();
    private string _assignName = "";
    private string _scheduleDate = "";

    protected override void OnInitialized()
    {
        _tasks.AddRange(new[]
        {
            new TaskRow { Name="Create Security Report", Client="CLIENT-001", Campaign="Phishing Simulation",     Priority="High",   Status="Completed",   Assignee="John Doe",       Progress=100 },
            new TaskRow { Name="Malware Analysis",        Client="CLIENT-003", Campaign="Malware Simulation",      Priority="Medium", Status="In Progress", Assignee="Steven Strange", Progress=65 },
            new TaskRow { Name="Vulnerability Scan",      Client="CLIENT-002", Campaign="Vulnerability Assessment",Priority="High",   Status="In Progress", Assignee="Carlos Sainz",   Progress=40 },
            new TaskRow { Name="Review Access Logs",      Client="CLIENT-002", Campaign="Security Audit",          Priority="High",   Status="Completed",   Assignee="Jane Smith",     Progress=100 },
            new TaskRow { Name="Penetration Test",        Client="CLIENT-004", Campaign="Security Assessment",     Priority="Medium", Status="Pending",     Assignee="Elton John",     Progress=0 },
            new TaskRow { Name="Policy Documentation",    Client="CLIENT-006", Campaign="Compliance Review",       Priority="Low",    Status="In Progress", Assignee="Siya Kholisi",   Progress=30 },
            new TaskRow { Name="Employee Training",       Client="CLIENT-001", Campaign="Security Training",       Priority="High",   Status="Pending",     Assignee="Edward Kenway",  Progress=0 },
            new TaskRow { Name="Update Firewall",         Client="CLIENT-005", Campaign="Security Assessment",     Priority="Low",    Status="In Progress", Assignee="Trevor Belmont", Progress=55 },
            new TaskRow { Name="Backup Critical Data",    Client="CLIENT-005", Campaign="Data Protection",         Priority="Medium", Status="Pending",     Assignee="Tony Stark",     Progress=0 },
        });

        _upcoming.AddRange(new[]
        {
            new UpcomingRow("High",   "Malware Analysis",       "CLIENT-003", "Malware Simulation",          "Trevor Belmont", "31/01/2026", "Due Tomorrow", "red"),
            new UpcomingRow("Medium", "Create Security Report", "CLIENT-002", "Security Awareness Training", "Edward Kenway",  "31/01/2026", "2 Days Left",  "yellow"),
            new UpcomingRow("High",   "Review Access Logs",     "CLIENT-002", "Security Audit",              "Tony Stark",     "31/01/2026", "2 Days Left",  "yellow"),
            new UpcomingRow("Low",    "Backup Critical Data",   "CLIENT-005", "Data Protection",             "Siya Kholisi",   "31/01/2026", "4 Days Left",  "green"),
        });
    }

    private IEnumerable<string> Assignees => _tasks.Select(t => t.Assignee).Distinct().OrderBy(a => a);

    private IEnumerable<TaskRow> Filtered => _tasks.Where(t =>
        (string.IsNullOrWhiteSpace(_search)
            || t.Name.Contains(_search, StringComparison.OrdinalIgnoreCase)
            || t.Client.Contains(_search, StringComparison.OrdinalIgnoreCase)
            || t.Campaign.Contains(_search, StringComparison.OrdinalIgnoreCase)
            || t.Assignee.Contains(_search, StringComparison.OrdinalIgnoreCase))
        && (_statusFilter == "All Status" || t.Status == _statusFilter)
        && (_priorityFilter == "All Priority" || t.Priority == _priorityFilter)
        && (_assigneeFilter == "All Assignees" || t.Assignee == _assigneeFilter));

    private int FilteredCount => Filtered.Count();
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(FilteredCount / (double)PageSize));
    private int CurrentPage => Math.Clamp(_page, 1, TotalPages);
    private IEnumerable<TaskRow> PageItems => Filtered.Skip((CurrentPage - 1) * PageSize).Take(PageSize);

    private string ShowingText
    {
        get
        {
            if (FilteredCount == 0) return "No tasks to show";
            var start = (CurrentPage - 1) * PageSize + 1;
            var end = Math.Min(CurrentPage * PageSize, FilteredCount);
            return $"Showing {start}–{end} of {FilteredCount} Tasks";
        }
    }

    private void ResetPage() => _page = 1;
    private void GoToPage(int p) => _page = Math.Clamp(p, 1, TotalPages);
    private void SelectRow(TaskRow row) => _selected = row == _selected ? null : row;

    private static string PriorityColor(string priority) => priority switch
    {
        "High" => "red",
        "Medium" => "yellow",
        "Low" => "green",
        _ => "gray",
    };

    private static string StatusColor(string status) => status switch
    {
        "Completed" => "green",
        "In Progress" => "blue",
        "Pending" => "yellow",
        _ => "gray",
    };

    private List<StatVm> BuildStats()
    {
        int total = _tasks.Count;
        int completed = _tasks.Count(t => t.Status == "Completed");
        int inProgress = _tasks.Count(t => t.Status == "In Progress");
        int pending = _tasks.Count(t => t.Status == "Pending");
        int overdue = _tasks.Count(t => t.Status != "Completed" && t.Priority == "High");

        return new List<StatVm>
        {
            new("Total Tasks", total.ToString(), "16% from last 24h", true,
                "<rect x='8' y='2' width='8' height='4' rx='1'/><path d='M9 4H6a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2h-3'/>"),
            new("Completed Tasks", completed.ToString(), "24% from last 24h", true,
                "<path d='M22 11.08V12a10 10 0 1 1-5.93-9.14'/><polyline points='22 4 12 14.01 9 11.01'/>"),
            new("In Progress", inProgress.ToString(), "12% from last 24h", true,
                "<circle cx='12' cy='12' r='9'/><polyline points='12 7 12 12 15 14'/>"),
            new("Pending Tasks", pending.ToString(), "5% from last 24h", true,
                "<rect x='6' y='5' width='4' height='14' rx='1'/><rect x='14' y='5' width='4' height='14' rx='1'/>"),
            new("Overdue Tasks", overdue.ToString(), "33% from last 24h", false,
                "<circle cx='12' cy='12' r='9'/><line x1='15' y1='9' x2='9' y2='15'/><line x1='9' y1='9' x2='15' y2='15'/>"),
        };
    }

    private List<SliceVm> BuildPriorityBreakdown()
    {
        int total = Math.Max(1, _tasks.Count);
        var order = new[] { ("High", "red"), ("Medium", "yellow"), ("Low", "green") };
        return order.Select(o =>
        {
            int count = _tasks.Count(t => t.Priority == o.Item1);
            return new SliceVm(o.Item1, count, (int)Math.Round(count * 100.0 / total), o.Item2);
        }).ToList();
    }

    private List<SliceVm> BuildOverviewBreakdown()
    {
        int total = Math.Max(1, _tasks.Count);
        var order = new[] { ("Completed", "green"), ("In Progress", "blue"), ("Pending", "yellow") };
        return order.Select(o =>
        {
            int count = _tasks.Count(t => t.Status == o.Item1);
            return new SliceVm(o.Item1, count, (int)Math.Round(count * 100.0 / total), o.Item2);
        }).ToList();
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
        _priorityFilter = "All Priority";
        _assigneeFilter = "All Assignees";
        _page = 1;
        ShowToast("Showing all tasks");
    }

    private void ExportTasks() => ShowToast($"Exported {FilteredCount} tasks to CSV (mock)");

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
