using Refit;
using Trinity.Shared.DTOs.Payload;
using Trinity.Shared.Interfaces;

namespace Client.Services;

public sealed record PayloadEntry(
    string Name,
    long Size,
    string Type,
    DateTime CreatedAt);

public class PayloadService(IPayloadAPI api)
{
    public async Task<List<PayloadEntry>> GetTasksAsync()
    {
        try
        {
            var dtos = await api.GetPayloadsAsync() ?? [];
            return dtos.Select(d => new PayloadEntry(d.Name, d.Size, d.Type, d.CreateTime)).ToList();
        }
        catch (ApiException) { return []; }
    }

    public async Task<bool> GenerateAsync(PayloadCreationDTO dto)
    {
        try { await api.GenerateAsync(dto); return true; }
        catch (ApiException) { return false; }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try { await api.DeleteAsync(id); return true; }
        catch (ApiException) { return false; }
    }

    public string GetDownloadUrl(int payloadId) => $"/api/v1/payloads/{payloadId}";
}
