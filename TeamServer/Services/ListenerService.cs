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
using Trinity.Shared.DTOs.Listener.Http;
using Trinity.Shared.DTOs.Listener.Smb;
using Trinity.Shared.DTOs.Listener.Tcp;
using Trinity.Shared.Enums;
using Trinity.Shared.Models;
using Trinity.Shared.Results;

namespace TeamServer.Services
{
    public class ListenerService 
    {
        private ILogger _logger;
        private ListenerRespository _listenerRepo;
        private ProtocolRespository _protocolRepo;
        private ConcurrentDictionary<string, HttpCommModule> _httpCommModules;
        private ListenerFactory _listenerFactory;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Default constructor 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="httpFactory"></param>
        public ListenerService(ILogger<ListenerService> logger, ListenerRespository listenerRepo, ProtocolRespository protocolrepo, ListenerFactory factory)
        {
            _logger = logger;
            _httpCommModules = new();
            _listenerRepo = listenerRepo;
            _protocolRepo = protocolrepo;
            _listenerFactory = factory;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates a HttpCommModule and starts its respective listener.
        /// </summary>
        /// <param name="httpListenerDto"></param>
        /// <returns></returns>
        //public HttpListenerDto StartHttpListener(HttpListenerDto httpListenerDto)
        //{
        //    try
        //    {
        //        HttpCommModule module = (HttpCommModule)_httpModuleFactory.CreateModule(httpListenerDto.Name,
        //            httpListenerDto.C2Port, httpListenerDto.BindPort,
        //            httpListenerDto.Headers, httpListenerDto.Hosts, httpListenerDto.UserAgent);

        //        module.Start();

        //        if (module.HttpListener.IsListening)
        //        {
        //            _logger.LogInformation("HTTP listener started on {port}", httpListenerDto.BindPort);
        //            _httpCommModules.TryAdd(module.Id, module);
        //        }
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        _logger.LogWarning(ex, "Module runtime startup failed: {Message}", ex.Message);
        //        httpListenerDto.Error = $"Runtime error: {ex.Message}";
        //    }
        //    catch (Exception ex) when (ex is ListenerCreationException || ex is ModuleCreationException)
        //    {
        //        _logger.LogWarning(ex, "Module creation failed: {Message}", ex.Message);
        //        httpListenerDto.Error = $"Creation error: {ex.Message}";
        //    }
        //    return httpListenerDto;
        //}

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
        /// <summary>
        /// Retrieves all listeners asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. The task result contains a Result wrapping a list of
        /// ListenerResponse.</returns>
        public async Task<Result<List<ListenerResponse>>> GetListenersAsync()
        {
            return await _listenerRepo.GetListenersAsync();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<Result> CreateHttpListenerAsync(CreateHttpListenerRequest request)
        {
            // Create Http comm module 
            // Check if port binds 
            // Yes => all is good dont worry => save to db =. return 200 with ID
            // No => all hell breaks loose => return problem detail 

            int protocolID = await _protocolRepo.GetProtocolIDAsync("HTTP");
            Listener listener = _listenerFactory.CreateHttpListener(request, protocolID);
            var response = await _listenerRepo.AddListenerAsync(listener);
            return response;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates a TCP listener from the specified request, persists it, and returns the created listener response.
        /// </summary>
        /// <remarks>Resolves the TCP protocol identifier, constructs the listener via a factory, and adds
        /// it to the repository.</remarks>
        /// <param name="request">The TCP listener settings used to create the listener.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a Result<CreateHttpListenerResponse>
        /// containing details of the created and persisted listener.</returns>
        public async Task<Result> CreateTCPListenerAsync(CreateTcpListenerRequest request)
        {
            int protocolID = await _protocolRepo.GetProtocolIDAsync("TCP");
            Listener listener = _listenerFactory.CreateTcpListener(request, protocolID);
            var response = await _listenerRepo.AddListenerAsync(listener);
            return response;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates an SMB listener from the specified request, resolves the TCP protocol identifier, and adds the
        /// listener to the repository.
        /// </summary>
        /// <remarks>Resolves the 'TCP' protocol identifier via the protocol repository and uses a factory
        /// to create the Listener before persisting it to the listener repository.</remarks>
        /// <param name="request">Configuration for creating the SMB listener.</param>
        /// <returns>A Result indicating the outcome of the create operation.</returns>
        public async Task<Result> CreateSmbListenerAsync(CreateSmbRequest request)
        {
            int protocolID = await _protocolRepo.GetProtocolIDAsync("TCP");
            Listener listener = _listenerFactory.CreateSmbListener(request, protocolID);
            var response = await _listenerRepo.AddListenerAsync(listener);
            return response;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Gets details for the listener with the specified identifier.
        /// </summary>
        /// <param name="ID">The identifier of the listener to retrieve.</param>
        /// <returns>A Task that returns a Result<GetListenerDetailsResponse> containing the listener details on success or an
        /// error result on failure.</returns>
        public async Task<Result<GetListenerDetailsResponse>> GetListener(int ID)
        {
            return await _listenerRepo.GetListenerAsync(ID);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Deletes the listener with the specified identifier.
        /// </summary>
        /// <param name="ID">Identifier of the listener to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result indicating the outcome
        /// of the delete operation.</returns>
        public async Task<Result> DeleteListenerAsync(int ID)
        {
            // NEED TO CHECK IF LISTENER IS HTTP => STOP AND REMOVE IT FROM THE DICTIONARY
            return await _listenerRepo.DeleteListenerAsync(ID);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Updates the listener identified by ID with the provided configuration.
        /// </summary>
        /// <remarks>The configuration object is serialized with JsonSerializer and parsed into a
        /// JsonDocument prior to the repository update. The operation is performed asynchronously.</remarks>
        /// <param name="ID">Identifier of the listener to update.</param>
        /// <param name="config">Configuration object to apply; serialized to JSON and parsed into a JsonDocument before being forwarded to
        /// the repository.</param>
        /// <returns>A Result indicating the outcome of the update operation.</returns>
        public async Task<Result> UpdateHttpListenerAsync(int ID, HttpConfig config)
        {
            return await _listenerRepo.UpdateListenerAsync(ID, JsonDocument.Parse(JsonSerializer.Serialize(config)));
        }
    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //