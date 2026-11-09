using Microsoft.AspNetCore.Components;
using MudBlazor;
using Client.Services;
using Agent = Trinity.Shared.Models.Agent;

namespace Client.Components.Pages;

public partial class Pivot
{
    [Inject] AgentService AgentService { get; set; } = null!;
    [Inject] ISnackbar Snackbar { get; set; } = null!;

    private List<Agent> _agents = [];
    private Agent? _selectedAgent;
    private bool _loading;
    private string _view = "graph";
    private string _svgViewBox = "0 0 800 560";
    private double _zoom = 1.0;

    protected override async Task OnInitializedAsync() => await LoadAgentsAsync();

    private async Task LoadAgentsAsync()
    {
        _loading = true;
        try { _agents = await AgentService.GetTasksAsync(); }
        catch (InvalidOperationException) { _agents = []; }
        catch { _agents = []; }
        finally { _loading = false; }
    }

    private static bool IsOnline(Agent a)
    {
        if (a.LastCheckIn == default) return false;
        return (DateTime.UtcNow - a.LastCheckIn).TotalSeconds < 120;
    }

    private void SelectAgent(Agent agent)
    {
        _selectedAgent = _selectedAgent?.ID == agent.ID ? null : agent;
    }

    // Returns integer coordinates so the markup can do arithmetic: x2="@(400 - ax)"
    private static (int ax, int ay) AgentPosition(int index, int total)
    {
        if (total == 0) return (400, 295);
        double angle = 2 * Math.PI * index / total - Math.PI / 2;
        return (
            (int)(400 + 220 * Math.Cos(angle)),
            (int)(295 + 180 * Math.Sin(angle))
        );
    }

    private static string Shorten(string? value, int max = 12)
    {
        if (string.IsNullOrWhiteSpace(value)) return "?";
        return value.Length <= max ? value : value[..max] + "…";
    }

    private static string FormatLastSeen(DateTime dt)
    {
        if (dt == default) return "Never";
        var diff = DateTime.UtcNow - dt;
        if (diff.TotalSeconds < 60) return $"{(int)diff.TotalSeconds}s ago";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
        return $"{(int)diff.TotalDays}d ago";
    }

    private void ZoomIn() { _zoom = Math.Min(_zoom + 0.2, 3.0); UpdateViewBox(); }
    private void ZoomOut() { _zoom = Math.Max(_zoom - 0.2, 0.3); UpdateViewBox(); }
    private void ResetZoom() { _zoom = 1.0; _svgViewBox = "0 0 800 560"; }

    private void UpdateViewBox()
    {
        double w = 800 / _zoom, h = 560 / _zoom;
        double x = (800 - w) / 2, y = (560 - h) / 2;
        _svgViewBox = $"{x:F0} {y:F0} {w:F0} {h:F0}";
    }
}