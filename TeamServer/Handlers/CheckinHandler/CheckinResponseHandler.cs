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
        private readonly Dictionary<Status, Func<JsonElement, Task>> _handlers;

        /// Default constuctor.
        /// Intialises checkin response handling strategies.
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        public CheckinResponseHandler(SuccessStrategy successStrategy, RunningStrategy runningStrategy, ErrorStrategy errorStrategy)
        {
            _handlers = new Dictionary<Status, Func<JsonElement, Task>>()
            {
                { SUCCESS, (element) => successStrategy.ProcessResponseAsync(element) },
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
            var status = (Status)element.GetProperty("Status").GetInt32();

            _handlers.TryGetValue(status, out var strategy);
            if (strategy == null)
            {
                throw new ArgumentException("Invalid status provided in task header!");
            }

            await strategy(element);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//