
using System.Text.RegularExpressions;
using Trinity.Shared.DTOs.Command;
//using Trinity.Shared.DTOs.Commands;

namespace Client.Interpreter
{

    // BASIC USSAGE 
    /*
     *       CommandStrategyInterpreter interpreter = new CommandStrategyInterpreter();
            var commanddto = interpreter.ProcessRawInput("powershell test");
            var test = interpreter.GetCommandEndpoint(commanddto);
     */

    // Have to create a strategy for each command 
    // specify the keyword for instance shell
    public class PowerShellDtoStrategy
    {
        public string Keyword => "powershell";
        public PowerShellDTO Execute(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
            {
                throw new ArgumentException("Powershell script cannot be empty...");
            }

            string trimmed = script.Trim();
            var match = Regex.Match(trimmed, @"^(?<cmd>[^\s]+)\s*(?<args>.*)$");

            return new PowerShellDTO
            {
                Commandlet = match.Groups["cmd"].Value,
                Arguements = string.IsNullOrWhiteSpace(match.Groups["args"].Value) ? null : match.Groups["args"].Value
            };
        }
    }

    public class CommandInterpreter
    {
        private readonly Dictionary<string, Func<string, object>> _strategyCache;

        public CommandInterpreter()
        {
            var psStrategy = new PowerShellDtoStrategy();

            _strategyCache = new Dictionary<string, Func<string, object>>(StringComparer.OrdinalIgnoreCase)
            {
                {psStrategy.Keyword, (payload) => psStrategy.Execute(payload)},
            };
        }

        public object ProcessRawInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Command input cannot be null!");
            }

            string cleanedInput = input.Trim();
            var parseMatch = Regex.Match(cleanedInput, @"^(?<keyword>[^\s]+)\s*(?<payload>.*)$");
            if (!parseMatch.Success)
            {
                throw new FormatException("Invalid command...");
            }

            string strategyKeyword = parseMatch.Groups["keyword"].Value;
            string command = parseMatch.Groups["payload"].Value;

            if (!_strategyCache.TryGetValue(strategyKeyword, out var executeFunc))
            {
                throw new NotSupportedException("Invalid command...");
            }

            return executeFunc(command);
        }

        // Add the endpoints below, this is used in the HTTP client to make the request to the correct endpoint
        // /api/v1/........
        public string GetCommandEndpoint(object commandDTO)
        {
            switch (commandDTO)
            {
                case PowerShellDTO ps:
                    return "endpoint";
                default:
                    throw new InvalidOperationException();
            }

        }
    }
}
