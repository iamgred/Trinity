namespace Client.Services;

public sealed class EventLogService
{
    public sealed record EventEntry(string Stamp, string Level, string Message);

    private readonly List<EventEntry> _events = [];
    public IReadOnlyList<EventEntry> Events => _events;

    public event Action? Changed;

    public void Add(string level, string message)
    {
        _events.Add(new EventEntry(DateTime.Now.ToString("HH:mm:ss"), level, message));
        Changed?.Invoke();
    }

    public void Clear()
    {
        _events.Clear();
        Changed?.Invoke();
    }
}
