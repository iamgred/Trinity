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
        public override IDisposable CreateListener(string host, int port)
        {
            try
            {
                UriBuilder uriBuilder = new UriBuilder("http", host, port);
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add(uriBuilder.ToString());
                return listener;
            }
            catch (ArgumentNullException)
            {
                // log exception here
                throw new ListenerCreationException("Invalid argument: host and port cannot be null");
            }
            catch (ArgumentException)
            {
                // log exception here
                throw new ListenerCreationException("Invalid argument host");
            }
            catch (HttpListenerException)
            {
                // log exception here
                // HttpListenerException is only thrown upon intialisation if the host is already registered on the listener
                throw new ListenerCreationException($"Cannot bind on host: http://{host}:{port}/ already registered");
            }
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//