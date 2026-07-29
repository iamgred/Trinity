//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
using System.Net;
using TeamServer.DTOs.Listeners;
using TeamServer.Exceptions;
using TeamServer.Modules;
using TeamServer.Services.Factories;

namespace TeamServer.Services
{
    public class ListenerService 
    {
        private List<HttpCommModule> _httpListeners;
        private ILogger _logger;
        private HttpCommModuleFactory _httpModuleFactory;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public ListenerService(ILogger<ListenerService> logger, HttpCommModuleFactory httpFactory)
        {
            _logger = logger;
            _httpListeners = new List<HttpCommModule>();
            _httpModuleFactory = httpFactory;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates a HTTP module and starts its respective listener.
        /// </summary>
        /// <param name="httpListenerDto"></param>
        /// <returns></returns>
        public HttpListenerDto StartHttpListener(HttpListenerDto httpListenerDto)
        {
            try
            {
                HttpCommModule module = (HttpCommModule)_httpModuleFactory.CreateModule(httpListenerDto.Name,
                    httpListenerDto.Host, httpListenerDto.Port,
                    httpListenerDto.Headers, httpListenerDto.UserAgent);

                module.Start();
                if (module.HttpListener.IsListening)
                {
                    _logger.LogInformation("HTTP listener started on {host}:{port}", httpListenerDto.Host, httpListenerDto.Port);
                    _httpListeners.Add(module);
                }
            }
            catch (ListenerCreationException ex)
            {
                _logger.LogWarning(ex, "Module creation failed: {Message}", ex.Message);
                httpListenerDto.Error = $"Creation error: {ex.Message}";
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Module runtime startup failed: {Message}", ex.Message);
                httpListenerDto.Error = $"Runtime error: {ex.Message}";
            }
            return httpListenerDto;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Stops a module's HTTP listener and removes the respective module
        /// </summary>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public bool StopHttpListener(string moduleId)
        {
            try
            {
                HttpCommModule? module = _httpListeners.FirstOrDefault(m => m.Id.ToString().Equals(moduleId));

                if (module != null)
                {
                    module.Stop();
                    _httpListeners.Remove(module);
                    return true;
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Module runtime startup failed: {Message}", ex.Message);
            }
            return false;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public void StartTcpListener() { }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public IEnumerable<HttpCommModule> GetListeners()
        {
            return _httpListeners;
        }

    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //