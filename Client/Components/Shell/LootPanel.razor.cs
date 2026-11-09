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
    }

    [Inject] private IJSRuntime    JS      { get; set; } = default!;
    [Inject] private FilesApiClient Files  { get; set; } = default!;

    private readonly List<LootItem> _items = [];

    private string?   _loadError;
    private bool      _isLoading;
    private LootItem? _selected;
    private string    _agentFilter = "";
    private string    _typeFilter  = "";
    private string?   _copyStatus;
    private bool      _noteOpen;
    private string    _noteAgent = "";
    private string    _noteName  = "";
    private string    _noteText  = "";
    private string?   _noteError;

    private IEnumerable<string> _agentIds => _items
        .Where(i => i.AgentId is not null)
        .Select(i => i.AgentId!.Value.ToString())
        .Distinct().Order();

    private IEnumerable<LootItem> Filtered => _items.Where(i =>
        (_agentFilter.Length == 0 || i.AgentId?.ToString() == _agentFilter)
        && (_typeFilter.Length == 0 || i.Type == _typeFilter));

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
                Type = "Download", Name = d.Name, PathOrValue = d.Path,
                Received = 0, Status = "complete", Collected = DateTime.UtcNow,
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
        var url = await Files.GetDownloadUrlAsync(_selected.Name);
        if (url is not null)
            await JS.InvokeVoidAsync("open", url, "_blank");
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

    private void OpenNote() { _noteName = ""; _noteText = ""; _noteError = null; _noteOpen = true; }

    private void SaveNote()
    {
        if (!int.TryParse(_noteAgent, out var agentId) || string.IsNullOrWhiteSpace(_noteName) || string.IsNullOrWhiteSpace(_noteText))
        {
            _noteError = "Pick an agent and fill in a title and the note.";
            return;
        }
        var note = new LootItem { AgentId = agentId, Type = "Note", Name = _noteName.Trim(), PathOrValue = _noteText.Trim(), Status = "saved", Collected = DateTime.Now };
        _items.Insert(0, note);
        _selected = note;
        _noteOpen = false;
    }

    private void Remove()
    {
        if (_selected is not null) { _items.Remove(_selected); _selected = null; }
    }

    private static string TypeIcon(string type) => type switch
    {
        "Credential" => "🔑",
        "Note"       => "📝",
        _            => "⬇",
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
