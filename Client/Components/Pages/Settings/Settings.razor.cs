using System.Net.Http;
using Client.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Components.Pages.Settings;
public partial class Settings
{
    private record StatVm(string Label, string Value, string Sub, string Icon);
    private record Channel(string Profile, string Transport, string Local, string Port, int Clients);
    private record Profile(int Id, string Name, string Protocol, string Local, string Port, string Status, string StatusColor);
    private record Policy(string Name, string Desc, string Badge, string BadgeColor);
    private record ActionVm(string Label, string Icon, Action OnClick);

    private readonly string[] _categories =
    {
        "General", "C2 Framework", "AI Agents", "Communication", "Implant & Payloads",
        "Tasking & Automation", "Evasion and OPSEC", "Data Handling", "Integrations",
        "Notifications", "Backup & Restore", "Audit & Compliance", "Advanced",
        "System Maintenance", "API Settings", "About",
    };
    private string _category = "C2 Framework";

    // settings form values
    private bool _enableC2 = true;
    private bool _autoRegister = true;
    private bool _logTraffic = true;
    private string _defaultProfile = "HTTPS Beacon";
    private string _responseProfile = "Balanced";
    private string _sleepJitter = "30s (25%)";
    private string _maxClients = "500";
    private string _clientExpiry = "90 Days";
    private string _killDate = "2026-02-15";

    // is the server up? (null = not checked yet)
    private bool? _serverOnline;
    private int HealthPercent => _serverOnline == true ? 100 : 0;
    private string HealthLabel => _serverOnline switch
    {
        true => "Operational",
        false => "Unreachable",
        null => "Checking…",
    };

    private readonly DialogOptions _dialogOptions = new() { MaxWidth = MaxWidth.Small, FullWidth = true, CloseOnEscapeKey = true };

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private ListenerApiClient ListenerApiClient { get; set; } = default!;

    [Inject]
    private ServerApiClient ServerApiClient { get; set; } = default!;

    private bool _liveListeners;
    private string? _serverIpV4;
    private string? _serverIpV6;

    private bool _showCreateProfile;
    private string _profileName = "";
    private string _profileProtocol = "HTTP";
    private string _profileHost = "";
    private string _profilePort = "";
    private string _profilePipe = "";

    protected override async Task OnInitializedAsync() => await LoadListenersAsync();

    private async Task LoadListenersAsync()
    {
        try
        {
            var listeners = await ListenerApiClient.GetListenersAsync();
            if (listeners.Count > 0)
            {
                _channels.Clear();
                _profiles.Clear();
                foreach (var listener in listeners)
                {
                    // real listeners from the server (name and type only)
                    _channels.Add(new Channel(listener.name, listener.protocol, "Not provided", "Not provided", 0));
                    _profiles.Add(new Profile(listener.id, listener.name, listener.protocol, "Not provided", "Not provided", "Unknown", "gray"));
                }
                _liveListeners = true;
            }
        }
        catch
        {
            // cant reach the server, keep the example tables
            _liveListeners = false;
        }

        try
        {
            var ip = await ServerApiClient.GetServerIpAsync();
            _serverIpV4 = ip?.ipV4;
            _serverIpV6 = ip?.ipV6;
        }
        catch
        {
            _serverIpV4 = null;
            _serverIpV6 = null;
        }

        try
        {
            _serverOnline = await ServerApiClient.GetServerStatusAsync();
        }
        catch
        {
            _serverOnline = false;
        }
    }

    private async Task RestartProfile(int id)
    {
        if (id <= 0) return;
        try
        {
            var ok = await ListenerApiClient.RestartListenerAsync(id);
            ShowToast(ok ? "Listener restarted." : "TeamServer could not restart the listener.");
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException) { ShowToast(ServerDown.Is(ex) ? ServerDown.Message : ex.Message); }
    }

