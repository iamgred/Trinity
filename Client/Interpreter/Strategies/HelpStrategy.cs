using System.Text;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Results;

namespace Client.Interpreter.Strategies;

public class HelpStrategy : ICommandStrategy
{
    public readonly string Keyword = "help";

    private static readonly Dictionary<string, (string Usage, string Description, string? Detail)> _entries =
        new(StringComparer.OrdinalIgnoreCase)
    {
        ["powershell"]       = ("powershell <script>",                        "Run a PowerShell script or one-liner",                                "Passes <script> to powershell.exe -NonInteractive -EncodedCommand.\nExample: powershell Get-Process"),
        ["download"]         = ("download <path> [destination]",               "Pull a file from the agent to the team server",                       "Retrieves <path> from the agent. [destination] defaults to the filename.\nExample: download C:\\Users\\Admin\\secret.txt\nExample: download C:\\tools\\nc.exe nc64.exe"),
        ["upload"]           = ("upload <path>",                              "Push a file from the team server to the agent",                       "Uploads a staged file to <path> on the agent.\nExample: upload C:\\Temp\\tool.exe"),
        ["execute-assembly"] = ("execute-assembly <path> [args]",             "Execute a .NET assembly in memory",                                   "Loads and runs the managed assembly at <path> without writing to disk.\nExample: execute-assembly C:\\Tools\\Rubeus.exe kerberoast"),
        ["escalate"]         = ("escalate <regKey> <targetBinary>",           "Escalate privileges via a registry key UAC bypass",                   "Uses <regKey> to hijack a privileged auto-elevated binary.\nExample: escalate HKCU\\...\\ms-settings cmd.exe"),
["help"]             = ("help [command]",                             "Show all commands, or detail for a specific command",                 "With no argument: list every command.\nWith a keyword: show its usage and description.\nExample: help download"),
    };

    public Task<Result<string>> ProcessCommandAsync(int agentID, string command)
    {
        var key = command?.Trim() ?? "";

        if (string.IsNullOrEmpty(key))
        {
            var sb = new StringBuilder();
            sb.AppendLine("Commands:");
            foreach (var (kw, (usage, desc, _)) in _entries.OrderBy(e => e.Key))
                sb.AppendLine($"  {usage,-48}  {desc}");
            sb.Append("Type 'help <command>' for detailed usage.");
            return Task.FromResult<Result<string>>(sb.ToString());
        }

        if (_entries.TryGetValue(key, out var info))
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Usage: {info.Usage}");
            sb.AppendLine(info.Description);
            if (info.Detail is not null)
                sb.Append(info.Detail);
            return Task.FromResult<Result<string>>(sb.ToString().TrimEnd());
        }

        return Task.FromResult<Result<string>>($"No help entry for '{key}'. Type 'help' to list all commands.");
    }
}
