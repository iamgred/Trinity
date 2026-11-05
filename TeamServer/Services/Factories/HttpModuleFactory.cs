//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Net;
using TeamServer.Modules;
namespace TeamServer.Services.Factories
{
    public class HttpModuleFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IServiceScopeFactory _scopeFactory;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initializes a new instance of HttpModuleFactory using the specified logger factory.
        /// </summary>
        /// <param name="loggerFactory">The logger factory used to create loggers for components produced by the factory.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="loggerFactory"/> is <c>null</c>.</exception>
        public HttpModuleFactory(ILoggerFactory loggerFactory, IServiceScopeFactory serviceScopeFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _scopeFactory = serviceScopeFactory;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates and starts an HttpModule that listens on http://localhost:{port}/.
        /// </summary>
        /// <remarks>The caller is responsible for disposing the returned HttpModule to stop and release
        /// the underlying HttpListener. Binding can fail if the port is in use or requires elevated privileges; only
        /// the localhost address is registered.</remarks>
        /// <param name="port">TCP port number to bind the HttpListener to.</param>
        /// <exception cref="HttpListenerException>"/>
        /// <returns>An HttpModule with a started HttpListener bound to localhost and the specified port.</returns>
        public HttpModule Create(int port)
        {
            HttpListener httpListener = new HttpListener();
            UriBuilder uriBuilder = new UriBuilder("http", "localhost", port);
            httpListener.Prefixes.Add(uriBuilder.ToString());
            httpListener.Start();

            return new HttpModule(_loggerFactory.CreateLogger<HttpModule>(), httpListener, _scopeFactory);
        }

    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//