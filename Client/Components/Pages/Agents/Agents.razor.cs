using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Client.Services;
using Trinity.Shared.DTOs.Agent;

namespace Client.Components.Pages;

public partial class Agents
{
    [Inject] AgentService AgentService { get; set; } = null!;
    [Inject] ISnackbar Snackbar { get; set; } = null!;
    [Inject] NavigationManager Nav { get; set; } = null!;

    private List<AgentResponse> _agents = [];
    private AgentResponse? _selectedAgent = null;
    private bool _loading = false;
    private string _search = string.Empty;
    private string _filterStatus = string.Empty;
    private string? _latestSeen = null;

    private AgentResponse? _terminalAgent;
    private AgentResponse? _contextAgent;
    private double _menuX, _menuY;
    private bool _menuOpen;

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

    protected override async Task OnInitializedAsync() => await LoadAgentsAsync();

    private async Task LoadAgentsAsync()
    {
        _loading = true;
        try
        {
            _agents = await AgentService.GetTasksAsync();
            var latest = _agents.Where(a => a.lastCheckIn != default).OrderByDescending(a => a.lastCheckIn).FirstOrDefault();
            _latestSeen = latest is null ? null : FormatLastSeen(latest.lastCheckIn);
        }
        catch (InvalidOperationException) { _agents = []; }
        catch { _agents = []; }
        finally { _loading = false; }
    }

    private static bool IsOnline(AgentResponse a) => a.lastCheckIn != default && (DateTime.UtcNow - a.lastCheckIn).TotalSeconds < 120;
    private static string FormatLastSeen(DateTime dt) { if (dt == default) return "Never"; var diff = DateTime.UtcNow - dt; return diff.TotalSeconds < 10 ? "Just now" : diff.TotalMinutes < 1 ? $"{(int)diff.TotalSeconds}s ago" : diff.TotalHours < 1 ? $"{(int)diff.TotalMinutes}m ago" : diff.TotalDays < 1 ? $"{(int)diff.TotalHours}h ago" : dt.ToString("dd MMM HH:mm"); }
    private void OnAgentSelected(AgentResponse? agent) => _selectedAgent = agent;

    private void InteractWithAgent(AgentResponse agent)
    {
        if (IsOnline(agent)) _terminalAgent = agent;
    }

    private void CloseTerminal() => _terminalAgent = null;

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
        if (_contextAgent is not null && IsOnline(_contextAgent))
            _terminalAgent = _contextAgent;
        _menuOpen = false;
    }

    private async Task DeleteFromContext()
    {
        if (_contextAgent is not null)
            await DeleteAgentAsync(_contextAgent);
        _menuOpen = false;
    }

    private async Task DeleteAgentAsync(AgentResponse agent)
    {
        try
        {
            await AgentService.DeleteAsync(agent.agentID);
            Snackbar.Add($"Agent '{agent.username}' removed.", Severity.Warning);
            if (_selectedAgent?.agentID == agent.agentID) _selectedAgent = null;
            if (_terminalAgent?.agentID == agent.agentID) _terminalAgent = null;
            await LoadAgentsAsync();
        }
        catch (Exception ex) { Snackbar.Add($"Delete failed: {ex.Message}", Severity.Error); }
    }
}