using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using Client.Components.Shell;
using Trinity.Shared.DTOs.Notification;
using Trinity.Shared.Enums.Tasks;
using Trinity.Shared.Interfaces;

namespace Client.Components.Layout;

public partial class WorkspaceLayout : LayoutComponentBase, IAsyncDisposable
{
    [Inject] IConfiguration Config { get; set; } = null!;
    [Inject] IJSRuntime JS { get; set; } = null!;
    [Inject] ICommandInterpreter CommandInterpreter { get; set; } = null!;

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

    private readonly WorkspaceState _workspaceState = new();
    private int _bottomH = 220;
    private int _bottomTab = 0;
    private void SetBottomTab(int idx) => _bottomTab = idx;
    private bool _serverOnline = false;
    private string _termHtml =
        "<span style='color:#E87722;font-weight:600;'>Trinity C2 v2.0 — Terminal Ready</span>\n" +
        "Type <span style='color:#4EC9B0;'>help</span> for commands or <span style='color:#4EC9B0;'>interact &lt;agentId&gt;</span> to target an agent.\n\n";
    private string _termInput = string.Empty;
    private int? _currentAgentId;
    private HubConnection? _hub;
    private readonly List<EventEntry> _events = [];
    private record EventEntry(string Stamp, string Level, string Message);

    /* prompt reflects active agent context */
    private string PromptHtml => _currentAgentId.HasValue
        ? $"<span style='color:#E87722;font-weight:600;'>trinity@agent-{_currentAgentId}</span>"
        : "<span style='color:#E87722;font-weight:600;'>trinity@c2</span>";

    /* plain-text version for the input row span */
    public string PromptText => _currentAgentId.HasValue
        ? $"trinity@agent-{_currentAgentId} ~$"
        : "trinity@c2 ~$";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        await JS.InvokeVoidAsync("trinityResize.init", DotNetObjectReference.Create(this), "bottom-panel", 140, 600);
        await ConnectHubAsync();
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

            /* real task output from agents comes back on TaskNotification */
            _hub.On<TaskNotification>("TaskNotification", async notification =>
            {
                var color = notification.Status switch
                {
                    Status.SUCCESS or Status.COMPLETED or Status.NO_MORE_TO_FOLLOW => "#4EC9B0",
                    Status.ERROR => "#F44747",
                    Status.RUNNING or Status.MORE_TO_FOLLOW or Status.DISPATCHED => "#DCDCAA",
                    _ => "rgba(255,255,255,0.87)"
                };
                var label = notification.Status.ToString();
                var msg = System.Net.WebUtility.HtmlEncode(notification.message);
                _termHtml += $"<span style='color:{color};'>[{label}] Task #{notification.TaskID}: {msg}</span>\n\n";
                _events.Add(new EventEntry(
                    notification.Timestamp.ToString("HH:mm:ss"),
                    notification.Status == Status.ERROR ? "ERROR" : "INFO",
                    $"Task #{notification.TaskID}: {notification.message}"));
                await InvokeAsync(StateHasChanged);
                await InvokeAsync(ScrollTerminal);
            });

            _hub.Reconnecting += _ => { _serverOnline = false; return InvokeAsync(StateHasChanged); };
            _hub.Reconnected += _ => { _serverOnline = true; return InvokeAsync(StateHasChanged); };

            await _hub.StartAsync();
            _serverOnline = true;
            _termHtml += "<span style='color:#4EC9B0;'>[+] Connected to TeamServer</span>\n\n";
        }
        catch
        {
            _serverOnline = false;
        }
        await InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public void SetPanelHeight(int px) { _bottomH = px; InvokeAsync(StateHasChanged); }

    private static string SevClass(string level) => level switch
    {
        "ERROR" or "CRIT" => "sev-error",
        "WARN" => "sev-warn",
        "SUCCESS" => "sev-ok",
        _ => "sev-info"
    };

    private async Task HandleTermKey(KeyboardEventArgs e)
    {
        if (e.Key != "Enter") return;

        var cmd = _termInput.Trim();
        _termInput = string.Empty;

        if (string.IsNullOrEmpty(cmd))
        {
            _termHtml += $"{PromptHtml} ~$\n";
            StateHasChanged();
            return;
        }

        var verb = cmd.Split(' ', 2)[0].ToLowerInvariant();

        if (verb is "cls" or "clear" or "exit")
        {
            _termHtml =
                "<span style='color:#E87722;font-weight:600;'>Trinity C2 v2.0 — Terminal Ready</span>\n" +
                "Type <span style='color:#4EC9B0;'>help</span> for commands or <span style='color:#4EC9B0;'>interact &lt;agentId&gt;</span> to target an agent.\n\n";
            StateHasChanged();
            return;
        }

        _termHtml += $"{PromptHtml} ~$ {System.Net.WebUtility.HtmlEncode(cmd)}\n";

        await ProcessTerminalCommandAsync(cmd, verb);

        StateHasChanged();
        await ScrollTerminal();
    }

    private async Task ProcessTerminalCommandAsync(string cmd, string verb)
    {
        switch (verb)
        {
            case "help":
                _termHtml += HelpText();
                return;

            case "interact":
                await HandleInteractAsync(cmd);
                return;

            case "agents":
                HandleAgentsCommand();
                return;

            case "deselect":
                if (_currentAgentId.HasValue)
                {
                    if (_hub?.State == HubConnectionState.Connected)
                        await _hub.InvokeAsync("UnsubscribeFromAgent", _currentAgentId.Value.ToString());
                    _termHtml += $"<span style='color:#DCDCAA;'>Deselected agent {_currentAgentId}. No agent is now targeted.</span>\n\n";
                    _currentAgentId = null;
                }
                else
                {
                    _termHtml += "<span style='color:#DCDCAA;'>No agent is currently selected.</span>\n\n";
                }
                return;
        }

        /* all real commands require an active agent target */
        if (!_currentAgentId.HasValue)
        {
            _termHtml += "<span style='color:#F44747;'>No agent selected. Run 'interact &lt;agentId&gt;' first.</span>\n\n";
            return;
        }

        try
        {
            var result = await CommandInterpreter.Interpret(_currentAgentId.Value, cmd);
            if (result.IsSuccess)
            {
                _termHtml +=
                    $"<span style='color:#4EC9B0;'>[Queued] {System.Net.WebUtility.HtmlEncode(result.Response ?? "Command queued.")}</span>\n" +
                    "<span style='color:rgba(255,255,255,0.4);'>Waiting for agent check-in...</span>\n\n";
            }
            else
            {
                _termHtml += $"<span style='color:#F44747;'>[Error] {System.Net.WebUtility.HtmlEncode(result.Error.Description)}</span>\n\n";
            }
        }
        catch (Exception ex)
        {
            _termHtml += $"<span style='color:#F44747;'>[Error] {System.Net.WebUtility.HtmlEncode(ex.Message)}</span>\n\n";
        }
    }

    private async Task HandleInteractAsync(string cmd)
    {
        var parts = cmd.Split(' ', 2);
        if (parts.Length < 2 || !int.TryParse(parts[1].Trim(), out var agentId))
        {
            _termHtml += "<span style='color:#F44747;'>Usage: interact &lt;agentId&gt;</span>\n\n";
            return;
        }

        /* unsubscribe from previous agent group */
        if (_currentAgentId.HasValue && _hub?.State == HubConnectionState.Connected)
            await _hub.InvokeAsync("UnsubscribeFromAgent", _currentAgentId.Value.ToString());

        _currentAgentId = agentId;

        /* subscribe to this agent's SignalR notification group */
        if (_hub?.State == HubConnectionState.Connected)
            await _hub.InvokeAsync("SubscribeToAgent", agentId.ToString());

        _termHtml += $"<span style='color:#4EC9B0;'>[+] Now interacting with agent {agentId}. All commands target this agent.</span>\n\n";
    }

    private void HandleAgentsCommand()
    {
        var agents = _workspaceState.Agents;
        if (!agents.Any())
        {
            _termHtml += "<span style='color:#DCDCAA;'>No agents currently connected.</span>\n\n";
            return;
        }
        _termHtml += "<span style='color:#4EC9B0;'>Connected agents:</span>\n";
        foreach (var a in agents)
        {
            var ago = WorkspaceState.FormatAgo(WorkspaceState.SinceLastCheckIn(a));
            var stale = WorkspaceState.IsStale(a) ? " <span style='color:#F44747;'>[STALE]</span>" : "";
            _termHtml +=
                $"  ID: <span style='color:#E87722;'>{a.agentID}</span>" +
                $"  user: {System.Net.WebUtility.HtmlEncode(a.username)}" +
                $"  host: {System.Net.WebUtility.HtmlEncode(a.externalIP)}" +
                $"  pid: {a.PID}" +
                $"  last: {ago}{stale}\n";
        }
        _termHtml += "\n";
    }

    private static string HelpText() =>
        "<span style='color:#E87722;font-weight:600;'>Trinity C2 v2.0</span>\n\n" +
        "<span style='color:#4EC9B0;'>Agent selection:</span>\n" +
        "  interact &lt;id&gt;              Target an agent for all commands\n" +
        "  deselect                    Release the current agent target\n" +
        "  agents                      List connected agents and their IDs\n\n" +
        "<span style='color:#4EC9B0;'>Agent commands (requires interact first):</span>\n" +
        "  shell &lt;cmd&gt;                 Run a cmd.exe command on the agent\n" +
        "  powershell &lt;cmdlet&gt; [args]  Run a PowerShell commandlet on the agent\n" +
        "  getuid                      Get the agent's current user identity\n" +
        "  download &lt;remotePath&gt;       Download a file from the agent\n" +
        "  upload &lt;localPath&gt;          Upload a file to the agent\n" +
        "  run &lt;cmd&gt;                   Spawn a process on the agent (no window)\n" +
        "  runas &lt;user&gt; &lt;pass&gt; &lt;cmd&gt;  Run a process as another user\n" +
        "  runu &lt;cmd&gt;                  Run as current user (non-elevated)\n" +
        "  setsleep &lt;secs&gt; [jitter]    Set the agent's sleep interval\n" +
        "  spawnto &lt;path&gt;              Set the agent's spawn-to process\n" +
        "  escalate &lt;technique&gt;        Trigger a privilege escalation\n" +
        "  execute-assembly &lt;path&gt;     Execute a .NET assembly in-process\n" +
        "  cancel                      Cancel the current file download\n" +
        "  kill                        Terminate the agent process\n\n" +
        "<span style='color:#4EC9B0;'>Terminal:</span>\n" +
        "  help                        Show this help text\n" +
        "  cls / clear                 Clear the terminal\n\n";

    private async Task ScrollTerminal()
    {
        await JS.InvokeVoidAsync("eval", "var el=document.querySelector('.term-output');if(el)el.scrollTop=el.scrollHeight;");
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub is not null) await _hub.DisposeAsync();
    }
}