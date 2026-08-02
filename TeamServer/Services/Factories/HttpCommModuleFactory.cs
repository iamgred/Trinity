//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Net;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using TeamServer.Exceptions;
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
        /// Default constructor.
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
        /// Creates a HTTPCommModule.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="c2Port"></param>
        /// <param name="bindPort"></param>
        /// <param name="headers"></param>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        /// <exception cref="ModuleCreationException"></exception>
        public override Module CreateModule(string name, int c2Port, int bindPort, Dictionary<string, string> headers, List<string> hosts, string userAgent)
        {
            if (c2Port <= 0 || c2Port > 65535)
            {
                throw new ModuleCreationException("C2 port numbers must be between 0 and 65535.");
            }

            HttpListener listener = _httpListenerFactory.CreateListener(bindPort);
            return new HttpCommModule(_logger, name, listener, c2Port, bindPort, headers, hosts, userAgent);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//