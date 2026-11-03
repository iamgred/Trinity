using Trinity.Shared.Results;

namespace TeamServer.Interface;

public interface IKeyloggerService
{
    Task<Result<string>> SaveDataAsync(int campaignId, string data);
}
