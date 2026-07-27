//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Net;
using TeamServer.Listeners;

namespace TeamServer.Modules
{
    public class HttpCommModule : Listener
    {
        public override Guid Id { get; }
        public HttpListener HttpListener { get; set; }
        public string HostRotationStrategy { get; set; }
        public string UserAgent { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public override ListenerType Type { get; set; }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Default constructor for HTTP modules
        /// </summary>
        /// <param name="name"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="headers"></param>
        /// <param name="uri"></param>
        /// <param name="userAgent"></param>
        /// <param name="rotationStrategy"></param>
        public HttpCommModule(string name, HttpListener httpListener,  Dictionary<string, string>? headers, string userAgent = "", string rotationStrategy = "")
        {
            Id = Guid.NewGuid();
            HttpListener = httpListener;
            Name = name;
            HostRotationStrategy = rotationStrategy;
            UserAgent = userAgent;
            Headers = headers != null ? headers : new();
            Type = ListenerType.HTTP;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public bool ValidateRequestHeaders(WebHeaderCollection header)
        {
            var keys = header.Keys;

            if (keys.Count <= 0)
            {
                return false;
            }

            for (int i = 0; i < keys.Count; i++)
            {
                Headers.ContainsKey(keys.Get(i));
            }

            return false;
        }



    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//