using Client.Components.Shell;
using Microsoft.AspNetCore.Components;
using Trinity.Shared.Enums;

namespace Client.Components.Pages.Logs;

// shows the live events EventLogFeed collects (see WorkspaceLayout)
public partial class Logs : IDisposable
{
    [CascadingParameter]
    private WorkspaceState State { get; set; } = default!;

    protected override void OnInitialized()
    {
        State.OpenTab("Event Log", "logs");
        State.Changed += OnStateChanged;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    // colours match the Figma labels
    private static string ToneFor(EventLogType type) => type switch
    {
        EventLogType.OPERATOR => "blue",
        EventLogType.LISTENER => "purple",
        EventLogType.PAYLOAD => "orange",
        EventLogType.AGENT => "green",
        EventLogType.ARMOURY => "yellow",
        _ => "grey",
    };

    private static string LocalTime(DateTime time) =>
        (time.Kind == DateTimeKind.Utc ? time.ToLocalTime() : time).ToString("HH:mm:ss");

    private string EmptyText => State.EventsLive ? "Waiting for events…" : "Connecting to the live event log…";

    public void Dispose() => State.Changed -= OnStateChanged;
}
