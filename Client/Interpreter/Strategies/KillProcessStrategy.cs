//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Trinity.Shared.DTOs.Command;
using Trinity.Shared.Errors;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Results;

namespace Client.Interpreter.Strategies
{
    public class KillProcessStrategy
    {
        public readonly string Keyword = "kill";
        private readonly ICommandAPI _client;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initializes a new instance of the KillProcessStrategy class using the specified command API client.
        /// </summary>
        /// <param name="client">The command API client to use for command execution.</param>
        public KillProcessStrategy(ICommandAPI client)
        {
            _client = client;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Queues a kill process command for the specified agent and returns the result asynchronously.
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

            if (int.TryParse(command.Trim(), out var pid))
            {
                var dto = new KillProcessDTO
                {
                    PID = pid
                };

                return await _client.QueueRunUCommandAsync(agentID, dto);
            }

            return CommandError.TooFewArgs(Keyword);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//