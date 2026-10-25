//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Text.RegularExpressions;
using Trinity.Shared.DTOs.Command;
using Trinity.Shared.Errors;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Results;

namespace Client.Interpreter.Strategies
{
    public class RunStrategy : ICommandStrategy
    {
        public readonly string Keyword = "run";
        private readonly ICommandAPI _client;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initializes a new instance of the RunStrategy class using the specified command API client.
        /// </summary>
        /// <param name="client">The command API client to use for command execution.</param>
        public RunStrategy(ICommandAPI client)
        {
            _client = client;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Queues a run command for the specified agent and returns the result asynchronously.
        /// </summary>
        /// <param name="agentID">The unique identifier of the agent.</param>
        /// <param name="command">The runAs command to execute.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the command output.</returns>
        public async Task<Result<CommandCreationResponse>> ProcessCommandAsync(int agentID, string? command)
        {
            if (string.IsNullOrEmpty(command))
            {
                return CommandError.TooFewArgs(Keyword);
            }

            var match = Regex.Match(command, @"^(\S*)(\s\S*)?");
            if (!match.Success)
            {
                return CommandError.TooFewArgs(Keyword);
            }

            var dto = new RunDTO
            {
                Program = match.Groups[1].Value,
                Arguements = match.Groups[2].Value.Trim()
            };

            return await _client.QueueRunCommandAsync(agentID, dto);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//