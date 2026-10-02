//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.Extensions.Logging;
using System.Net;
using Trinity.Shared.Errors;
using Trinity.Shared.Results;

namespace Trinity.Shared.Modules
{
    public class HttpModule
    {
        private HttpListener _httpListener { get; set; }
        private CancellationTokenSource _cts;
        private ILogger<HttpModule> _logger;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="httpListener"></param>
        public HttpModule(ILogger<HttpModule> logger, HttpListener httpListener)
        {
            _httpListener = httpListener;
            _cts = new CancellationTokenSource();
            _logger = logger;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Starts an asynchronous loop that accepts incoming HTTP requests and dispatches each request to a background
        /// task for processing until cancellation is requested.
        /// </summary>
        /// <remarks>Requires HttpListener to be started. Each incoming request is handled concurrently
        /// via Task.Run; exceptions thrown by request handlers are not observed here. Use the associated
        /// CancellationTokenSource to stop polling.</remarks>
        /// <returns>A task representing the asynchronous operation; completes when polling stops (for example, when cancellation
        /// is requested).</returns>
        public async Task StartPolling()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                HttpListenerContext context = await _httpListener.GetContextAsync();
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
                _httpListener.Stop();
                _httpListener.Prefixes.Clear();

                _httpListener.Prefixes.Add(CreateURI(bindport));
                await RestartAsync();

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
        public async Task RestartAsync()
        {
            _httpListener.Stop();
            _httpListener.Start();
            await _cts.CancelAsync();
            _cts = new CancellationTokenSource();
            _ = Task.Run(async() => StartPolling());
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Stop the HTTP listener and asynchronously cancel ongoing operations.
        /// </summary>
        /// <remarks>HttpListener is stopped synchronously; cancellation of pending work is performed
        /// asynchronously via the cancellation token source. Await the returned task to ensure cancellation handlers
        /// and cleanup have completed; pending requests may be aborted or canceled.</remarks>
        /// <returns>A task that represents the asynchronous shutdown operation.</returns>
        public async Task ShutdownAsync()
        {
            _httpListener.Stop();
            await _cts.CancelAsync();
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//