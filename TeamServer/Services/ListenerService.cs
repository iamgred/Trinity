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
        private HttpListenerFactory _httpFactory;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public ListenerService(ILogger<ListenerService> logger, HttpListenerFactory httpFactory)
        {
            _logger = logger;
            _httpListeners = new List<HttpCommModule>();
            _httpFactory = httpFactory;
        }

        // TODO: Create user-defined type that contains the listener 
        // Specifies => Allowed request type (e.g. POST, GET ect...)
        // Add exception handler for invalid URI => e.g. when a improper base URI is given
        // Conditional that checks the request type to determine whether it should respond or redirect 
        // Conditional that checks the Agent type from request
        // Conditional that checks Headers

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public HttpListenerDto StartHttpListener(HttpListenerDto httpListenerDto)
        {
            try
            {
                HttpListener httpListener = (HttpListener)_httpFactory.CreateListener(httpListenerDto.Host, httpListenerDto.Port);
                HttpCommModule module = new HttpCommModule(httpListenerDto.Name, httpListener, httpListenerDto.Headers, httpListenerDto.UserAgent);

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

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public void StartTcpListener() { }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public IEnumerable<HttpCommModule> GetListeners()
        {
            return _httpListeners;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public void Deletelistener() { }

    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //