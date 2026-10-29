using Trinity.Shared.DTOs.Agent;

namespace Client.Components.Shell;

public sealed class WorkspaceState
{
    public IReadOnlyList<AgentResponse> Agents { get; set; } = [];
    public string? AgentsError { get; set; }
    public bool Connected { get; set; }
    public int ListenerCount { get; set; }
    public string Filter { get; set; } = "";
    public int? SelectedAgentId { get; set; }
    public int AgentsPanelHeight { get; set; } = 300;

    public List<ShellTab> Tabs { get; } = [new("Event Log", "logs")];

    public event Action? Changed;

    public void Notify() => Changed?.Invoke();

    public IEnumerable<AgentResponse> FilteredAgents
    {
        get
        {
            var filter = Filter.Trim();
            var agents = Agents.OrderBy(a => SinceLastCheckIn(a));
            if (filter.Length == 0)
            {
                return agents;
            }

            return agents.Where(a =>
                a.agentID.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)
                || a.username.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || a.externalIP.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || a.internalIP.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || a.processName.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }
    }

    // TODO(connect to backend): no link to the parent agent, so guess SMB from the listener name
    public static bool IsSmb(AgentResponse agent) => agent.listener.Contains("smb", StringComparison.OrdinalIgnoreCase);

    public int SmbPeerCount => Agents.Count(IsSmb);

    public static TimeSpan SinceLastCheckIn(AgentResponse agent)
    {
        var last = agent.lastCheckIn.Kind == DateTimeKind.Local ? agent.lastCheckIn.ToUniversalTime() : agent.lastCheckIn;
        var since = DateTime.UtcNow - last;
        return since < TimeSpan.Zero ? TimeSpan.Zero : since;
    }

    // stale after 3 missed check-ins
    public static bool IsStale(AgentResponse agent) => SinceLastCheckIn(agent) > TimeSpan.FromSeconds(Math.Max(agent.sleep, 1) * 3);

    public static string FormatAgo(TimeSpan span) => span.TotalSeconds < 60
        ? $"{(int)span.TotalSeconds}s"
        : span.TotalMinutes < 60 ? $"{(int)span.TotalMinutes}m {span.Seconds}s" : $"{(int)span.TotalHours}h {span.Minutes}m";

    public void OpenTab(string title, string href)
    {
        if (Tabs.All(t => t.Href != href))
        {
            Tabs.Add(new ShellTab(title, href));
            Notify();
        }
    }
}

public sealed record ShellTab(string Title, string Href);
