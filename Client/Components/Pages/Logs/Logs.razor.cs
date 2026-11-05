using System.Text.Json;
using Client.Components.Shell;
using Client.Services;
using Microsoft.AspNetCore.Components;
using TrinityTask = Trinity.Shared.Models.Task;

namespace Client.Components.Pages.Logs;

public partial class Logs : IAsyncDisposable
{
    private sealed record EventEntry(DateTime Time, string Category, string Tone, IReadOnlyList<EventPart> Parts);
    private sealed record EventPart(string Text, string Style = "");

    [Inject]
    private TaskApiService TaskApiClient { get; set; } = default!;

    [CascadingParameter]
    private WorkspaceState? State { get; set; }

    private readonly CancellationTokenSource _stop = new();
    private List<EventEntry> _events = [];
    private string? _loadError;
    private bool _isLoading;

    protected override async Task OnInitializedAsync()
    {
        State?.OpenTab("Event Log", "logs");
        await LoadAsync();
        _ = PollAsync();
    }

    private async Task PollAsync()
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        try
        {
            while (await timer.WaitForNextTickAsync(_stop.Token))
            {
                await LoadAsync();
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException) { }
    }

    /* TODO: no event log yet, show tasks instead */
    private async Task LoadAsync()
    {
        _isLoading = true;
        try
        {
            var tasks = await TaskApiClient.GetTasksAsync();
            _events = tasks.OrderBy(t => t.CreatedAt).Select(ToEvent).ToList();
            _loadError = null;
        }
        catch (HttpRequestException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            /* /api/v1/tasks not yet implemented on the team server */
            _events = [];
            _loadError = null;
        }
        catch (Exception e) when (e is HttpRequestException or InvalidOperationException or JsonException or TaskCanceledException)
        {
            _loadError = ServerDown.Is(e) ? ServerDown.Message : $"Could not load events from the team server: {e.Message}";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private static EventEntry ToEvent(TrinityTask task)
    {
        var status = task.Status.ToString();
        var failed = TaskStatuses.IsFailed(status);

        var time = task.CreatedAt.Kind == DateTimeKind.Utc
            ? task.CreatedAt.ToLocalTime()
            : task.CreatedAt;

        EventPart[] parts = failed
            ?
            [
                new(task.AgentID.ToString(), "em"),
                new($" {task.CommandType} failed: "),
                new(status, "err"),
            ]
            :
            [
                new(task.AgentID.ToString(), "em"),
                new($" {task.CommandType} · {status}"),
            ];

        return new EventEntry(
            time,
            failed ? "error" : "agent",
            failed ? "red" : "green",
            parts);
    }

    public ValueTask DisposeAsync()
    {
        _stop.Cancel();
        _stop.Dispose();
        return ValueTask.CompletedTask;
    }
}