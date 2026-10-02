using System.Globalization;

namespace Client.Components.Pages.Settings;
//All of this is mock data for UI testing, can be removed without issue
public partial class Settings
{
    private record StatVm(string Label, string Value, string Sub, string Icon);
    private record Channel(string Profile, string Transport, string Local, string Port, int Clients);
    private record Profile(string Name, string Protocol, string Local, string Port, string Status, string StatusColor);
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

    // C2 Framework settings state
    private bool _enableC2 = true;
    private bool _autoRegister = true;
    private bool _logTraffic = true;
    private string _defaultProfile = "HTTPS Beacon";
    private string _responseProfile = "Balanced";
    private string _sleepJitter = "30s (25%)";
    private string _maxClients = "500";
    private string _clientExpiry = "90 Days";
    private string _killDate = "2026-02-15";

    private const int HealthPercent = 98;

    private string? _toast;

    private readonly List<Channel> _channels = new()
    {
        new("HTTPS Beacon", "TCP", "10.0.0.5", "443", 5),
        new("WebDAV",       "TCP", "10.0.0.5", "80",  0),
        new("Custom (RAW)", "TCP", "10.0.0.5", "9001", 0),
    };

    private readonly List<Profile> _profiles = new()
    {
        new("HTTPS Beacon", "HTTPS",  "10.0.0.5", "443",  "Active",   "green"),
        new("WebDAV",       "HTTP",   "10.0.0.5", "80",   "Standby",  "yellow"),
        new("Custom (RAW)", "Custom", "10.0.0.5", "9001", "Inactive", "red"),
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
        new("C2 Profiles", "18", "Communication Profiles",
            "<path d='M5 12.55a11 11 0 0 1 14.08 0'/><path d='M1.42 9a16 16 0 0 1 21.16 0'/><path d='M8.53 16.11a6 6 0 0 1 6.95 0'/><line x1='12' y1='20' x2='12.01' y2='20'/>"),
        new("AI Agent Policies", "8", "AI Behaviour Policies",
            "<circle cx='12' cy='8' r='4'/><path d='M4 21v-1a6 6 0 0 1 6-6h4a6 6 0 0 1 6 6v1'/>"),
        new("OPSEC Controls", "12", "Operational Security Rules",
            "<path d='M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z'/>"),
        new("Evasion Techniques", "23", "Evasion Configurations",
            "<path d='M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24'/><line x1='1' y1='1' x2='23' y2='23'/>"),
        new("System Health", "98%", "Overall System Status",
            "<rect x='3' y='4' width='18' height='6' rx='1'/><rect x='3' y='14' width='18' height='6' rx='1'/><path d='M7 7h.01M7 17h.01'/>"),
    };

    private static string Css(string color) => color switch
    {
        "green" => "var(--status-green)",
        "yellow" => "var(--status-yellow)",
        "red" => "var(--status-red)",
        "purple" => "var(--status-purple)",
        "blue" => "var(--status-blue)",
        _ => "var(--text-muted)",
    };

    private string HealthGauge
    {
        get
        {
            var fill = (HealthPercent / 100.0 * 100).ToString("0.##", CultureInfo.InvariantCulture);
            return $"var(--status-green) 0% {fill}%, var(--bg-input) {fill}% 100%";
        }
    }

    private ActionVm[] Actions => new[]
    {
        new ActionVm("Create New C2 Profile",
            "<path d='M5 12.55a11 11 0 0 1 14.08 0'/><path d='M1.42 9a16 16 0 0 1 21.16 0'/><path d='M8.53 16.11a6 6 0 0 1 6.95 0'/><line x1='12' y1='20' x2='12.01' y2='20'/>",
            () => ShowToast("Opened new C2 profile wizard (mock)")),
        new ActionVm("Create AI Policy",
            "<circle cx='12' cy='8' r='4'/><path d='M4 21v-1a6 6 0 0 1 6-6h4a6 6 0 0 1 6 6v1'/>",
            () => ShowToast("Opened new AI policy editor (mock)")),
        new ActionVm("Export Config",
            "<path d='M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4'/><polyline points='7 10 12 15 17 10'/><line x1='12' y1='15' x2='12' y2='3'/>",
            () => ShowToast("Exported configuration to file (mock)")),
        new ActionVm("Reset C2 Settings",
            "<circle cx='12' cy='12' r='3'/><path d='M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z'/>",
            () => ShowToast("C2 settings reset to defaults (mock)")),
        new ActionVm("Deploy Evasion Config",
            "<path d='M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24'/><line x1='1' y1='1' x2='23' y2='23'/>",
            () => ShowToast("Evasion configuration deployed (mock)")),
        new ActionVm("Test C2 Connection",
            "<path d='M5 12.55a11 11 0 0 1 14.08 0'/><path d='M1.42 9a16 16 0 0 1 21.16 0'/><path d='M8.53 16.11a6 6 0 0 1 6.95 0'/><line x1='12' y1='20' x2='12.01' y2='20'/>",
            () => ShowToast("C2 connection test passed (mock)")),
        new ActionVm("Import Config",
            "<path d='M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4'/><polyline points='17 8 12 3 7 8'/><line x1='12' y1='3' x2='12' y2='15'/>",
            () => ShowToast("Imported configuration from file (mock)")),
        new ActionVm("OPSEC Checklist",
            "<path d='M9 11l3 3L22 4'/><path d='M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11'/>",
            () => ShowToast("Opened OPSEC checklist (mock)")),
    };

    private void SelectCategory(string c)
    {
        _category = c;
        if (c != "C2 Framework")
            ShowToast($"\"{c}\" settings coming soon (mock)");
    }

    private void SaveChanges() => ShowToast("C2 Framework settings saved (mock)");

    private async void ShowToast(string msg)
    {
        _toast = msg;
        StateHasChanged();
        await Task.Delay(2600);
        if (_toast == msg) { _toast = null; StateHasChanged(); }
    }
}
