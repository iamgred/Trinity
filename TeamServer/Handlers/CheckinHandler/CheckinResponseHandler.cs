//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Text.Json;
using TeamServer.Handlers.CheckinHandler.Strategies;
using Trinity.Shared.Enums.Tasks;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Results;
using static Trinity.Shared.Enums.Tasks.Status;

namespace TeamServer.Handlers.CheckinHandler
{
    public class CheckinResponseHandler : ICheckinResponseHandler
    {
        private readonly Dictionary<Enum, Func<JsonElement, Task>> _handlers;

        /// Default constuctor.
        /// Intialises checkin response handling strategies.
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        public CheckinResponseHandler(
            SuccessStrategy successStrategy,
            KeyloggerStrategy keyloggerStrategy,
            UploadStrategy uploadStrategy,
            DownloadStrategy downloadStrategy,
            RunningStrategy runningStrategy,
            ErrorStrategy errorStrategy)
        {
            _handlers = new Dictionary<Enum, Func<JsonElement, Task>>()
            {
                { SUCCESS, (element) => successStrategy.ProcessResponseAsync(element) },
                { TaskType.KEYLOGGER_STOP, (element) => keyloggerStrategy.ProcessResponseAsync(element) },
                { TaskType.UPLOAD, (element) => uploadStrategy.ProcessResponseAsync(element) },
                { TaskType.DOWNLOAD, (element) => downloadStrategy.ProcessResponseAsync(element) },
                { RUNNING, (element) =>  runningStrategy.ProcessResponseAsync(element) },
                { ERROR, (element) => errorStrategy.ProcessResponseAsync(element) }
            };
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task HandleResponseAsync(JsonElement element)
        {
            var taskType = element.TryGetProperty("TaskType", out var taskTypeElement)
                ? (TaskType?)((TaskType)taskTypeElement.GetInt32())
                : null;
            var status = (Status)element.GetProperty("Status").GetInt32();
            var key = status == Status.SUCCESS && taskType.HasValue
                && Enum.IsDefined(taskType.Value)
                ? (Enum)taskType.Value
                : status;

            if (!_handlers.TryGetValue(key, out var strategy))
                throw new ArgumentException($"Unsupported check-in response '{key}'.", nameof(element));

            await strategy(element);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//