using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Trinity.Shared.DTOs.Agent;
using Trinity.Shared.DTOs.Task;

namespace Client.Components.Pages.Clients;

public partial class Clients
{
    private class ClientRow
    {
        public string Name = "";
        public string Ip = "";
        public string Os = "";
        public string Status = "Online";
        public string LastSeen = "just now";
        public string User = "";
        public bool IsNew;
        public int AgentId;
        public bool IsSample;
    }

    private record Event(string When, string Text, string Color);
    private record StatVm(string Label, string Value, string Trend, bool Up, string Icon);
    private record OsVm(string Name, int Count, int Percent, string Color);
    private record ActionVm(string Label, string Icon, Action OnClick);

    private readonly List<ClientRow> _clients = new();
    private readonly List<Event> _timeline = new();
    private ClientRow? _selected;

    private string _search = "";
    private string _statusFilter = "All Status";
    private string _osFilter = "All Architectures";
    private int _page = 1;
    private const int PageSize = 6;

    private string? _toast;
    private string? _loadError;
    private string? _taskLoadError;
    private bool _isLoading;

    [Inject]
    private AgentApiClient AgentApiClient { get; set; } = default!;

    [Inject]
    private TaskApiClient TaskApiClient { get; set; } = default!;

    [Inject]
    private TeamServerConnection Connection { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private CommandApiClient CommandApiClient { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    private void GoToLogin() => Navigation.NavigateTo("/login");

    private bool _showAdd, _showAnnounce, _showExecute, _showShell;
    private ClientRow _form = new();
    private string _announceText = "";
    private string _jobName = "Get UID";
    private string _jobArg = "";
    private string _jobArg2 = "";
    private string _jobArg3 = "";
    private int _jobTargetId;
    private bool _jobImmediate = true;

    // Job catalogue: display name -> the command path suffix sent to the TeamServer.
    private static readonly (string Name, string Path)[] _jobCatalogue =
    {
        ("Get UID",          "execute/getuid"),
        ("Set Sleep",        "execute/setsleep"),
        ("Shell Command",    "spawn/shell"),
        ("PowerShell",       "spawn/powershell"),
        ("Run",              "spawn/run"),
        ("Run As",           "spawn/runas"),
        ("Run U",            "spawn/runu"),
        ("Kill Process",     "spawn/killprocess"),
        ("Escalate (UAC)",   "spawn/escalate"),
        (".NET Assembly",    "spawn/dotnetassembly"),
        ("BOF",              "execute/bof"),
        ("File Download",    "execute/filedownload"),
        ("Cancel Download",  "execute/cancelFileDownload"),
        ("Upload",           "execute/upload"),
        ("Spawn To",         "execute/spawnto"),
        ("Update Hosts",     "execute/updatehosts"),
        ("Kill Agent",       "execute/kill"),
    };

    // Per-job input labels; null entries hide that field. Index 0/1/2 -> _jobArg/_jobArg2/_jobArg3.
    private static (string?, string?, string?) JobLabels(string job) => job switch
    {
        "Set Sleep"       => ("Sleep (ms)", null, null),
        "Shell Command"   => ("Command", null, null),
        "PowerShell"      => ("Cmdlet", "Arguments", null),
        "Run"             => ("Program", "Arguments", null),
        "Run As"          => (@"DOMAIN\user", "Command", "Arguments"),
        "Run U"           => ("PID", "Command", "Arguments"),
        "Kill Process"    => ("PID", null, null),
        "Escalate (UAC)"  => ("Registry key", "Target binary", null),
        ".NET Assembly"   => ("Assembly (base64 or path)", "Arguments", null),
        "BOF"             => ("BOF (base64)", "Entrypoint", "Arguments"),
        "File Download"   => ("Remote path", null, null),
        "Cancel Download" => ("Task ID", null, null),
        "Upload"          => ("Path", null, null),
        "Spawn To"        => ("Path", null, null),
        "Update Hosts"    => ("Hosts (comma-separated)", null, null),
        _ => (null, null, null), // Get UID, Kill Agent: no input
    };

    private string? JobLabel1 => JobLabels(_jobName).Item1;
    private string? JobLabel2 => JobLabels(_jobName).Item2;
    private string? JobLabel3 => JobLabels(_jobName).Item3;
    private string _shellInput = "";
    private readonly List<string> _shellLog = new() { "Trinity remote shell (mock). Type a command and press Run." };

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    private async Task RefreshDataAsync()
    {
        await LoadAgentsAsync();
        await LoadTasksAsync();
    }

    private async Task LoadAgentsAsync()
    {
        _isLoading = true;
        _loadError = null;
        _clients.Clear();
        _selected = null;

        try
        {
            var agents = await AgentApiClient.GetAgentsAsync();
            _clients.AddRange(agents.OrderBy(agent => agent.agentID).Select(ToClientRow));
            _page = 1;
        }
        catch (InvalidOperationException exception)
        {
            _loadError = exception.Message;
        }
        catch (HttpRequestException exception)
        {
            _loadError = $"Could not load agents from TeamServer: {exception.Message}";
        }
        catch (TaskCanceledException exception)
        {
            _loadError = $"The TeamServer agent request timed out: {exception.Message}";
        }
        catch (JsonException exception)
        {
            _loadError = $"TeamServer returned an invalid agent response: {exception.Message}";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private static ClientRow ToClientRow(AgentResponse agent)
    {
        var isSample = agent.username.StartsWith("sample-user-", StringComparison.OrdinalIgnoreCase)
            && agent.externalIP.StartsWith("203.0.113.", StringComparison.Ordinal);
        return new ClientRow
        {
            Name = isSample ? $"SAMPLE-CLIENT-{agent.agentID:000}" : $"CLIENT-{agent.agentID:000}",
            Ip = agent.externalIP,
            Os = agent.arch,
            Status = ComputeStatus(agent.lastCheckIn, agent.sleep),
            LastSeen = agent.lastCheckIn.ToLocalTime().ToString("g"),
            User = agent.username,
            AgentId = agent.agentID,
            IsSample = isSample
        };
    }

    // Derives Online/Offline from check-in recency. The TeamServer has no explicit status,
    // so an agent counts as online if it checked in within ~2.5 sleep intervals (sleep is
    // treated as seconds, floored at 60s so very short sleeps don't flap).
    private static string ComputeStatus(DateTime lastCheckIn, int sleep)
    {
        var window = TimeSpan.FromSeconds(Math.Max(sleep, 60) * 2.5);
        var age = DateTime.UtcNow - lastCheckIn.ToUniversalTime();
        return age <= window ? "Online" : "Offline";
    }

    private async Task LoadTasksAsync()
    {
        _taskLoadError = null;
        _timeline.Clear();
        try
        {
            var tasks = await TaskApiClient.GetTasksAsync();
            var names = _clients.ToDictionary(client => client.AgentId, client => client.Name);
            _timeline.AddRange(tasks.OrderByDescending(task => task.timeStamp).Take(20)
                .Select(task => new Event(
                    task.timeStamp.ToLocalTime().ToString("g"),
                    $"{names.GetValueOrDefault(task.agentID, $"Agent {task.agentID}")} · {task.commandType} · {task.status}",
                    task.status.Equals("Failure", StringComparison.OrdinalIgnoreCase) ? "red" : "green")));
        }
        catch (InvalidOperationException exception)
        {
            _taskLoadError = exception.Message;
        }
        catch (HttpRequestException exception)
        {
            _taskLoadError = $"Could not load task history from TeamServer: {exception.Message}";
        }
        catch (TaskCanceledException exception)
        {
            _taskLoadError = $"The TeamServer task request timed out: {exception.Message}";
        }
        catch (JsonException exception)
        {
            _taskLoadError = $"TeamServer returned an invalid task response: {exception.Message}";
        }
    }

    private IEnumerable<ClientRow> Filtered => _clients.Where(c =>
        (string.IsNullOrWhiteSpace(_search)
            || c.Name.Contains(_search, StringComparison.OrdinalIgnoreCase)
            || c.Ip.Contains(_search, StringComparison.OrdinalIgnoreCase)
            || c.User.Contains(_search, StringComparison.OrdinalIgnoreCase))
        && (_statusFilter == "All Status" || c.Status == _statusFilter)
        && (_osFilter == "All Architectures" || c.Os.StartsWith(_osFilter, StringComparison.OrdinalIgnoreCase)));

    private int FilteredCount => Filtered.Count();
    private int SampleCount => _clients.Count(client => client.IsSample);
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(FilteredCount / (double)PageSize));
    private int CurrentPage => Math.Clamp(_page, 1, TotalPages);
    private IEnumerable<ClientRow> PageItems => Filtered.Skip((CurrentPage - 1) * PageSize).Take(PageSize);

    private string ShowingText
    {
        get
        {
            if (_isLoading) return "Loading agents...";
            if (_loadError is not null) return "Agents unavailable";
            if (FilteredCount == 0) return "No clients to show";
            var start = (CurrentPage - 1) * PageSize + 1;
            var end = Math.Min(CurrentPage * PageSize, FilteredCount);
            return $"Showing {start}–{end} of {FilteredCount} clients";
        }
    }

    private void ResetPage() => _page = 1;
    private void GoToPage(int p) => _page = Math.Clamp(p, 1, TotalPages);
    private void SelectRow(ClientRow row) => _selected = row == _selected ? null : row;

    private static string StatusColor(string status) => status switch
    {
        "Online" => "green",
        "Offline" => "gray",
        _ => "gray",
    };

    private List<StatVm> BuildStats()
    {
        int total = _clients.Count;
        int online = _clients.Count(client => client.Status == "Online");
        int offline = total - online;

        return new List<StatVm>
        {
            new("Total Clients",   total.ToString(),    "From TeamServer", true,
                "<circle cx='12' cy='8' r='4.5'/><path d='M4 21v-1a7 7 0 0 1 7-7h2a7 7 0 0 1 7 7v1'/>"),
            new("Online Clients",  online.ToString(),   "Checked in recently", true,
                "<rect x='2' y='3' width='20' height='14' rx='2'/><line x1='8' y1='21' x2='16' y2='21'/><line x1='12' y1='17' x2='12' y2='21'/>"),
            new("Offline Clients", offline.ToString(),  "No recent check-in", false,
                "<rect x='2' y='3' width='20' height='14' rx='2'/><line x1='8' y1='21' x2='16' y2='21'/><line x1='12' y1='17' x2='12' y2='21'/><circle cx='19.5' cy='4.5' r='2.6' fill='#E5484D' stroke='#1C1C1F' stroke-width='1.6'/>"),
            new("New Clients",     "Not provided",       "TeamServer does not provide this count", true,
                "<circle cx='10' cy='7' r='4'/><path d='M3 21v-1a6 6 0 0 1 6-6h2'/><line x1='19' y1='11' x2='19' y2='17'/><line x1='16' y1='14' x2='22' y2='14'/>"),
            new("Inactive Clients","Not provided", "TeamServer does not provide this count", false,
                "<circle cx='10' cy='7' r='4'/><path d='M3 21v-1a6 6 0 0 1 6-6h3'/><circle cx='18' cy='16' r='4.5'/><path d='M18 14v2l1.3 1'/>"),
        };
    }

    private static readonly (string Name, string Color)[] _families =
    {
        ("x64", "blue"), ("x86", "green"), ("Other", "gray")
    };

    private static string Family(string architecture) =>
        architecture.Equals("x64", StringComparison.OrdinalIgnoreCase) ? "x64" :
        architecture.Equals("x86", StringComparison.OrdinalIgnoreCase) ? "x86" : "Other";

    private List<OsVm> BuildOsBreakdown()
    {
        int total = Math.Max(1, _clients.Count);
        return _families.Select(f =>
        {
            int count = _clients.Count(c => Family(c.Os) == f.Name);
            int pct = (int)Math.Round(count * 100.0 / total);
            return new OsVm(f.Name, count, pct, f.Color);
        }).ToList();
    }

    private string DonutGradient()
    {
        var colorVar = new Dictionary<string, string>
        {
            ["green"] = "var(--status-green)", ["blue"] = "var(--status-blue)",
            ["yellow"] = "var(--status-yellow)", ["red"] = "var(--status-red)", ["gray"] = "var(--text-muted)"
        };
        int total = Math.Max(1, _clients.Count);
        var sb = new StringBuilder();
        double acc = 0;
        var stops = new List<string>();
        foreach (var f in _families)
        {
            int count = _clients.Count(c => Family(c.Os) == f.Name);
            if (count == 0) continue;
            double start = acc / total * 100;
            acc += count;
            double end = acc / total * 100;
            stops.Add($"{colorVar[f.Color]} {start.ToString("0.##", CultureInfo.InvariantCulture)}% {end.ToString("0.##", CultureInfo.InvariantCulture)}%");
        }
        return string.Join(", ", stops);
    }

    private ActionVm[] Actions => new[]
    {
        new ActionVm("Add New Client",
            "<circle cx='9' cy='7' r='4'/><path d='M2 21v-1a6 6 0 0 1 6-6h2'/><line x1='19' y1='11' x2='19' y2='17'/><line x1='16' y1='14' x2='22' y2='14'/>",
            OpenAdd),
        new ActionVm("Send Announcement",
            "<path d='M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z'/>",
            () => { _showAnnounce = true; }),
        new ActionVm("Execute Job",
            "<rect x='2' y='3' width='20' height='14' rx='2'/><line x1='8' y1='21' x2='16' y2='21'/><line x1='12' y1='17' x2='12' y2='21'/>",
            OpenExecute),
        new ActionVm("Lock Client",
            "<rect x='3' y='11' width='18' height='11' rx='2'/><path d='M7 11V7a5 5 0 0 1 10 0v4'/>",
            LockClient),
        new ActionVm("Remote Shell",
            "<polyline points='4 17 10 11 4 5'/><line x1='12' y1='19' x2='20' y2='19'/>",
            OpenShell),
        new ActionVm("View All Clients",
            "<line x1='3' y1='6' x2='21' y2='6'/><line x1='3' y1='12' x2='21' y2='12'/><line x1='3' y1='18' x2='21' y2='18'/>",
            ClearFilters),
    };

    private void OpenAdd()
    {
        _form = new ClientRow { Name = NextClientName(), Os = "Windows 10", Status = "Online" };
        _showAdd = true;
    }

    private string NextClientName()
    {
        int max = _clients
            .Where(c => c.Name.StartsWith("CLIENT-", StringComparison.OrdinalIgnoreCase))
            .Select(c => int.TryParse(c.Name.AsSpan(7), out var v) ? v : 0)
            .DefaultIfEmpty(0)
            .Max();
        return $"CLIENT-{max + 1:000}";
    }

    private void AddClient()
    {
        _form.Name = NextClientName();
        _form.LastSeen = "just now";
        _form.IsNew = true;
        _clients.Insert(0, _form);
        AddTimeline($"{_form.Name} added ({_form.Ip})", "green");
        ShowToast($"{_form.Name} added");
        _showAdd = false;
        _page = 1;
    }

    private void SendAnnouncement()
    {
        var msg = string.IsNullOrWhiteSpace(_announceText) ? "(empty message)" : _announceText.Trim();
        AddTimeline($"Announcement broadcast to all clients: \"{Truncate(msg, 40)}\"", "blue");
        ShowToast($"Announcement sent to {_clients.Count} clients");
        _announceText = "";
        _showAnnounce = false;
    }

    private void OpenExecute()
    {
        if (_clients.Count == 0) { ShowToast("No clients available to target."); return; }
        _jobTargetId = _selected?.AgentId ?? _clients[0].AgentId;
        _jobName = "Get UID";
        _jobArg = "";
        _jobArg2 = "";
        _jobArg3 = "";
        _jobImmediate = true;
        _showExecute = true;
    }

    // Builds the request body for the selected job from the argument fields.
    private object BuildJobBody()
    {
        int ParseInt(string s, int fallback = 0) => int.TryParse(s, out var v) ? v : fallback;

        return _jobName switch
        {
            "Get UID"         => new { },
            "Kill Agent"      => new { },
            "Set Sleep"       => new { Sleep = ParseInt(_jobArg, 5000) },
            "Shell Command"   => new { Command = _jobArg },
            "PowerShell"      => new { Commandlet = _jobArg, Arguements = _jobArg2 },
            "Run"             => new { Program = _jobArg, Arguements = _jobArg2 },
            "Run As"          => BuildRunAsBody(),
            "Run U"           => new { PID = ParseInt(_jobArg), Command = _jobArg2, Arguements = _jobArg3 },
            "Kill Process"    => new { PID = ParseInt(_jobArg) },
            "Escalate (UAC)"  => new { RegKey = _jobArg, TargetBinary = _jobArg2 },
            ".NET Assembly"   => new { Assembly = _jobArg, Arguments = _jobArg2 },
            "BOF"             => new { Bof = _jobArg, Entrypoint = _jobArg2, Arguements = _jobArg3 },
            "File Download"   => new { Path = _jobArg },
            "Cancel Download" => new { TaskID = _jobArg },
            "Upload"          => new { Path = _jobArg },
            "Spawn To"        => new { Path = _jobArg },
            "Update Hosts"    => new { Hosts = _jobArg.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() },
            _ => new { },
        };
    }

    // Run As takes a DOMAIN\user pair; split it, defaulting the domain to "." when omitted.
    private object BuildRunAsBody()
    {
        var parts = _jobArg.Split('\\', 2);
        var (domain, user) = parts.Length == 2 ? (parts[0], parts[1]) : (".", _jobArg);
        return new { Domain = domain, User = user, Command = _jobArg2, Arguements = _jobArg3 };
    }

    private async Task RunJob()
    {
        var agentId = _jobTargetId;
        var targetName = _clients.FirstOrDefault(client => client.AgentId == agentId)?.Name ?? $"Agent {agentId}";

        if (!_jobImmediate)
        {
            ShowToast("Scheduled execution is not supported by the TeamServer; choose Immediate.");
            return;
        }

        var pathSuffix = _jobCatalogue.FirstOrDefault(job => job.Name == _jobName).Path;
        if (string.IsNullOrEmpty(pathSuffix))
        {
            ShowToast($"Unknown job \"{_jobName}\".");
            return;
        }

        try
        {
            var queued = await CommandApiClient.QueueCommandAsync(agentId, pathSuffix, BuildJobBody());

            if (queued)
            {
                AddTimeline($"{targetName} · queued {_jobName}", "green");
                ShowToast($"Queued \"{_jobName}\" on {targetName}");
                _showExecute = false;
            }
            else
            {
                ShowToast($"TeamServer rejected the {_jobName} command.");
            }
        }
        catch (InvalidOperationException exception)
        {
            ShowToast(exception.Message);
        }
        catch (HttpRequestException exception)
        {
            ShowToast($"Could not reach the TeamServer: {exception.Message}");
        }
    }

    private void LockClient()
    {
        if (_selected is null) { ShowToast("Select a client in the table first"); return; }
        _selected.Status = "Inactive";
        AddTimeline($"{_selected.Name} locked by operator", "yellow");
        ShowToast($"{_selected.Name} locked");
    }

    private void OpenShell()
    {
        if (_selected is null) { ShowToast("Select a client in the table first"); return; }
        _showShell = true;
    }

    private void ShellKey(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") RunShell();
    }

    private void RunShell()
    {
        var cmd = _shellInput.Trim();
        if (cmd.Length == 0) return;
        _shellLog.Add($"> {cmd}");
        _shellLog.Add(MockShellResponse(cmd));
        _shellInput = "";
    }

    private string MockShellResponse(string cmd) => cmd.ToLowerInvariant() switch
    {
        "whoami" => $"{_selected?.User ?? "operator"} (mock)",
        "hostname" => _selected?.Name ?? "unknown",
        "ipconfig" or "ifconfig" => _selected?.Ip ?? "0.0.0.0",
        "ls" or "dir" => "Documents  Downloads  report.pdf  config.json",
        _ => $"'{cmd}' executed (mock) — no real command was run",
    };

    private void ClearFilters()
    {
        _search = "";
        _statusFilter = "All Status";
        _osFilter = "All Architectures";
        _page = 1;
        ShowToast("Showing all clients");
    }

    private async Task ExportClients()
    {
        if (FilteredCount == 0) { ShowToast("No clients to export."); return; }

        var sb = new StringBuilder();
        sb.AppendLine("Name,IP Address,Architecture,Status,Last Seen,User");
        foreach (var c in Filtered)
        {
            sb.AppendLine(string.Join(",", new[] { c.Name, c.Ip, c.Os, c.Status, c.LastSeen, c.User }.Select(CsvField)));
        }

        try
        {
            var module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/clients.js");
            await module.InvokeVoidAsync("downloadCsv", $"trinity-clients-{DateTime.Now:yyyyMMdd-HHmmss}.csv", sb.ToString());
            ShowToast($"Exported {FilteredCount} clients to CSV.");
        }
        catch (JSException exception)
        {
            ShowToast($"CSV export failed: {exception.Message}");
        }
    }

    // Quotes a CSV field when it contains a comma, quote, or newline.
    private static string CsvField(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
        return value;
    }

    private void AddTimeline(string text, string color) => _timeline.Insert(0, new Event("just now", text, color));

    private async void ShowToast(string msg)
    {
        _toast = msg;
        StateHasChanged();
        await Task.Delay(2600);
        if (_toast == msg) { _toast = null; StateHasChanged(); }
    }

    private void CloseModals()
    {
        _showAdd = _showAnnounce = _showExecute = _showShell = false;
    }

    private static string Truncate(string s, int n) => s.Length <= n ? s : s[..n] + "…";
    private static string Fmt(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);

    private record Marker(double X, double Y, string Color);

    private readonly List<(double X, double Y)> LandDots = new();


    //chat generate for testying
    private static readonly (double Cx, double Cy, double Rx, double Ry)[] _continents =
    {
        (22, 15, 12, 9),
        (24, 24, 3, 4),
        (30, 35, 6, 10),
        (40, 8, 4, 3),
        (50, 14, 5, 4),
        (52, 30, 8, 11),
        (68, 15, 16, 9),
        (67, 26, 4, 4),
        (80, 30, 6, 3),
        (85, 38, 6, 4),
    };

    private static readonly Marker[] MapMarkers = [];

    private void BuildLandDots()
    {
    }
}
