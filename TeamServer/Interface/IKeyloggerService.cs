using Trinity.Shared.Results;

namespace TeamServer.Interface;

public interface IKeyloggerService
{
    Task<Result<string>> SaveDataAsync(string data);
    Task<Result<byte[]>> GetFileAsync(string fileName);
}
