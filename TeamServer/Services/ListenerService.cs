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

        // TODO: Create user-defined type that contains the listener 
        // Specifies => Allowed request type (e.g. POST, GET ect...)
        // Add exception handler for invalid URI => e.g. when a improper base URI is given
        // Conditional that checks the request type to determine whether it should respond or redirect 
        // Conditional that checks the Agent type from request
        // Conditional that checks Headers

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
                    _logger.LogInformation($"HTTP listener started on port: {httpListenerDto.Port}");
                    _httpListeners.Add(module);
                }
                return httpListenerDto;
            }
            catch (ListenerCreationException ex)
            {
                _logger.LogInformation($"{ex.Message}");
                httpListenerDto.Error = $"{ex.Message}";
            }
            return httpListenerDto;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Stops a module's HTTP listener 
        /// </summary>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public bool StopHttpListener(string moduleId)
        {
            try
            {
                HttpCommModule? module = _httpListeners.Find(m => m.Id.Equals(moduleId));

                if (module == null)
                {
                    return false;
                }
                module.Stop();
            }
            catch (ListenerAlreadyActiveException ex)
            {
                _logger.LogInformation($"{ex.Message}");
                throw;
            }
            return true;
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