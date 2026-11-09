using Trinity.Shared.DTOs.Command;
using Trinity.Shared.Errors;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Results;

namespace Client.Interpreter.Strategies;

public class PersistStrategy : ICommandStrategy
{
    public readonly string Keyword = "persist";
    private readonly ICommandAPI _client;

    public PersistStrategy(ICommandAPI client)
    {
        _client = client;
    }

    public async Task<Result<string>> ProcessCommandAsync(int agentID, string? command)
    {
        var parts = command?.Trim().Split(' ', 3, StringSplitOptions.RemoveEmptyEntries) ?? [];
        if (parts.Length < 3)
            return CommandError.TooFewArgs(Keyword);

        var dto = new WinTaskDTO
        {
            Path     = parts[0],
            Username = parts[1],
            TaskName = parts[2],
        };

        var task = await _client.QueuePersistCommandAsync(agentID, dto);
        return $"Tasked agent to persist via scheduled task #{task}";
    }
}
