using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Client.Services;

namespace Client.Components.Shell;

public partial class LootPanel : ComponentBase
{
    private sealed class LootItem
    {
        public int?   AgentId     { get; init; }
        public required string Type        { get; init; }
        public required string Name        { get; init; }
        public required string PathOrValue { get; init; }
        public long?  Size        { get; init; }
        public long   Received    { get; set; }
        public required string Status      { get; set; }
        public required DateTime Collected { get; init; }
        public string? Note       { get; set; }
    }

    [Inject] private IJSRuntime    JS      { get; set; } = default!;
    [Inject] private FilesApiClient Files  { get; set; } = default!;

    private readonly List<LootItem> _items = [];

    private string?   _loadError;
    private bool      _isLoading;
    private LootItem? _selected;
    private string?   _copyStatus;
    private LootItem? _noteTarget;
    private string    _noteDraft  = "";
    private string?   _noteError;

    private IEnumerable<LootItem> Filtered => _items;

    private bool CanSave   => _selected is { Type: "Download", Status: "complete" };
    private bool CanCancel => _selected is { Type: "Download", Status: "chunked" };

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _isLoading = true;
        _loadError = null;
        try
        {
            var downloads = await Files.GetLootFilesAsync();
            _items.RemoveAll(i => i.Type == "Download");
            _items.InsertRange(0, downloads.Select(d => new LootItem
            {
                Type = "Download", Name = d.Name, PathOrValue = d.Name,
                Size = d.Size, Received = d.Size, Status = "complete", Collected = d.LastModified,
            }));
        }
        catch (Exception e)
        {
            _loadError = $"Could not load downloads: {e.Message}";
        }
        finally { _isLoading = false; }
    }

    private async Task SaveToDiskAsync()
    {
        if (_selected is null) return;
        var bytes = await Files.DownloadFileBytesAsync(_selected.Name);
        if (bytes is not null)
            await JS.InvokeVoidAsync("downloadBlob", _selected.Name, bytes);
    }


    private void CancelDownload()
    {
        if (_selected is not null) { _selected.Status = "cancelled"; _selected.Received = 0; }
    }

    private async Task CopyValueAsync()
    {
        if (_selected is null) return;
        var value = _selected.Type == "Download" ? _selected.Name : _selected.PathOrValue;
        try { await JS.InvokeVoidAsync("navigator.clipboard.writeText", value); _copyStatus = "Copied"; }
        catch (JSException) { _copyStatus = "Copy blocked"; }
        StateHasChanged();
        await Task.Delay(1500);
        _copyStatus = null;
    }

    private void OpenNote(LootItem item) { _noteTarget = item; _noteDraft = item.Note ?? ""; _noteError = null; }

    private void SaveNote()
    {
        if (_noteTarget is null) return;
        if (string.IsNullOrWhiteSpace(_noteDraft))
        {
            _noteError = "Note cannot be empty.";
            return;
        }
        _noteTarget.Note = _noteDraft.Trim();
        _noteTarget = null;
    }

    private void Remove()
    {
        if (_selected is not null) { _items.Remove(_selected); _selected = null; }
    }

    private static string FileExtType(string name)
    {
        var ext = Path.GetExtension(name);
        return ext.Length > 1 ? ext[1..].ToUpperInvariant() : "BIN";
    }

    private static string TypeIcon(string name) => Path.GetExtension(name).ToLowerInvariant() switch
    {
        ".txt" or ".log" or ".md" => "📝",
        ".ps1" or ".bat" or ".sh" => "⚡",
        ".exe" or ".dll"          => "⚙",
        _                         => "⬇",
    };

    private static string StatusTone(string status) => status switch
    {
        "chunked"   => "yellow",
        "complete"  => "green",
        "cancelled" => "red",
        _           => "grey",
    };

    private static string FormatSize(long bytes) => bytes >= 1_048_576
        ? $"{bytes / 1_048_576.0:0.0} MB"
        : $"{Math.Max(1, (int)Math.Round(bytes / 1024.0))} KB";

    private static int ProgressPct(LootItem item) =>
        item.Size is > 0 ? (int)Math.Round(100.0 * item.Received / item.Size.Value) : 0;

    private static string ReceivedLabel(LootItem item) => item.Status switch
    {
        "cancelled" => "cancelled",
        "complete"  => item.Size is not null ? FormatSize(item.Received) : "–",
        _           => $"{FormatSize(item.Received)} / {FormatSize(item.Size ?? 0)}",
    };
}
