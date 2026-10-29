using System.Text.Json;
using Client.Components.Shell;
using Client.Services;
using Microsoft.AspNetCore.Components;
using Trinity.Shared.DTOs.Task;

namespace Client.Components.Pages.Logs;

public partial class Logs : IAsyncDisposable
{
    private sealed record EventEntry(DateTime Time, string Category, string Tone, IReadOnlyList<EventPart> Parts);

    private sealed record EventPart(string Text, string Style = "");

    [Inject]
    private TaskApiClient TaskApiClient { get; set; } = default!;

    [CascadingParameter]
    private WorkspaceState State { get; set; } = default!;

    private readonly CancellationTokenSource _stop = new();
    private List<EventEntry> _events = [];
    private string? _loadError;
    private bool _isLoading;

    protected override async Task OnInitializedAsync()
    {
        State.OpenTab("Event Log", "logs");
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
        catch (OperationCanceledException)
        {
        }
    }

    // TODO(connect to backend): no event log yet, so show tasks instead
    private async Task LoadAsync()
    {
        _isLoading = true;
        try
        {
            var tasks = await TaskApiClient.GetTasksAsync(_stop.Token);
            _events = tasks.OrderBy(t => t.timeStamp).Select(ToEvent).ToList();
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

    private static EventEntry ToEvent(TaskResponse task)
    {
        var failed = TaskStatuses.IsFailed(task.status);
        var time = task.timeStamp.Kind == DateTimeKind.Utc ? task.timeStamp.ToLocalTime() : task.timeStamp;
        EventPart[] parts = failed
            ?
            [
                new(task.agentID.ToString(), "em"),
                new($" {task.commandType} failed: "),
                new(task.status, "err"),
            ]
            :
            [
                new(task.agentID.ToString(), "em"),
                new($" {task.commandType} · {task.status}"),
            ];

        return new EventEntry(time, failed ? "error" : "agent", failed ? "red" : "green", parts);
    }

    public ValueTask DisposeAsync()
    {
        _stop.Cancel();
        _stop.Dispose();
        return ValueTask.CompletedTask;
    }
}
