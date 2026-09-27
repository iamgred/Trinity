//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using TeamServer.DTOs.Listeners;
using TeamServer.Exceptions;
using TeamServer.Modules;
using TeamServer.Repositories;
using TeamServer.Services.Factories;
using TeamServer.Utils;
using Trinity.Shared.DTOs.Listener;
using Trinity.Shared.Enums;
using Trinity.Shared.Models;

namespace TeamServer.Services
{
    public class ListenerService 
    {
        private ILogger _logger;
        private ListenerRespository _listenerRepo;
        private ProtocolRespository _protocolRepo;
        private ConcurrentDictionary<string, HttpCommModule> _httpCommModules;
        private HttpCommModuleFactory _httpModuleFactory;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Default constructor 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="httpFactory"></param>
        public ListenerService(ILogger<ListenerService> logger, HttpCommModuleFactory httpFactory, ListenerRespository listenerRepo, ProtocolRespository protocolrepo)
        {
            _logger = logger;
            _httpCommModules = new();
            _httpModuleFactory = httpFactory;
            _listenerRepo = listenerRepo;
            _protocolRepo = protocolrepo;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates a HttpCommModule and starts its respective listener.
        /// </summary>
        /// <param name="httpListenerDto"></param>
        /// <returns></returns>
        public HttpListenerDto StartHttpListener(HttpListenerDto httpListenerDto)
        {
            try
            {
                HttpCommModule module = (HttpCommModule)_httpModuleFactory.CreateModule(httpListenerDto.Name,
                    httpListenerDto.C2Port, httpListenerDto.BindPort,
                    httpListenerDto.Headers, httpListenerDto.Hosts, httpListenerDto.UserAgent);

                module.Start();

                if (module.HttpListener.IsListening)
                {
                    _logger.LogInformation("HTTP listener started on {port}", httpListenerDto.BindPort);
                    _httpCommModules.TryAdd(module.Id, module);
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Module runtime startup failed: {Message}", ex.Message);
                httpListenerDto.Error = $"Runtime error: {ex.Message}";
            }
            catch (Exception ex) when (ex is ListenerCreationException || ex is ModuleCreationException)
            {
                _logger.LogWarning(ex, "Module creation failed: {Message}", ex.Message);
                httpListenerDto.Error = $"Creation error: {ex.Message}";
            }
            return httpListenerDto;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<HttpListenerDto> UpdateHttpListener(HttpListenerDto httpListenerDto)
        {
            try
            {
                HttpCommModule module = _httpCommModules[httpListenerDto.Id];
                await module.Update(httpListenerDto.Hosts, httpListenerDto.Headers, String.Empty, httpListenerDto.C2Port, httpListenerDto.BindPort);
                return httpListenerDto;
            }
            catch (Exception ex) when (ex is InvalidOperationException)
            {
                _logger.LogWarning(ex, "Module update failed: {Message}", ex.Message);
                httpListenerDto.Error = ex.Message;
            }
            return httpListenerDto;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Stops a module's HTTP listener and removes the respective module.
        /// </summary>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public bool StopHttpListener(string moduleId)
        {
            try
            {
                if (_httpCommModules.ContainsKey(moduleId))
                {
                    HttpCommModule module;
                    bool result = _httpCommModules.Remove(moduleId, out module!);
                    module.Stop();
                    return result;
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Module runtime startup failed: {Message}", ex.Message);
            }
            return false;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<List<ListenerResponse>> GetListenersAsync()
        {
            return await _listenerRepo.GetListenersAsync();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<CreateHttpListenerResponse> CreateHttpListenerAsync(CreateHttpListenerRequest request)
        {
            var protocol = await _protocolRepo.GetByIdAsync(1);
            var response = await _listenerRepo.AddListenerAsync(BasicHttpMapper(ref request, protocol.ID));
            return response;
        }

        private Listener BasicHttpMapper(ref CreateHttpListenerRequest request, int protocolID)
        {
            return new Listener
            {
                Name = request.Name,
                ProtocolID = protocolID,
                Config = JsonDocument.Parse(JsonSerializer.Serialize(request))
            };
        }


    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //