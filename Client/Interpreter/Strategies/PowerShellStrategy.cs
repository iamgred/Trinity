//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Text.RegularExpressions;
using Trinity.Shared.DTOs.Command;
using Trinity.Shared.Interfaces;

namespace Client.Interpreter.Strategies
{
    public class PowerShellStrategy : ICommandStrategy
    {
        public readonly string Keyword = "powershell";
        private readonly ICommandAPI _client;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public PowerShellStrategy(ICommandAPI client)
        {
            _client = client;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<string> ProcessCommandAsync(int agentID, string command)
        {
            if (command.Split().Length < 2)
            {
                return "Powershell command does not have enough arguements";
            }

            var match = Regex.Match(command.Trim(), @"^(?<command>[^\s]+)\s*(?<args>.$)");
            return await _client.QueuePowerShellCommandAsync(agentID, new PowerShellDTO { Commandlet = match.Groups["command"].Value, Arguements = match.Groups["args"].Value });
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//