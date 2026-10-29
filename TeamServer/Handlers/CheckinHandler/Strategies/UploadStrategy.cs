using System.Text.Json;
using Trinity.Shared.Interfaces;

namespace TeamServer.Handlers.CheckinHandler.Strategies;

public sealed class UploadStrategy : ICheckinStrategy
{
    private readonly SuccessStrategy _successStrategy;

    public UploadStrategy(SuccessStrategy successStrategy)
    {
        _successStrategy = successStrategy;
    }

    public Task ProcessResponseAsync(JsonElement element) =>
        _successStrategy.ProcessResponseAsync(element);
}
