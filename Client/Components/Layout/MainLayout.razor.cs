using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using Client.Services;
using Client.Interpreter;
using Client.Components.Shell;
using System.Net.Http.Json;
using System.Text.Encodings.Web;

namespace Client.Components.Layout;

public partial class MainLayout : LayoutComponentBase, IAsyncDisposable
{
    [Inject] IConfiguration Config { get; set; } = null!;
    [Inject] IJSRuntime JS { get; set; } = null!;
    [Inject] CommandInterpreter Interpreter { get; set; } = null!;
    [Inject] TaskApiService TaskService { get; set; } = null!;
    [Inject] PowerShellService PSService { get; set; } = null!;

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
    private string _termHtml = "<span style='color:#E87722;font-weight:600;'>Trinity C2 v2.0 — Terminal Ready</span>\n\n";
    private string _termInput = string.Empty;
    private int _activeAgent = 0;
    private readonly List<string> _history = [];
    private int _historyIdx = -1;
    private HubConnection? _hub;
    private readonly List<EventEntry> _events = [];
    private record EventEntry(string Stamp, string Level, string Message);
    private readonly WorkspaceState _workspaceState = new();

    protected override void OnInitialized()
    {
        _workspaceState.Changed += () => InvokeAsync(StateHasChanged);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        await InitResizeAsync();
        await ConnectHubAsync();
    }

    private async Task InitResizeAsync()
    {
        await JS.InvokeVoidAsync("trinityResize.init", DotNetObjectReference.Create(this), "bottom-panel", 140, 600);
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

            _hub.On<string>("RecieveTaskOutput", async output =>
            {
                AppendTerm($"<span style='color:#00ff41;'>{HtmlEnc(output)}</span>");
                AddEvent("INFO", output);
                await InvokeAsync(StateHasChanged);
                await JS.InvokeVoidAsync("trinityResize.scrollTerm", "term-output");
            });

            _hub.Reconnecting += _ => { _serverOnline = false; InvokeAsync(StateHasChanged); return Task.CompletedTask; };
            _hub.Reconnected += _ => { _serverOnline = true; InvokeAsync(StateHasChanged); return Task.CompletedTask; };

            await _hub.StartAsync();
            _serverOnline = true;
            AppendTerm("<span style='color:#4CAF50;'>[+] Connected to TeamServer</span>\n");
        }
        catch
        {
            _serverOnline = false;
            AppendTerm("<span style='color:#F44336;'>[-] TeamServer unreachable — check config</span>\n");
        }
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnTermKeyDown(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "Enter": await ExecuteTermCommand(); break;
            case "ArrowUp":
                if (_history.Count > 0) { _historyIdx = Math.Min(_historyIdx + 1, _history.Count - 1); _termInput = _history[_history.Count - 1 - _historyIdx]; }
                break;
            case "ArrowDown":
                if (_historyIdx > 0) { _historyIdx--; _termInput = _history[_history.Count - 1 - _historyIdx]; }
                else { _historyIdx = -1; _termInput = string.Empty; }
                break;
            case "l" when e.CtrlKey:
                _termHtml = string.Empty; _termInput = string.Empty;
                break;
        }
    }

    private async Task ExecuteTermCommand()
    {
        var raw = _termInput.Trim();
        if (string.IsNullOrEmpty(raw)) return;
        _history.Add(raw);
        _historyIdx = -1;
        AppendTerm($"<span style='color:#E87722;'>trinity@c2 ~$</span> <span style='color:#ccc;'>{HtmlEnc(raw)}</span>\n");
        _termInput = string.Empty;
        await InvokeAsync(StateHasChanged);
        var parts = raw.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var cmd = parts[0].ToLowerInvariant();
        var args = parts.Length > 1 ? parts[1] : string.Empty;
        switch (cmd)
        {
            case "help": AppendTerm(HelpText()); break;
            case "clear": _termHtml = string.Empty; break;
            case "interact":
                if (int.TryParse(args, out var agId)) { _activeAgent = agId; AppendTerm($"<span style='color:#FFC107;'>[*] Now interacting with agent {agId}</span>\n"); }
                else AppendTerm("<span style='color:#F44336;'>Usage: interact &lt;agent_id&gt;</span>\n");
                break;
            case "deselect":
            case "back":
                _activeAgent = 0; AppendTerm("<span style='color:#90CAF9;'>[*] Deselected agent</span>\n"); break;
            case "shell":
                if (string.IsNullOrWhiteSpace(args)) AppendTerm("<span style='color:#F44336;'>Usage: shell &lt;command&gt;</span>\n");
                else { try { var result = await PSService.ExecuteAsync(args); AppendTerm($"<span style='color:#ccc;'>{HtmlEnc(result)}</span>\n"); } catch (Exception ex) { AppendTerm($"<span style='color:#F44336;'>Error: {HtmlEnc(ex.Message)}</span>\n"); } }
                break;
            case "whoami": AppendTerm("<span style='color:#ccc;'>Administrator (Team Lead)</span>\n"); break;
            case "status": AppendTerm(_serverOnline ? "<span style='color:#4CAF50;'>[+] TeamServer: Online</span>\n" : "<span style='color:#F44336;'>[-] TeamServer: Offline</span>\n"); break;
            default:
                if (_activeAgent > 0)
                {
                    AppendTerm($"<span style='color:#FFC107;'>[*] Command queued: {HtmlEnc(raw)}</span>\n");
                }
                else AppendTerm($"<span style='color:#F44336;'>Unknown command: {HtmlEnc(cmd)}. Type 'help'.</span>\n");
                break;
        }
        await InvokeAsync(StateHasChanged);
        await JS.InvokeVoidAsync("trinityResize.scrollTerm", "term-output");
    }

    [JSInvokable]
    public void SetPanelHeight(int px) { _bottomH = px; InvokeAsync(StateHasChanged); }

    private void AppendTerm(string html) => _termHtml += html;
    private static string HtmlEnc(string s) => HtmlEncoder.Default.Encode(s);
    private static string SevClass(string level) => level switch { "ERROR" or "CRIT" => "sev-error", "WARN" => "sev-warn", "SUCCESS" => "sev-ok", _ => "sev-info" };
    private void AddEvent(string level, string msg) => _events.Add(new EventEntry(DateTime.Now.ToString("HH:mm:ss"), level, msg));

    private static string HelpText() => @"<span style='color:#E87722;'>── Trinity C2 Commands ──</span>
<span style='color:#ccc;'> help </span><span style='color:#888;'>Show this help</span>
<span style='color:#ccc;'> clear </span><span style='color:#888;'>Clear terminal</span>
<span style='color:#ccc;'> status </span><span style='color:#888;'>TeamServer connection status</span>
<span style='color:#ccc;'> interact &lt;id&gt; </span><span style='color:#888;'>Target an agent by ID</span>
<span style='color:#ccc;'> deselect </span><span style='color:#888;'>Deselect current agent</span>
<span style='color:#ccc;'> shell &lt;cmd&gt; </span><span style='color:#888;'>Run local PowerShell command</span>
<span style='color:#ccc;'> whoami </span><span style='color:#888;'>Show current operator</span>
<span style='color:#888;'> (any C2 cmd) Routed to agent when one is selected</span>

";

    public async ValueTask DisposeAsync()
    {
        if (_hub is not null) await _hub.DisposeAsync();
    }
}