    private async Task DeleteProfile(int id)
    {
        if (id <= 0) return;
        try
        {
            var ok = await ListenerApiClient.DeleteListenerAsync(id);
            if (ok)
            {
                ShowToast("C2 profile deleted.");
                await LoadListenersAsync();
            }
            else
            {
                ShowToast("TeamServer could not delete the profile.");
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException) { ShowToast(ServerDown.Is(ex) ? ServerDown.Message : ex.Message); }
    }

    private void OpenCreateProfile()
    {
        _profileName = "";
        _profileProtocol = "HTTP";
        _profileHost = "";
        _profilePort = "";
        _profilePipe = "";
        _showCreateProfile = true;
    }

    private void CloseCreateProfile() => _showCreateProfile = false;

    private async Task CreateProfile()
    {
        if (string.IsNullOrWhiteSpace(_profileName)) { ShowToast("Profile name is required."); return; }

        try
        {
            bool created;
            switch (_profileProtocol)
            {
                case "HTTP":
                    if (!int.TryParse(_profilePort, out var httpPort) || httpPort <= 0 || httpPort > 65535) { ShowToast("Enter a valid port (1-65535)."); return; }
                    var host = string.IsNullOrWhiteSpace(_profileHost) ? "0.0.0.0" : _profileHost.Trim();
                    created = await ListenerApiClient.CreateHttpListenerAsync(_profileName.Trim(), host, httpPort);
                    break;
                case "TCP":
                    if (!int.TryParse(_profilePort, out var tcpPort) || tcpPort <= 0 || tcpPort > 65535) { ShowToast("Enter a valid port (1-65535)."); return; }
                    created = await ListenerApiClient.CreateTcpListenerAsync(_profileName.Trim(), tcpPort);
                    break;
                case "SMB":
                    if (string.IsNullOrWhiteSpace(_profilePipe)) { ShowToast("Enter a pipe name."); return; }
                    created = await ListenerApiClient.CreateSmbListenerAsync(_profileName.Trim(), _profilePipe.Trim());
                    break;
                default:
                    ShowToast("Unsupported protocol."); return;
            }

            if (created)
            {
                ShowToast($"C2 profile \"{_profileName.Trim()}\" created.");
                _showCreateProfile = false;
                await LoadListenersAsync();
            }
            else
            {
                ShowToast("TeamServer rejected the profile. Check the port and try again.");
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException)
        {
            ShowToast(ServerDown.Is(ex) ? ServerDown.Message : ex.Message);
        }
    }

    private readonly List<Channel> _channels = new()
    {
        new("HTTPS Beacon", "TCP", "10.0.0.5", "443", 5),
        new("WebDAV",       "TCP", "10.0.0.5", "80",  0),
        new("Custom (RAW)", "TCP", "10.0.0.5", "9001", 0),
    };

    private readonly List<Profile> _profiles = new()
    {
        new(0, "HTTPS Beacon", "HTTPS",  "10.0.0.5", "443",  "Active",   "green"),
        new(0, "WebDAV",       "HTTP",   "10.0.0.5", "80",   "Standby",  "yellow"),
        new(0, "Custom (RAW)", "Custom", "10.0.0.5", "9001", "Inactive", "red"),
    };

    private readonly List<Policy> _policies = new()
    {
        new("Target Selection Policy",  "Define how AI agents select and prioritize targets", "Aggressive", "red"),
        new("Task Execution Policy",    "Define how AI agents execute assigned tasks",        "Balanced",   "green"),
        new("Reporting & Exfil Policy", "Define how AI agents collect and exfiltrate data",   "Stealth",    "purple"),
        new("Adaption Policy",          "Define how AI agents adapt to changes",              "Enabled",    "green"),
    };

    private List<StatVm> BuildStats() => new()
    {
        new("C2 Profiles", _liveListeners ? _profiles.Count.ToString() : "18", "Communication Profiles", Icons.Material.Outlined.Wifi),
        new("AI Agent Policies", "8", "AI Behaviour Policies", Icons.Material.Outlined.SmartToy),
        new("OPSEC Controls", "12", "Operational Security Rules", Icons.Material.Outlined.Shield),
        new("Evasion Techniques", "23", "Evasion Configurations", Icons.Material.Outlined.VisibilityOff),
        new("System Health", $"{HealthPercent}%", HealthLabel, Icons.Material.Outlined.Dns),
    };

    private ActionVm[] Actions => new[]
    {
        new ActionVm("Create New C2 Profile", Icons.Material.Filled.Wifi, OpenCreateProfile),
        new ActionVm("Create AI Policy", Icons.Material.Filled.SmartToy, () => ShowToast("Opened new AI policy editor (mock)")),
        new ActionVm("Export Config", Icons.Material.Filled.Download, () => ShowToast("Exported configuration to file (mock)")),
        new ActionVm("Reset C2 Settings", Icons.Material.Filled.RestartAlt, () => ShowToast("C2 settings reset to defaults (mock)")),
        new ActionVm("Deploy Evasion Config", Icons.Material.Filled.VisibilityOff, () => ShowToast("Evasion configuration deployed (mock)")),
        new ActionVm("Test C2 Connection", Icons.Material.Filled.NetworkCheck, () => ShowToast("C2 connection test passed (mock)")),
        new ActionVm("Import Config", Icons.Material.Filled.Upload, () => ShowToast("Imported configuration from file (mock)")),
        new ActionVm("OPSEC Checklist", Icons.Material.Filled.Checklist, () => ShowToast("Opened OPSEC checklist (mock)")),
    };

    private void SelectCategory(string c)
    {
        _category = c;
        if (c != "C2 Framework")
            ShowToast($"\"{c}\" settings coming soon (mock)");
    }

    private void SaveChanges() => ShowToast("C2 Framework settings saved (mock)");

    private void ShowToast(string msg) => Snackbar.Add(msg, Severity.Info);
}
