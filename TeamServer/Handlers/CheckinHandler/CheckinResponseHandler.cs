//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Text.Json;
using TeamServer.Handlers.CheckinHandler.Strategies;
using Trinity.Shared.Enums.Tasks;
using Trinity.Shared.Interfaces;

namespace TeamServer.Handlers.CheckinHandler
{
    public class CheckinResponseHandler : ICheckinResponseHandler
    {
        private readonly Dictionary<Enum, Func<JsonElement, Task>> _handlers;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
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
                { Status.SUCCESS, (element) => successStrategy.ProcessResponseAsync(element) },
                { Status.UPLOAD_CHUNK, (element) => uploadStrategy.ProcessResponseAsync(element) },
                { Status.UPLOAD_FINAL, (element) => uploadStrategy.ProcessResponseAsync(element) },
                { Status.DOWNLOAD_CHUNK, (element) => downloadStrategy.ProcessResponseAsync(element) },
                { Status.DOWNLOAD_FINAL, (element) => downloadStrategy.ProcessResponseAsync(element) },
                { Status.KEYLOGGER, (element) => keyloggerStrategy.ProcessResponseAsync(element) },
                { Status.RUNNING, (element) => runningStrategy.ProcessResponseAsync(element) },
                { Status.ERROR, (element) => errorStrategy.ProcessResponseAsync(element) }
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
           var status = (Status)element.GetProperty("status").GetInt32();
           _handlers.TryGetValue(status, out var strategy);

           await strategy!(element);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//