//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Net;
using TeamServer.Exceptions;
using Trinity.Shared.Results;
using Trinity.Shared.Errors;

namespace TeamServer.Modules
{
    public class HttpModule
    {
        public HttpListener HttpListener { get; set; }
        private CancellationTokenSource _cts;
        private bool _isDisposed;
        private ILogger<HttpModule> _logger;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="httpListener"></param>
        public HttpModule(ILogger<HttpModule> logger, HttpListener httpListener)
        {
            HttpListener = httpListener;
            _cts = new CancellationTokenSource();
            _logger = logger;
            _isDisposed = false;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task StartPolling()
        {
            while (!_cts.Token.IsCancellationRequested) 
            {
                HttpListenerContext context = await HttpListener.GetContextAsync();
                _ = Task.Run(() => ProcessRequest(context), _cts.Token);
            }
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Processes an incoming HttpListenerContext by sending a fixed UTF-8 HTML acknowledgment, logging receipt and
        /// exceptions, and ensuring the response is closed.
        /// </summary>
        /// <remarks>Writes a UTF-8 encoded HTML acknowledgment to the response stream, sets
        /// ContentLength64, logs informational and error events, and closes the response in a finally block.</remarks>
        /// <param name="context">The HttpListenerContext containing the HTTP request and response.</param>
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
                _logger.LogError(ex, "Exception occured: {Message}", ex.Message);
            }
            finally
            {
                context.Response.Close();
            }
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Reconfigure and restart the HTTP listener to bind to the specified port by canceling current operations and
        /// restarting the polling loop.
        /// </summary>
        /// <remarks>Cancels the ongoing polling loop via the internal cancellation token source, clears
        /// and updates HttpListener prefixes, restarts the listener, and starts polling. Logs an error and returns a
        /// specific port-occupied result when binding fails with HttpListenerException.</remarks>
        /// <param name="bindport">Port number to bind the HTTP listener to.</param>
        /// <returns>A Result indicating success, or a port-occupied error if the listener cannot bind to the port.</returns>
        public async Task<Result> Update(int bindport)
        {
            try
            {
                await _cts.CancelAsync();
                HttpListener.Stop();
                HttpListener.Prefixes.Clear();

                HttpListener.Prefixes.Add(CreateURI(bindport));
                await Restart();

                return Result.Success();
            }
            catch (HttpListenerException ex)
            {
                _logger.LogError(ex, "Could not bind HTTP listener to port '{port}'", bindport);
                return ListenerError.PortOccupied(bindport);
            }
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Constructs an HTTP URI for localhost using the specified port.
        /// </summary>
        /// <remarks>Uses System.UriBuilder with scheme "http" and host "localhost".</remarks>
        /// <param name="port">The port number to include in the URI.</param>
        /// <returns>The string representation of the constructed HTTP URI.</returns>
        private string CreateURI(int port)
        {
            var UriBuilder = new UriBuilder("http", "localhost", port);
            return UriBuilder.ToString();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Restarts the HTTP listener and resumes polling.
        /// </summary>
        /// <remarks>Stops and restarts the HttpListener, then awaits StartPolling. Concurrent invocations
        /// may cause unexpected behavior; callers should avoid invoking Restart concurrently and handle any exceptions
        /// thrown by the listener operations.</remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task Restart()
        {
            HttpListener.Stop();
            HttpListener.Start();
            await StartPolling();
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//