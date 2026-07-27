//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Net;
using TeamServer.Exceptions;
using TeamServer.Listeners;

namespace TeamServer.Modules
{
    public class HttpCommModule : Module
    {
        public override Guid Id { get; }
        public HttpListener HttpListener { get; set; }
        public string HostRotationStrategy { get; set; }
        public string UserAgent { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public override ListenerType Type { get; set; }
        private CancellationTokenSource _cts;
        private ILogger<HttpCommModule> _logger;

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
        public HttpCommModule(ILogger<HttpCommModule> logger, string name, HttpListener httpListener,  Dictionary<string, string>? headers, string userAgent = "", string rotationStrategy = "")
        {
            Id = Guid.NewGuid();
            HttpListener = httpListener;
            Name = name;
            HostRotationStrategy = rotationStrategy;
            UserAgent = userAgent;
            Headers = headers != null ? headers : new();
            Type = ListenerType.HTTP;
            _cts = new CancellationTokenSource();
            _logger = logger;
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

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ListenerCreationException"></exception>
        public void Start()
        {
            try
            {
                if (HttpListener.IsListening)
                {
                    _logger.LogInformation($"HTTP listener #{Id} already listening");
                    return;
                }

                HttpListener.Start();
                _ = HandleResponseAsync(_cts.Token);
            }
            catch (HttpListenerException ex)
            {
                throw new ListenerCreationException("Cannot bind on host already registered");
            }
        }
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Handles incoming http requests. 
        /// The returned task stores all non-usage exceptions, exception will be 
        /// thrown if any when it is awaited. 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task HandleResponseAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    HttpListenerContext context = await HttpListener.GetContextAsync();
                    _ = Task.Run(() => ProcessRequest(context));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Performs validation on the agent HTTP request and returns pending tasks
        /// in response. 
        /// </summary>
        /// <param name="context"></param>
        private void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                _logger.LogInformation($"Request recieved from agent");
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                string responseString = "<HTML><BODY> Recieved </BODY></HTML>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            finally
            {
                _logger.LogInformation($"Response sent to agent");
                context.Response.Close();
            }
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Terminates the Http listener and the processing of ongoing requests
        /// </summary>
        public void Stop()
        {
            if (!HttpListener.IsListening)
            {
                throw new ListenerAlreadyActiveException($"HTTP listener #{Id} already disposed");
            }
            _cts.Cancel();
            HttpListener.Stop();
            HttpListener.Close();
            _cts.Dispose();
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//