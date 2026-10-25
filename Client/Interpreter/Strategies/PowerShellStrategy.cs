//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Text.RegularExpressions;
using Trinity.Shared.DTOs.Command;
using Trinity.Shared.Errors;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Results;

namespace Client.Interpreter.Strategies
{
    public class PowerShellStrategy : ICommandStrategy
    {
        public readonly string Keyword = "powershell";
        private readonly ICommandAPI _client;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initializes a new instance of the PowerShellStrategy class using the specified command API client.
        /// </summary>
        /// <param name="client">The command API client used for executing commands.</param>
        public PowerShellStrategy(ICommandAPI client)
        {
            _client = client;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Queues a PowerShell command for the specified agent and returns the result asynchronously.
        /// </summary>
        /// <param name="agentID">The unique identifier of the agent.</param>
        /// <param name="command">The PowerShell command to execute.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the command output.</returns>
        public async Task<Result<CommandCreationResponse>> ProcessCommandAsync(int agentID, string? command)
        {
            if (string.IsNullOrEmpty(command))
            {
                return CommandError.TooFewArgs(Keyword);
            }

            var cmdMatch = Regex.Match(command, @"(\S*)");
            var argMatch = Regex.Match(command, @"^(\S*)(\s.*\S)");
            var dto = new PowerShellDTO
            {
                Commandlet = cmdMatch.Groups[1].Value,
                Arguements = argMatch.Groups.Count == 3 ? argMatch.Groups[2].Value.Trim() : string.Empty
            };

            return await _client.QueuePowerShellCommandAsync(agentID, dto);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//