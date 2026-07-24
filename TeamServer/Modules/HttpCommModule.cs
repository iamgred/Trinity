//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
using System.Net;

namespace TeamServer.Modules
{
    public class HttpCommModule
    {
        public string Name { get; set; }
        public HttpListener HttpListener { get; set; }
        public string UserAgent { get; set; } = String.Empty;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public HttpCommModule(string name, string uri, string userAgent)
        {
            HttpListener = new HttpListener();
            HttpListener.Prefixes.Add(uri);
            Name = name;
            UserAgent = userAgent;
        }

    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //