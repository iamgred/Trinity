using Client.Components.Shell;
using Client.Services;
using Microsoft.AspNetCore.Components;
using System.Text.Json;
using Trinity.Shared.DTOs.Payload;

namespace Client.Components.Pages;

public partial class Payloads
{
    [CascadingParameter] private WorkspaceState? State { get; set; }

    [SupplyParameterFromQuery(Name = "generate")]
    public int? GenerateQuery { get; set; }

    private List<PayloadEntry> _payloads = [];
    private PayloadEntry? _selected;
    private string? _loadError;
    private bool _loading;
    private bool _generateOpen;

    protected override async Task OnInitializedAsync()
    {
        State?.OpenTab("Payloads", "payloads");
        await LoadAsync();
    }

    protected override void OnParametersSet()
    {
        if (GenerateQuery == 1)
            _generateOpen = true;
    }

    private async Task LoadAsync()
    {
        _loading = true;
        try
        {
            _payloads = (await PayloadService.GetTasksAsync())
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
            _selected = _payloads.FirstOrDefault();
            _loadError = null;
        }
        catch (Exception e) when (e is HttpRequestException or JsonException or TaskCanceledException)
        {
            _loadError = ServerDown.Is(e) ? ServerDown.Message : $"Could not load payloads: {e.Message}";
        }
        finally
        {
            _loading = false;
        }
    }

    private void CloseGenerate()
    {
        _generateOpen = false;
        if (GenerateQuery == 1)
            Nav.NavigateTo("payloads", replace: true);
    }

    private async Task OnGeneratedAsync()
    {
        CloseGenerate();
        await LoadAsync();
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024):F1} MB";
    }
}