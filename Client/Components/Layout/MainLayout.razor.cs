using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using Client.Components.Shell;
using Client.Services;
using Trinity.Shared.DTOs.Notification;
using Trinity.Shared.Enums.Tasks;

namespace Client.Components.Layout;

public partial class MainLayout : LayoutComponentBase, IAsyncDisposable
{
    [Inject] IConfiguration Config { get; set; } = null!;
    [Inject] IJSRuntime JS { get; set; } = null!;
    [Inject] NavigationManager Nav { get; set; } = null!;
    [Inject] EventLogService EventLog { get; set; } = null!;
    [Inject] TeamServerConnection Connection { get; set; } = null!;
    [Inject] ProtectedSessionStorage Session { get; set; } = null!;

    private readonly MudTheme _theme = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = "#E87722",
            PrimaryContrastText = "#FFFFFF",
            Background = "#141414",
            Surface = "#1E1E1E",
            AppbarBackground = "#111111",
            DrawerBackground = "#111111",
            DrawerText = "rgba(255,255,255,0.87)",
            TextPrimary = "rgba(255,255,255,0.87)",
            TextSecondary = "rgba(255,255,255,0.60)",
            ActionDefault = "rgba(255,255,255,0.54)",
            Divider = "rgba(255,255,255,0.12)",
            TableHover = "rgba(255,255,255,0.04)",
            TableLines = "rgba(255,255,255,0.12)",
            LinesDefault = "rgba(255,255,255,0.12)",
        }
    };

    private record NavItem(string Label, string Href, string Icon, NavLinkMatch Match = NavLinkMatch.Prefix);

    private readonly NavItem[] _navItems =
    [
        new("Dashboard", "/dashboard", Icons.Material.Filled.Dashboard, NavLinkMatch.All),
        new("Listeners", "/listeners", Icons.Material.Filled.Router),
        new("Agents", "/agents", Icons.Material.Filled.Computer),
        new("Pivot", "/pivot", Icons.Material.Filled.AccountTree),
        new("Payload", "/payload", Icons.Material.Filled.Code),
        new("Admin", "/admin", Icons.Material.Filled.AdminPanelSettings),
    ];

    private int _bottomH = 220;
    private void SetBottomTab(int idx) { _workspaceState.ActiveBottomTab = idx; }
    private bool _serverOnline = false;
    private string _operatorName = "operator";
    private bool _redirecting = false;
    private HubConnection? _hub;
    private readonly HashSet<int> _subscribedAgents = [];
    private readonly WorkspaceState _workspaceState = new();

    protected override void OnInitialized()
    {
        if (Connection.Token is null)
        {
            _redirecting = true;
            Nav.NavigateTo("/login", replace: true);
            return;
        }

        _workspaceState.Changed += () => InvokeAsync(async () =>
        {
            await SyncAgentSubscriptionsAsync();
            StateHasChanged();
        });
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || _redirecting) return;
        _bottomH = await JS.InvokeAsync<int>("trinityResize.getHalfHeight");
        await InitResizeAsync();
        await ConnectHubAsync();
        try
        {
            var result = await Session.GetAsync<ShellUser>(ShellUser.StorageKey);
            if (result.Success && result.Value is not null)
                _operatorName = result.Value.Name;
        }
        catch { }
        StateHasChanged();
    }

    private async Task LogoutAsync()
    {
        try { await Session.DeleteAsync(ShellUser.StorageKey); } catch { }
        Connection.ClearToken();
        Nav.NavigateTo("/login", replace: true);
    }

    private async Task InitResizeAsync()
    {
        await JS.InvokeVoidAsync("trinityResize.init", DotNetObjectReference.Create(this), "bottom-panel", 140);
    }

    private async Task ConnectHubAsync()
    {
        var baseUrl = Config["TeamServer:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl)) return;
        try
        {
            _hub = new HubConnectionBuilder()
                .WithUrl($"{baseUrl}/register")
                .WithAutomaticReconnect()
                .Build();

            _hub.On<TaskNotification>("TaskNotification", async n =>
            {
                var level = n.Status == Status.ERROR ? "ERROR" : "INFO";
                AddEvent(level, $"Task #{n.TaskID}: {n.message}");
                _workspaceState.NotifyAgentsChanged();
                await InvokeAsync(StateHasChanged);
            });

            _hub.On<EventLog>("EventLogNotification", async ev =>
            {
                AddEvent(ev.Type.ToString(), ev.message);
                _workspaceState.NotifyAgentsChanged();
                await InvokeAsync(StateHasChanged);
            });

            _hub.On("AgentPulse", () =>
            {
                _workspaceState.NotifyAgentsChanged();
                InvokeAsync(StateHasChanged);
            });

            _hub.Reconnecting += _ => { _serverOnline = false; InvokeAsync(StateHasChanged); return Task.CompletedTask; };
            _hub.Reconnected += async _ => { _serverOnline = true; await SyncAgentSubscriptionsAsync(); await InvokeAsync(StateHasChanged); };

            await _hub.StartAsync();
            _serverOnline = true;
        }
        catch
        {
            _serverOnline = false;
        }
        await InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public void SetPanelHeight(int px) { _bottomH = px; InvokeAsync(StateHasChanged); }

    private static string SevClass(string level) => level switch { "ERROR" or "CRIT" => "sev-error", "WARN" => "sev-warn", "SUCCESS" => "sev-ok", _ => "sev-info" };
    private void AddEvent(string level, string msg) => EventLog.Add(level, msg);

    private async Task SyncAgentSubscriptionsAsync()
    {
        if (_hub is null || _hub.State != HubConnectionState.Connected) return;

        var desired = _workspaceState.OpenAgentTerminals.Select(a => a.agentID).ToHashSet();

        foreach (var id in desired.Except(_subscribedAgents))
        {
            await _hub.SendAsync("SubscribeToAgent", id.ToString());
            _subscribedAgents.Add(id);
        }

        foreach (var id in _subscribedAgents.Except(desired).ToList())
        {
            _subscribedAgents.Remove(id);
        }
    }

    private bool IsOnAgentsPage
    {
        get
        {
            var rel = Nav.ToBaseRelativePath(Nav.Uri);
            return rel == "" || rel.StartsWith("agents", StringComparison.OrdinalIgnoreCase);
        }
    }

    private string AgentsNavClass(string view) =>
        IsOnAgentsPage && _workspaceState.AgentView == view ? "c2-nav-item active" : "c2-nav-item";

    private void NavigateToAgentView(string view)
    {
        _workspaceState.SetAgentView(view);
        if (!IsOnAgentsPage) Nav.NavigateTo("/agents");
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub is not null) await _hub.DisposeAsync();
    }
}