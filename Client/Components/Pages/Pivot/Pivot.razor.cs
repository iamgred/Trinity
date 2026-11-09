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
    private string _svgViewBox = "0 0 850 500";
    private double _zoom = 1.0;
    private int _graphH = 480;

    private int SvgW => 850;
    private int SvgH => Math.Max(500, _agents.Count * 105 + 100);

    protected override async Task OnInitializedAsync() => await LoadAgentsAsync();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        var r = DotNetObjectReference.Create(this);
        await JS.InvokeVoidAsync("trinityResize.initGraph", r, "graph-viewport", 240);
        await JS.InvokeVoidAsync("trinityResize.initGraphPan", r, "graph-viewport");
    }

    [JSInvokable] public void SetGraphHeight(int px) { _graphH = px; InvokeAsync(StateHasChanged); }
    [JSInvokable] public void SyncViewBox(string vb) { _svgViewBox = vb; }

    private async Task LoadAgentsAsync()
    {
        _loading = true;
        try { _agents = await AgentService.GetTasksAsync(); }
        catch (InvalidOperationException) { _agents = []; }
        catch { _agents = []; }
        finally
        {
            _loading = false;
            _svgViewBox = $"0 0 {SvgW} {SvgH}";
        }
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

    private void ZoomIn()  { _zoom = Math.Min(_zoom + 0.2, 3.0); UpdateViewBox(); }
    private void ZoomOut() { _zoom = Math.Max(_zoom - 0.2, 0.3); UpdateViewBox(); }
    private void ResetZoom() { _zoom = 1.0; _svgViewBox = $"0 0 {SvgW} {SvgH}"; }

    private void UpdateViewBox()
    {
        double w = SvgW / _zoom, h = SvgH / _zoom;
        double x = (SvgW - w) / 2, y = (SvgH - h) / 2;
        _svgViewBox = $"{x:F0} {y:F0} {w:F0} {h:F0}";
    }
}
