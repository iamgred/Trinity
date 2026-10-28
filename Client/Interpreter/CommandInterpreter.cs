//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Client.Interpreter.Strategies;
using Trinity.Shared.DTOs.Command;
using Trinity.Shared.Errors;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Results;

namespace Client.Interpreter
{
    public class CommandInterpreter : ICommandInterpreter
    {
        private readonly Dictionary<string, Func<int, string?, Task<Result<CommandCreationResponse>>>> _strategyCache;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initializes a new instance of the CommandInterpreter class with the specified command strategies.
        /// </summary>
        /// <param name="psStrategy">The PowerShell command strategy.</param>
        /// <param name="shellStrategy">The shell command strategy.</param>
        /// <param name="runAsStrategy">The run-as command strategy.</param>
        /// <param name="runStrategy">The run command strategy.</param>
        /// <param name="runUStrategy">The run-u command strategy.</param>
        /// <param name="killProcessStrategy">The kill process command strategy.</param>
        /// <param name="escalateStrategy">The escalate command strategy.</param>
        /// <param name="dotNetAsmStrategy">The .NET assembly command strategy.</param>
        /// <param name="downloadStrategy">The download command strategy.</param>
        /// <param name="cancelDownloadStrategy">The cancel download command strategy.</param>
        /// <param name="uploadStrategy">The upload command strategy.</param>
        /// <param name="spawnToStrategy">The spawn-to command strategy.</param>
        /// <param name="updateHostsStrategy">The update hosts command strategy.</param>
        /// <param name="killStrategy">The kill command strategy.</param>
        /// <param name="getUIDStrategy">The get UID command strategy.</param>
        public CommandInterpreter(PowerShellStrategy psStrategy, ShellStrategy shellStrategy, RunAsStrategy runAsStrategy,
            RunStrategy runStrategy, RunUStrategy runUStrategy, KillProcessStrategy killProcessStrategy, EscalateStrategy escalateStrategy,
            DotNetAsmStrategy dotNetAsmStrategy, DownloadStrategy downloadStrategy, CancelDownloadStrategy cancelDownloadStrategy,
            UploadStrategy uploadStrategy, SpawnToStrategy spawnToStrategy, UpdateHostsStrategy updateHostsStrategy,
            KillStrategy killStrategy, GetUIDStrategy getUIDStrategy)
        {
            _strategyCache = new Dictionary<string, Func<int, string?, Task<Result<CommandCreationResponse>>>>(StringComparer.OrdinalIgnoreCase)
            {
                { psStrategy.Keyword, (agentID, command) => psStrategy.ProcessCommandAsync(agentID, command) },
                { shellStrategy.Keyword, (agentID, command) => shellStrategy.ProcessCommandAsync(agentID, command) },
                { runStrategy.Keyword, (agentID, command) => runAsStrategy.ProcessCommandAsync(agentID, command) },
                { runStrategy.Keyword, (agentID, command) => runStrategy.ProcessCommandAsync(agentID, command) },
                { runUStrategy.Keyword, (agentID, command) => runUStrategy.ProcessCommandAsync(agentID, command) },
                { killProcessStrategy.Keyword, (agentID, command) => killProcessStrategy.ProcessCommandAsync(agentID, command) },
                { escalateStrategy.Keyword, (agentID, command) => escalateStrategy.ProcessCommandAsync(agentID, command) },
                { dotNetAsmStrategy.Keyword, (agentID, command) => dotNetAsmStrategy.ProcessCommandAsync(agentID, command) },
                { downloadStrategy.Keyword, (agentID, command) => downloadStrategy.ProcessCommandAsync(agentID, command) },
                { cancelDownloadStrategy.Keyword, (agentID, command) => cancelDownloadStrategy.ProcessCommandAsync(agentID, command) },
                { uploadStrategy.Keyword, (agentID, command) => uploadStrategy.ProcessCommandAsync(agentID, command) },
                { spawnToStrategy.Keyword, (agentID, command) => spawnToStrategy.ProcessCommandAsync(agentID, command) },
                { updateHostsStrategy.Keyword, (agentID, command) => updateHostsStrategy.ProcessCommandAsync(agentID, command) },
                { killStrategy.Keyword, (agentID, command) => killStrategy.ProcessCommandAsync(agentID, command) },
                { getUIDStrategy.Keyword, (agentID, command) => getUIDStrategy.ProcessCommandAsync(agentID, command) }
            };
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Interprets a command for the specified agent and returns the result of command creation.
        /// </summary>
        /// <param name="agentID">The unique identifier of the agent.</param>
        /// <param name="command">The command string to interpret.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result with the command
        /// creation response.</returns>
        public async Task<Result<CommandCreationResponse>> Interpret(int agentID, string command)
        {
            var tokens = command.Split(" ");

            if (!_strategyCache.TryGetValue(tokens[0], out var strategy))
            {
                return CommandError.NotFound(command.Trim());
            }

            return await strategy(agentID, command[1..]);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//