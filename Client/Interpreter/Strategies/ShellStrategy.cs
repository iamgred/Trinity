using Trinity.Shared.DTOs.Command;
using Trinity.Shared.Errors;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Results;

namespace Client.Interpreter.Strategies
{
    public class ShellStrategy : ICommandStrategy
    {
        public readonly string Keyword = "shell";
        private readonly ICommandAPI _client;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initializes a new instance of the ShellStrategy class using the specified command API client.
        /// </summary>
        /// <param name="client">The command API client implementation.</param>
        public ShellStrategy(ICommandAPI client)
        {
            _client = client;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Queues a shell command for the specified agent asynchronously.
        /// </summary>
        /// <param name="agentID">The unique identifier of the agent.</param>
        /// <param name="command">The shell command to execute.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result with the command
        /// creation response.</returns>
        public async Task<Result<CommandCreationResponse>> ProcessCommandAsync(int agentID, string? command)
        {
            if (string.IsNullOrEmpty(command))
            {
                return CommandError.TooFewArgs(Keyword);
            }

            var dto = new ShellDTO { Command = command };

            return await _client.QueueShellCommandAsync(agentID, dto);
        }
    }
}
