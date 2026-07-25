//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
using System.Net;
using TeamServer.DTOs.Listeners;
using TeamServer.Modules;

namespace TeamServer.Services
{
    public class ListenerService 
    {
        private List<HttpCommModule> _httpListeners;
        private ILogger _logger;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public ListenerService(ILogger<ListenerService> logger)
        {
            _logger = logger;
            _httpListeners = new List<HttpCommModule>();
        }

        // TODO: Create user-defined type that contains the listener 
        // Specifies => Allowed request type (e.g. POST, GET ect...)
        // Add exception handler for invalid URI => e.g. when a improper base URI is given
        // Conditional that checks the request type to determine whether it should respond or redirect 
        // Conditional that checks the Agent type from request
        // Conditional that checks Headers

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public HttpListenerDto StartHttpListener(HttpListenerDto httpListenerDto)
        {
            try
            {
                HttpCommModule module = new HttpCommModule(httpListenerDto.Name, httpListenerDto.Host, httpListenerDto.Port, httpListenerDto.Headers, httpListenerDto.UserAgent);
                module.HttpListener.Start();

                if (module.HttpListener.IsListening)
                {
                    _logger.LogInformation($"HTTP listener started on port: {httpListenerDto.Port}");
                    IAsyncResult result = module.HttpListener.BeginGetContext(new AsyncCallback(HandleRequest), module.HttpListener);
                    _httpListeners.Add(module);
                }
                return httpListenerDto;
            }
            catch (HttpListenerException e)
            {
                _logger.LogInformation($"{e.Message}");
                httpListenerDto.Error = $"{e.Message}";
            }
            catch (ArgumentException ex)
            {
                _logger.LogInformation($"{ex.ToString()}");
                httpListenerDto.Error = $"{ex.Message}";
            }
            return httpListenerDto;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// 
        private void HandleRequest(IAsyncResult result)
        {
            Console.WriteLine("Request recieved!");

            HttpListener listener = (HttpListener)result.AsyncState;
            listener.Prefixes.First();

            // Used to retrieve the associated module of the listener
            var module = _httpListeners.Find(l => l.HttpListener.Prefixes.Equals(listener.Prefixes));

            HttpListenerContext context = listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            var headers = request.Headers;
            module.ValidateRequestHeaders((WebHeaderCollection)headers);
            
            string responseString = "<HTML><BODY> Recieved </BODY></HTML>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

            response.ContentLength64 = buffer.Length;
            System.IO.Stream responseStream = response.OutputStream;
            responseStream.Write(buffer, 0, buffer.Length);
            responseStream.Close();

        }
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public void StartTcpListener() { }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public IEnumerable<HttpCommModule> GetListeners()
        {
            return _httpListeners;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public void Deletelistener() { }

    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //