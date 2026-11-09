using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Client.Services;

namespace Client.Components.Shell;

public partial class LootPanel : ComponentBase
{
    private sealed class LootItem
    {
        public int?   AgentId      { get; init; }
        public required string Type         { get; init; }
        public required string Name         { get; init; }
        public required string PathOrValue  { get; init; }
        public long?  Size         { get; init; }
        public long   Received     { get; set; }
        public required string Status       { get; set; }
        public required DateTime Collected  { get; init; }
    }

    [Inject] private IJSRuntime           JS          { get; set; } = default!;
    [Inject] private TeamServerConnection Connection  { get; set; } = default!;
    [Inject] private IHttpClientFactory   HttpFactory { get; set; } = default!;
    [Inject] private IConfiguration       Config      { get; set; } = default!;

    private readonly List<LootItem> _items =
    [
        new() { AgentId = 1, Type = "Credential", Name = "sample-account", PathOrValue = "sample-password", Status = "saved", Collected = new DateTime(2026, 10, 21, 14, 6, 0) },
        new() { AgentId = 1, Type = "Note",       Name = "host details",   PathOrValue = "Windows 11 · x64 · domain joined", Status = "saved", Collected = new DateTime(2026, 10, 21, 13, 40, 0) },
    ];

    private string?      _loadError;
    private bool         _isLoading;
    private LootItem?    _selected;
    private string       _agentFilter = "";
    private string       _typeFilter  = "";
    private string?      _copyStatus;
    private bool         _noteOpen;
    private string       _noteAgent = "";
    private string       _noteName  = "";
    private string       _noteText  = "";
    private string?      _noteError;

    private IEnumerable<string> _agentIds => _items
        .Where(i => i.AgentId is not null)
        .Select(i => i.AgentId!.Value.ToString())
        .Distinct().Order();

    private IEnumerable<LootItem> Filtered => _items.Where(i =>
        (_agentFilter.Length == 0 || i.AgentId.ToString() == _agentFilter)
        && (_typeFilter.Length == 0 || i.Type == _typeFilter));

    private bool CanSave   => _selected is { Type: "Download", Status: "complete" };
    private bool CanCancel => _selected is { Type: "Download", Status: "chunked" };

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private Uri? ResolveBaseUri()
    {
        if (Connection.BaseAddress is not null) return Connection.BaseAddress;
        var c = Config["TeamServer:BaseUrl"];
        return string.IsNullOrWhiteSpace(c) ? null : new Uri(c.TrimEnd('/') + "/");
    }

    private async Task LoadAsync()
    {
        var baseUri = ResolveBaseUri();
        if (baseUri is null) { _loadError = "Connect to a TeamServer to load downloads."; return; }
        _isLoading = true;
        try
        {
            var http     = HttpFactory.CreateClient();
            var response = await http.GetAsync(new Uri(baseUri, "api/v1/files?folderName=Loot"));
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                _items.RemoveAll(i => i.Type == "Download");
                _loadError = null;
                return;
            }
            response.EnsureSuccessStatusCode();
            var paths = await response.Content.ReadFromJsonAsync<List<string>>() ?? [];
            _items.RemoveAll(i => i.Type == "Download");
            _items.InsertRange(0, paths.Select(p => new LootItem
            {
                Type = "Download", Name = Path.GetFileName(p), PathOrValue = p,
                Received = 0, Status = "complete", Collected = DateTime.UtcNow,
            }));
            _loadError = null;
        }
        catch (Exception e) when (e is HttpRequestException or JsonException or TaskCanceledException)
        {
            _loadError = ServerDown.Is(e) ? ServerDown.Message : $"Could not load downloads: {e.Message}";
        }
        finally { _isLoading = false; }
    }

    private async Task SaveToDiskAsync()
    {
        if (_selected is null) return;
        var baseUri = ResolveBaseUri();
        if (baseUri is null) return;
        var url = new Uri(baseUri, $"api/v1/files/download/{Uri.EscapeDataString(_selected.Name)}?folderName=Loot");
        await JS.InvokeVoidAsync("open", url.ToString(), "_blank");
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
