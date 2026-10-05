using System.Net.Http.Json;
using System.Text.Json;
using Trinity.Shared.DTOs.Task;

namespace Client.Services;

public sealed class TaskApiClient(HttpClient httpClient, TeamServerConnection connection)
{
    public async Task<IReadOnlyList<TaskResponse>> GetTasksAsync(CancellationToken cancellationToken = default)
    {
        if (connection.BaseAddress is null)
        {
            throw new InvalidOperationException("Connect to a TeamServer from the Login page before viewing TeamServer data.");
        }

        var requestUri = new Uri(connection.BaseAddress, "api/v1/tasks/tasks");
        var tasks = await httpClient.GetFromJsonAsync<List<TaskResponse>>(requestUri, cancellationToken);
        return tasks ?? throw new JsonException("The TeamServer returned an empty task list response.");
    }

    // Fetches a single task's detail (incl. command payload); returns null if no task has that ID.
    public async Task<GetTaskDetailsResponse?> GetTaskDetailAsync(int taskId, CancellationToken cancellationToken = default)
    {
        if (connection.BaseAddress is null)
        {
            throw new InvalidOperationException("Connect to a TeamServer from the Login page before viewing TeamServer data.");
        }

        var requestUri = new Uri(connection.BaseAddress, $"api/v1/tasks/tasks/{taskId}");
        var response = await httpClient.GetAsync(requestUri, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        // An empty 200 also means no match.
        if (response.Content.Headers.ContentLength == 0)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<GetTaskDetailsResponse>(cancellationToken);
    }
}
