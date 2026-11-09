using Microsoft.AspNetCore.Components;
using MudBlazor;
using Client.Services;
using Trinity.Shared.DTOs.Agent;
using TrinityTask = Trinity.Shared.Models.Task;

namespace Client.Components.Pages;

public partial class Dashboard
{
    [Inject] AgentService AgentService { get; set; } = null!;
    [Inject] ListenerApiClient ListenerClient { get; set; } = null!;
    [Inject] TaskApiService TaskApiService { get; set; } = null!;
    [Inject] ISnackbar Snackbar { get; set; } = null!;

    private bool _loading = false;
    private int _onlineAgents = 0;
    private int _activeListeners = 0;
    private int _queuedTasks = 0;
    private int _completedTasks = 0;
    private List<AgentResponse> _recentAgents = [];
    private List<TrinityTask> _recentTasks = [];
    private string _lastRefresh = "—";

    protected override async Task OnInitializedAsync() => await LoadAllAsync();

    private async Task LoadAllAsync()
    {
        _loading = true;
        try { await Task.WhenAll(LoadAgentsAsync(), LoadListenersAsync(), LoadTasksAsync()); _lastRefresh = DateTime.Now.ToString("HH:mm:ss"); }
        finally { _loading = false; }
    }

    private async Task LoadAgentsAsync()
    {
        try
        {
            var agents = await AgentService.GetTasksAsync();
            _onlineAgents = agents.Count(IsOnline);
            _recentAgents = agents.OrderByDescending(a => a.lastCheckIn).Take(8).ToList();
        }
        catch (InvalidOperationException) { }
        catch { }
    }

    private async Task LoadListenersAsync()
    {
        try
        {
            var listeners = await ListenerClient.GetListenersAsync();
            _activeListeners = listeners.Count;
        }
        catch (InvalidOperationException) { }
        catch { }
    }

    private async Task LoadTasksAsync()
    {
        try
        {
            var tasks = await TaskApiService.GetAllTasksAsync();
            _queuedTasks = tasks.Count(t => t.Status.ToString().Equals("Pending", StringComparison.OrdinalIgnoreCase) || t.Status.ToString().Equals("Queued", StringComparison.OrdinalIgnoreCase));
            _completedTasks = tasks.Count(t => t.Status.ToString().Equals("Completed", StringComparison.OrdinalIgnoreCase));
            _recentTasks = tasks.OrderByDescending(t => t.CreatedAt).Take(8).ToList();
        }
        catch (InvalidOperationException) { }
        catch { }
    }

    private static bool IsOnline(AgentResponse a) { if (a.lastCheckIn == default) return false; return (DateTime.UtcNow - a.lastCheckIn).TotalSeconds < 120; }
    private static string FormatLastSeen(DateTime dt) { if (dt == default) return "Never"; var diff = DateTime.UtcNow - dt; return diff.TotalSeconds < 10 ? "Just now" : diff.TotalMinutes < 1 ? $"{(int)diff.TotalSeconds}s ago" : diff.TotalHours < 1 ? $"{(int)diff.TotalMinutes}m ago" : diff.TotalDays < 1 ? $"{(int)diff.TotalHours}h ago" : dt.ToString("dd MMM HH:mm"); }
    private static string FormatDate(DateTime dt) => dt == default ? "—" : dt.ToString("HH:mm:ss");
    private static string Shorten(string? s, int max = 42) { if (string.IsNullOrEmpty(s)) return "—"; return s.Length > max ? s[..max] + "…" : s; }
    private static string TaskStatusClass(string? status) => status?.ToLowerInvariant() switch { "completed" => "task-complete", "pending" => "task-pending", "queued" => "task-pending", "failed" => "task-failed", "running" => "task-running", _ => "task-muted" };
}