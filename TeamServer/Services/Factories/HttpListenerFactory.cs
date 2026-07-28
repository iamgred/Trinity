//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Net;
using TeamServer.Exceptions;
using TeamServer.Modules;

namespace TeamServer.Services.Factories
{
    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
    /// <summary>
    /// HttpListenerFactory
    /// </summary>
    public class HttpListenerFactory : ListenerFactory
    {
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates and returns an HttpListener bound to the specified host and port.
        /// </summary>
        /// <remarks>Returned listener is not started; call Start() to begin accepting requests.</remarks>
        /// <param name="host">Host name or IP address to bind the listener to.</param>
        /// <param name="port">TCP port number to bind the listener to.</param>
        /// <returns>An HttpListener (as IDisposable) configured with a prefix for http://{host}:{port}/.</returns>
        /// <exception cref="ListenerCreationException">Thrown when host or port are invalid or when 
        /// the listener cannot bind because the address is already
        /// registered.</exception>
        public override HttpListener CreateListener(string host, int port)
        {
            if (port <= 0 || port > 65535)
            {
                throw new ListenerCreationException("Port numbers must be between 0 and 65535.");
            }

            try
            {
                UriBuilder uriBuilder = new UriBuilder("http", host, port);
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add(uriBuilder.ToString());
                return listener;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is UriFormatException || ex is HttpListenerException)
            {
                throw new ListenerCreationException($"Failed to initialise listener on {host}:{port}.",ex);
            }
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//