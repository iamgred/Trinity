using Refit;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Models;

namespace Client.Services;

public class ArmouryService(IArmouryAPI api, IConfiguration config)
{
    public async Task<List<Tool>> GetToolsAsync()
    {
        try { return await api.GetToolsAsync(0, 1000) ?? []; }
        catch (ApiException) { return []; }
    }

    public async Task<bool> RemoveAsync(int toolID)
    {
        try { await api.RemoveToolAsync(toolID); return true; }
        catch (ApiException) { return false; }
    }

    public async Task<bool> UploadAsync(string name, int operatorId, Stream stream, string fileName)
    {
        try
        {
            var baseUrl = config["TeamServer:BaseUrl"]?.TrimEnd('/') ?? "";
            using var client = new HttpClient { BaseAddress = new Uri(baseUrl + "/") };
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(name), "Name");
            form.Add(new StringContent(operatorId.ToString()), "OperatorID");
            var sc = new StreamContent(stream);
            sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            form.Add(sc, "file", fileName);
            var resp = await client.PostAsync("/api/v1/armoury/upload", form);
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}
