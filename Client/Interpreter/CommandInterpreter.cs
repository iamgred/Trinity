//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Client.Interpreter.Strategies;
using Trinity.Shared.Interfaces;

namespace Client.Interpreter
{

    public class CommandInterpreter : ICommandInterpreter
    {
        private readonly Dictionary<string, Func<int, string, Task<string>>> _strategyCache;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public CommandInterpreter(ICommandAPI client)
        {
            var psStrategy = new PowerShellStrategy(client);

            _strategyCache = new Dictionary<string, Func<int, string, Task<string>>>(StringComparer.OrdinalIgnoreCase)
            {
                { psStrategy.Keyword, (agentID, command) => psStrategy.ProcessCommandAsync(agentID, command) },
            };
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<string> Interpret(int agentID, string command)
        {
            var tokens = command.Split(" ");

            if (!_strategyCache.TryGetValue(tokens[0], out var strategy))
            {
                return $"Unknown command: {command}";
            }

            return await strategy(agentID, command[1..]);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//