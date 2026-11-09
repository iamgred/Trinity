using Microsoft.AspNetCore.Components;
using Client.Services;

namespace Client.Components.Shell;

public partial class PayloadsPanel : ComponentBase
{
    [Inject] PayloadService PayloadService { get; set; } = null!;

    private List<PayloadEntry> _payloads = [];
    private PayloadEntry? _selected;
    private bool _loading;
    private bool _generateOpen;

    protected override async Task OnInitializedAsync() => await RefreshAsync();

    private async Task RefreshAsync()
    {
        _loading = true;
        _payloads = (await PayloadService.GetTasksAsync()).OrderByDescending(p => p.CreatedAt).ToList();
        _selected = null;
        _loading = false;
        StateHasChanged();
    }

    private void OpenGenerate() => _generateOpen = true;
    private void CloseGenerate() => _generateOpen = false;

    private async Task OnGeneratedAsync()
    {
        _generateOpen = false;
        await RefreshAsync();
    }

    private async Task HandleDelete()
    {
        if (_selected is null) return;
        await PayloadService.DeleteAsync(_selected.Id);
        await RefreshAsync();
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024):F1} MB";
    }
}
