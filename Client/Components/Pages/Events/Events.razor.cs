using Microsoft.AspNetCore.Components;

namespace Client.Components.Pages;

public partial class Events : ComponentBase
{
    private record EventEntry(string Stamp, string Level, string Message);

    private List<EventEntry> _events = new();

    private string SevClass(string level) => level switch
    {
        "ERROR" or "CRIT" => "sev-error",
        "WARN" => "sev-warn",
        "SUCCESS" => "sev-ok",
        _ => "sev-info"
    };
}