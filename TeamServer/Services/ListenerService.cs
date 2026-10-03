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
using Trinity.Shared.Interfaces;
using Trinity.Shared.DTOs.Listener.Tor;

namespace TeamServer.Services
{
    public class ListenerService
    {
        private ILogger _logger;
        private ListenerRespository _listenerRepo;
        private ProtocolRespository _protocolRepo;
        private ConcurrentDictionary<int, ICommunicationListener> _httpCommModules;
        private readonly ListenerFactory _listenerFactory;
        private readonly HttpModuleFactory _moduleFactory;
        private readonly HttpListenerManager _httpListenerManager;
        private readonly ICommunicationMessageHandler _messageHandler;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Default constructor 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="httpFactory"></param>
        public ListenerService(
            ICommunicationMessageHandler messageHandler,
            ILogger<ListenerService> logger,
            ListenerRespository listenerRepo,
            ProtocolRespository protocolrepo,
            ListenerFactory factory,
            HttpModuleFactory moduleFactory,
            HttpListenerManager httpListenerManager)
        {
            _messageHandler = messageHandler;
            _logger = logger;
            _httpCommModules = new();
            _listenerRepo = listenerRepo;
            _protocolRepo = protocolrepo;
            _listenerFactory = factory;
            _moduleFactory = moduleFactory;
            _httpListenerManager = httpListenerManager;
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
                var module = _moduleFactory.Create(request.Config.BindPort, _messageHandler);
                _ = System.Threading.Tasks.Task.Run(async () => module.StartAsync());

                int protocolID = await _protocolRepo.GetProtocolIDAsync("HTTP");
                Listener listener = _listenerFactory.CreateHttpListener(request, protocolID);
                await _listenerRepo.AddListenerAsync(listener);
                await _listenerRepo.CommitAsync();

                _httpListenerManager.AddModule(listener.ID, module);
                return Result.Success();
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
            await _listenerRepo.AddListenerAsync(listener);

            await _listenerRepo.CommitAsync();
            return Result.Success();
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
            await _listenerRepo.AddListenerAsync(listener);

            await _listenerRepo.CommitAsync();
            return Result.Success();
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
            var listener = await _listenerRepo.GetListenerAsync(ID);
            return listener == null ? ListenerError.NotFound(ID) : new GetListenerDetailsResponse(listener.ID, listener.Name, listener.Config);
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
            var listener = await _listenerRepo.GetListenerAsync(ID);

            if (listener == null)
            {
                return ListenerError.NotFound(ID);
            }

            _listenerRepo.DeleteListener(listener);
            await _listenerRepo.CommitAsync();
            return Result.Success();
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
            var listener = await _listenerRepo.GetListenerAsync(ID);
            if (listener == null)
            {
                return ListenerError.NotFound(ID);
            }
            listener.Config = JsonDocument.Parse(JsonSerializer.Serialize(config));
            await _listenerRepo.CommitAsync();

            return Result.Success();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Restarts the listener identified by the specified ID and returns a Result indicating success or failure.
        /// </summary>
        /// <param name="ID">The identifier of the listener to restart.</param>
        /// <returns>A Result representing the operation outcome. Returns Success if the listener was restarted; returns NotFound
        /// if no listener exists with the given ID.</returns>
        public async Task<Result> RestartListenerAsync(int ID)
        {
            var module = _httpListenerManager.GetModuleByID(ID);

            if (module == null)
            {
                return ListenerError.NotFound(ID);
            }

            await module.StopAsync();
            await module.StartAsync();
            return Result.Success();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        ///
        public async Task<Result> RemoveListenerAsync(int ID)
        {
            var listener = await _listenerRepo.GetListenerAsync(ID);

            if (listener == null)
            {
                return ListenerError.NotFound(ID);
            }

            _listenerRepo.DeleteListener(listener);
            await _listenerRepo.CommitAsync();

            var module = _httpListenerManager.GetModuleByID(ID);

            if (module != null)
            {
                await module.StopAsync();
                _httpListenerManager.RemoveModule(ID);
            }
            return Result.Success();
        }
        /// <summary>
        /// Creates a Tor listener using the provided request and persists it to the repository.
        /// </summary>
        /// <param name="request">The request containing the listener configuration.</param>
        /// <returns>A Result indicating the success or failure of the operation.</returns>
        public async Task<Result> CreateTorListenerAsync(CreateTorListenerRequest request)
        {
            int protocolID = await _protocolRepo.GetProtocolIDAsync("TOR");

            Listener listener =
                _listenerFactory.CreateTorListener(request, protocolID);

            await _listenerRepo.AddListenerAsync(listener);
            await _listenerRepo.CommitAsync();

            return Result.Success();
        }
    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //