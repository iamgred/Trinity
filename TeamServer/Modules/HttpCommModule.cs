//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Net;
using TeamServer.Exceptions;
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
        private CancellationTokenSource _cts;

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
            _cts = new CancellationTokenSource();
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
                    return;
                }

                HttpListener.Start();
                _ = HandleResponseAsync(_cts.Token);
            }
            catch (HttpListenerException ex)
            {
                throw new ListenerCreationException("");
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
                // log exceptions in the awaited Task if there is any 
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
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                string responseString = "<HTML><BODY> Recieved </BODY></HTML>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                context.Response.Close();
            }
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if (!HttpListener.IsListening)
            {
                // throw listener already stopped exception here
                return;
            }
            _cts.Cancel();
            HttpListener.Stop();
            HttpListener.Close();
            _cts.Dispose();
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//