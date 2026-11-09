using Microsoft.AspNetCore.Components;
using MudBlazor;
using Client.Components.Shell;
using Client.Services;

namespace Client.Components.Pages;

public partial class Listeners
{
    [Inject] ListenerApiClient ListenerClient { get; set; } = null!;
    [Inject] ISnackbar Snackbar { get; set; } = null!;
    [Inject] EventLogService EventLog { get; set; } = null!;
    [CascadingParameter] private WorkspaceState Workspace { get; set; } = default!;

    private IReadOnlyList<ListenerResponse> _listeners = [];
    private bool _loading = false;
    private string _search = string.Empty;
    private string _lastRefresh = "—";
    private bool _createOpen = false;
    private string _newName = string.Empty;
    private string _newHost = "0.0.0.0";
    private int _newPort = 8080;
    private bool _creating = false;
    private string _createError = string.Empty;

    private readonly DialogOptions _dialogOptions = new()
    {
        CloseButton = true,
        MaxWidth = MaxWidth.Small,
        FullWidth = true
    };

    private IEnumerable<ListenerResponse> FilteredListeners =>
        string.IsNullOrWhiteSpace(_search)
            ? _listeners
            : _listeners.Where(l =>
                l.name.Contains(_search, StringComparison.OrdinalIgnoreCase) ||
                l.protocol.Contains(_search, StringComparison.OrdinalIgnoreCase));

    protected override async Task OnInitializedAsync() => await LoadListenersAsync();

    private async Task LoadListenersAsync()
    {
        _loading = true;
        try { _listeners = await ListenerClient.GetListenersAsync(); Workspace.Listeners = _listeners; _lastRefresh = DateTime.Now.ToString("HH:mm:ss"); }
        catch (InvalidOperationException) { _listeners = []; }
        catch { _listeners = []; }
        finally { _loading = false; }
    }

    // ListenerResponse has no Status field — all listed listeners are considered active
    private static bool IsActive(ListenerResponse l) => true;

    private void OpenCreateDialog() { _newName = string.Empty; _newHost = "0.0.0.0"; _newPort = 8080; _createError = string.Empty; _createOpen = true; }
    private void CloseCreateDialog() => _createOpen = false;

    private async Task CreateListenerAsync()
    {
        if (string.IsNullOrWhiteSpace(_newName)) { _createError = "Listener name is required."; return; }
        _creating = true; _createError = string.Empty;
        try { await ListenerClient.CreateHttpListenerAsync(_newName, _newHost, _newPort); Snackbar.Add($"Listener '{_newName}' created.", Severity.Success); EventLog.Add("SUCCESS", $"Listener '{_newName}' created."); _createOpen = false; await LoadListenersAsync(); }
        catch (Exception ex) { _createError = ex.Message; }
        finally { _creating = false; }
    }

    private async Task RestartListenerAsync(ListenerResponse listener)
    {
        try { await ListenerClient.RestartListenerAsync(listener.id); Snackbar.Add($"Listener '{listener.name}' restarted.", Severity.Info); EventLog.Add("INFO", $"Listener '{listener.name}' restarted."); await LoadListenersAsync(); }
        catch (Exception ex) { Snackbar.Add($"Restart failed: {ex.Message}", Severity.Error); }
    }

    private async Task DeleteListenerAsync(ListenerResponse listener)
    {
        try { await ListenerClient.DeleteListenerAsync(listener.id); Snackbar.Add($"Listener '{listener.name}' deleted.", Severity.Warning); EventLog.Add("WARN", $"Listener '{listener.name}' deleted."); await LoadListenersAsync(); }
        catch (Exception ex) { Snackbar.Add($"Delete failed: {ex.Message}", Severity.Error); }
    }
}