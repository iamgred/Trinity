using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Client.Services;
using Client.Components.Shell;
using Trinity.Shared.DTOs.Agent;

namespace Client.Components.Pages;

public partial class Agents : IDisposable
{
    [Inject] AgentService AgentService { get; set; } = null!;
    [Inject] NavigationManager Nav { get; set; } = null!;
    [Inject] IJSRuntime JS { get; set; } = null!;
    [CascadingParameter] private WorkspaceState Workspace { get; set; } = default!;

    private System.Timers.Timer? _statusTimer;

    private List<AgentResponse> _agents = [];
    private AgentResponse? _selectedAgent = null;
    private bool _loading = false;
    private string _search = string.Empty;
    private string _filterStatus = string.Empty;
    private string? _latestSeen = null;

    private AgentResponse? _contextAgent;
    private double _menuX, _menuY;
    private bool _menuOpen;

    // graph state — view is driven by WorkspaceState.AgentView
    private string _view => Workspace.AgentView;
    private string _svgViewBox = "0 0 850 500";
    private double _zoom = 1.0;
    private bool _graphPanReady = false;

    private int SvgW => 850;
    private int SvgH => Math.Max(500, _agents.Count * 105 + 100);

    private int OnlineCount => _agents.Count(IsOnline);

    private IEnumerable<AgentResponse> FilteredAgents
    {
        get
        {
            var query = _agents.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(_search))
                query = query.Where(a =>
                    (a.username?.Contains(_search, StringComparison.OrdinalIgnoreCase) == true) ||
                    (a.externalIP?.Contains(_search, StringComparison.OrdinalIgnoreCase) == true));
            if (_filterStatus == "online") query = query.Where(IsOnline);
            else if (_filterStatus == "offline") query = query.Where(a => !IsOnline(a));
            return query;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        Workspace.Changed += OnWorkspaceChanged;
        Workspace.AgentsChanged += OnAgentsChanged;
        await LoadAgentsAsync();

        _statusTimer = new System.Timers.Timer(10_000);
        _statusTimer.Elapsed += (_, _) => InvokeAsync(StateHasChanged);
        _statusTimer.AutoReset = true;
        _statusTimer.Start();
    }

    private void OnWorkspaceChanged()
    {
        if (_view == "graph") _graphPanReady = false;
        InvokeAsync(StateHasChanged);
    }

    private void OnAgentsChanged() => InvokeAsync(LoadAgentsAsync);

    public void Dispose()
    {
        Workspace.Changed -= OnWorkspaceChanged;
        Workspace.AgentsChanged -= OnAgentsChanged;
        _statusTimer?.Dispose();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_view == "graph" && !_graphPanReady)
        {
            _graphPanReady = true;
            var r = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("trinityResize.initGraphPan", r, "ag-graph-viewport");
        }
    }

    private async Task LoadAgentsAsync()
    {
        _loading = true;
        try
        {
            _agents = await AgentService.GetTasksAsync();
            var latest = _agents.Where(a => a.lastCheckIn != default).OrderByDescending(a => a.lastCheckIn).FirstOrDefault();
            _latestSeen = latest is null ? null : FormatLastSeen(latest.lastCheckIn);
            _svgViewBox = $"0 0 {SvgW} {SvgH}";
        }
        catch (InvalidOperationException) { _agents = []; }
        catch { _agents = []; }
        finally { _loading = false; }
    }


    [JSInvokable] public void SyncViewBox(string vb) { _svgViewBox = vb; }

    private void ZoomIn()  { _zoom = Math.Min(_zoom + 0.2, 3.0); UpdateViewBox(); }
    private void ZoomOut() { _zoom = Math.Max(_zoom - 0.2, 0.3); UpdateViewBox(); }
    private void ResetZoom() { _zoom = 1.0; _svgViewBox = $"0 0 {SvgW} {SvgH}"; }

    private void UpdateViewBox()
    {
        double w = SvgW / _zoom, h = SvgH / _zoom;
        double x = (SvgW - w) / 2, y = (SvgH - h) / 2;
        _svgViewBox = $"{x:F0} {y:F0} {w:F0} {h:F0}";
    }

    private static bool IsOnline(AgentResponse a) =>
        a.lastCheckIn != default && (DateTime.UtcNow - a.lastCheckIn).TotalMilliseconds < (a.sleep * 1.5);

    private static string FormatLastSeen(DateTime dt)
    {
        if (dt == default) return "Never";
        var diff = DateTime.UtcNow - dt;
        return diff.TotalSeconds < 10 ? "Just now"
             : diff.TotalMinutes < 1 ? $"{(int)diff.TotalSeconds}s ago"
             : diff.TotalHours   < 1 ? $"{(int)diff.TotalMinutes}m ago"
             : diff.TotalDays    < 1 ? $"{(int)diff.TotalHours}h ago"
             : dt.ToString("dd MMM HH:mm");
    }

    private static string FormatSleep(int ms)
    {
        if (ms <= 0) return "—";
        return ms < 1000 ? $"{ms}ms" : ms % 1000 == 0 ? $"{ms / 1000}s" : $"{ms / 1000.0:F1}s";
    }

    private static string Shorten(string? value, int max = 12)
    {
        if (string.IsNullOrWhiteSpace(value)) return "?";
        return value.Length <= max ? value : value[..max] + "…";
    }

    private void SelectAgent(AgentResponse agent)
    {
        _selectedAgent = _selectedAgent?.agentID == agent.agentID ? null : agent;
    }

    private void OnAgentSelected(AgentResponse? agent) => _selectedAgent = agent;

    private void InteractWithAgent(AgentResponse agent)
    {
        if (!IsOnline(agent)) return;
        Workspace.OpenAgentTerminal(agent);
    }

    private void ShowContextMenu(MouseEventArgs e, AgentResponse agent)
    {
        _contextAgent = agent;
        _menuX = e.ClientX;
        _menuY = e.ClientY;
        _menuOpen = true;
    }

    private void CloseContextMenu() => _menuOpen = false;

    private void OpenTerminalFromContext()
    {
        if (_contextAgent is not null)
            InteractWithAgent(_contextAgent);
        CloseContextMenu();
    }

    private void NavigateToLoot()
    {
        Nav.NavigateTo($"/loot?agent={_contextAgent?.agentID}");
        CloseContextMenu();
    }
}
