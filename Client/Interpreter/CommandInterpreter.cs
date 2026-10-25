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
        public CommandInterpreter(PowerShellStrategy psStrategy, ShellStrategy shellStrategy, RunAsStrategy runAsStrategy,
            RunStrategy runStrategy, RunUStrategy runUStrategy, KillProcessStrategy killProcessStrategy, EscalateStrategy escalateStrategy,
            DotNetAsmStrategy dotNetAsmStrategy)
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
                { dotNetAsmStrategy.Keyword, (agentID, command) => dotNetAsmStrategy.ProcessCommandAsync(agentID, command) }
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
                return CommandError.NotFound(command);
            }

            return await strategy(agentID, command[1..]);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//