using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Client.Services;
using Trinity.Shared.DTOs.Agent;

namespace Client.Components.Pages;

public partial class Pivot
{
    [Inject] AgentService AgentService { get; set; } = null!;
    [Inject] IJSRuntime JS { get; set; } = null!;

    private List<AgentResponse> _agents = [];
    private AgentResponse? _selectedAgent;
    private bool _loading;
    private string _view = "graph";
    private string _svgViewBox = "0 0 800 560";
    private double _zoom = 1.0;
    private int _graphH = 480;

    protected override async Task OnInitializedAsync() => await LoadAgentsAsync();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        await JS.InvokeVoidAsync("trinityResize.initGraph", DotNetObjectReference.Create(this), "graph-viewport", 240);
    }

    [JSInvokable]
    public void SetGraphHeight(int px) { _graphH = px; InvokeAsync(StateHasChanged); }

    private async Task LoadAgentsAsync()
    {
        _loading = true;
        try { _agents = await AgentService.GetTasksAsync(); }
        catch (InvalidOperationException) { _agents = []; }
        catch { _agents = []; }
        finally { _loading = false; }
    }

    private static bool IsOnline(AgentResponse a)
    {
        if (a.lastCheckIn == default) return false;
        return (DateTime.UtcNow - a.lastCheckIn).TotalSeconds < 120;
    }

    private void SelectAgent(AgentResponse agent)
    {
        _selectedAgent = _selectedAgent?.agentID == agent.agentID ? null : agent;
    }

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
