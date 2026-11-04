//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Text.Json;
using TeamServer.Handlers.CheckinHandler.Strategies;
using Trinity.Shared.Enums.Tasks;
using Trinity.Shared.Interfaces;
using static Trinity.Shared.Enums.Tasks.Status;

namespace TeamServer.Handlers.CheckinHandler
{
    public class CheckinResponseHandler : ICheckinResponseHandler
    {
        private readonly Dictionary<Status, Func<JsonElement, Task>> _handlers;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// Default constuctor.
        /// Intialises checkin response handling strategies.
        public CheckinResponseHandler(
            SuccessStrategy successStrategy,
            RunningStrategy runningStrategy,
            ErrorStrategy errorStrategy,
            UploadChunkStrategy uploadChunkStrategy,
            KeyloggerStrategy keyloggerStrategy,
            DownloadChunkStrategy downloadChunkStrategy)
        {
            _handlers = new Dictionary<Status, Func<JsonElement, Task>>()
            {
                { SUCCESS, (element) => successStrategy.ProcessResponseAsync(element) },
                { RUNNING, (element) =>  runningStrategy.ProcessResponseAsync(element) },
                { ERROR, (element) => errorStrategy.ProcessResponseAsync(element) },
                { COMPLETED, (element) => keyloggerStrategy.ProcessResponseAsync(element) },
                { MORE_TO_FOLLOW, (element) => uploadChunkStrategy.ProcessResponseAsync(element) },
                { NO_MORE_TO_FOLLOW, (element) => uploadChunkStrategy.ProcessResponseAsync(element) },
                { DOWNLOAD_CHUNK_STATUS, (element) => downloadChunkStrategy.ProcessResponseAsync(element) },
                { DOWNLOAD_FINAL_STATUS, (element) => downloadChunkStrategy.ProcessResponseAsync(element) }
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
            var status = (Status)element.GetProperty("Status").GetInt32();
            _handlers.TryGetValue(status, out var strategy);

            await strategy!(element);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//