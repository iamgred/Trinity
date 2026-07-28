//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Net;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using TeamServer.Listeners;
using TeamServer.Modules;

namespace TeamServer.Services.Factories
{
    public class HttpCommModuleFactory : ModuleFactory
    {
        private ILogger<HttpCommModule> _logger;
        private HttpListenerFactory _httpListenerFactory;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Default constructor 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="httpListenerFactory"></param>
        public HttpCommModuleFactory(ILogger<HttpCommModule> logger, HttpListenerFactory httpListenerFactory)
        {
            _logger = logger;
            _httpListenerFactory = httpListenerFactory;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates a HTTPCommModule 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="headers"></param>
        /// <param name="userAgent"></param>
        /// <param name="rotationStrategy"></param>
        /// <returns></returns>
        public override Module CreateModule(string name, string host, int port, Dictionary<string, string> headers, string userAgent)
        {
            HttpListener listener = _httpListenerFactory.CreateListener(host, port);
            return new HttpCommModule(_logger, name, listener, headers, userAgent);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//