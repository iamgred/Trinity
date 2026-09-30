//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using TeamServer.Repositories;
using TeamServer.Services.Factories;
using Trinity.Shared.DTOs.Listener;
using Trinity.Shared.DTOs.Listener.Http;
using Trinity.Shared.DTOs.Listener.Smb;
using Trinity.Shared.DTOs.Listener.Tcp;
using Trinity.Shared.Errors;
using Trinity.Shared.Models;
using Trinity.Shared.Results;
using Trinity.Shared.Modules;

namespace TeamServer.Services
{
    public class ListenerService 
    {
        private ILogger _logger;
        private ListenerRespository _listenerRepo;
        private ProtocolRespository _protocolRepo;
        private ConcurrentDictionary<int, HttpModule> _httpCommModules;
        private readonly ListenerFactory _listenerFactory;
        private readonly HttpModuleFactory _moduleFactory;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Default constructor 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="httpFactory"></param>
        public ListenerService(ILogger<ListenerService> logger, ListenerRespository listenerRepo, ProtocolRespository protocolrepo, ListenerFactory factory, HttpModuleFactory moduleFactory)
        {
            _logger = logger;
            _httpCommModules = new();
            _listenerRepo = listenerRepo;
            _protocolRepo = protocolrepo;
            _listenerFactory = factory;
            _moduleFactory = moduleFactory;
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
        /// <summary>
        /// Creates and registers an HTTP listener using the supplied request configuration.
        /// </summary>
        /// <remarks>Obtains the HTTP protocol ID, constructs the listener, and adds it to the repository.
        /// Logs binding failures and returns a port-occupied error when an HttpListenerException is thrown.</remarks>
        /// <param name="request">Request containing listener configuration, including the bind port and other settings.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result indicating success or
        /// failure; returns a port-occupied error if the requested port cannot be bound.</returns>
        public async Task<Result> CreateHttpListenerAsync(CreateHttpListenerRequest request)
        {
            try
            {
                var module = _moduleFactory.Create(request.Config.BindPort);
                _ = System.Threading.Tasks.Task.Run(async () => module.StartPolling());
                _httpCommModules.TryAdd(1, module);

                int protocolID = await _protocolRepo.GetProtocolIDAsync("HTTP");
                Listener listener = _listenerFactory.CreateHttpListener(request, protocolID);
                var response = await _listenerRepo.AddListenerAsync(listener);
                return response;
            }
            catch (HttpListenerException ex)
            {
                _logger.LogError(ex, "Could not bind HTTP listener to port '{port}'", request.Config.BindPort);
                return ListenerError.PortOccupied(request.Config.BindPort);
            }
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
            var result = await _listenerRepo.IsListenerHttp(ID);

            if (result.IsSuccess)
            {
                // Stop the listener then process with delete.
            }

            return result.IsSuccess ? await _listenerRepo.DeleteListenerAsync(ID) : result;
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

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<Result> RestartListenerAsync(int ID)
        {
            var result = await _listenerRepo.IsListenerHttp(ID);

            if (!result.IsSuccess)
            {
                return result;
            }
            HttpModule module;
            var moduleExists = _httpCommModules.TryGetValue(ID, out module);

            if (moduleExists)
            {
                await module.Restart();
            }

            return moduleExists ? Result.Success() : result;
        }
    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //