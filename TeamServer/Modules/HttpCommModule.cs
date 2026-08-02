//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Collections.Concurrent;
using System.Net;
using TeamServer.Exceptions;
using TeamServer.Listeners;

namespace TeamServer.Modules
{
    public class HttpCommModule : Module
    {
        public override string Id { get; }
        public int HttpC2port { get; set; }
        public int HttpBindport { get; set; }
        public HttpListener HttpListener { get; set; }
        public string HostRotationStrategy { get; set; }
        public string UserAgent { get; set; }
        public ConcurrentDictionary<string, string> Headers { get; set; }
        public HashSet<string> Hosts { get; set; }
        public override ListenerType Type { get; set; }
        private CancellationTokenSource _cts;
        private readonly Lock _threadLock;
        private readonly SemaphoreSlim _semaphore;
        private bool _isDisposed;
        private ILogger<HttpCommModule> _logger;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="name"></param>
        /// <param name="httpListener"></param>
        /// <param name="headers"></param>
        /// <param name="userAgent"></param>
        /// <param name="rotationStrategy"></param>
        public HttpCommModule(ILogger<HttpCommModule> logger, string name, HttpListener httpListener, int httpC2Port, int httpBindport, Dictionary<string, string>? headers, List<string>? hosts, string userAgent = "", string rotationStrategy = "")
        {
            Id = Guid.NewGuid().ToString();
            HttpC2port = httpC2Port;
            HttpBindport = httpBindport;
            HttpListener = httpListener;
            Name = name;
            HostRotationStrategy = rotationStrategy;
            UserAgent = userAgent;
            Headers = headers != null ? new(headers) : new();
            Hosts =  hosts != null ? new(hosts) : new();
            Type = ListenerType.HTTP;
            _cts = new CancellationTokenSource();
            _logger = logger;
            _threadLock = new();
            _semaphore = new(1);
            _isDisposed = false;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Starts the HttpCommModule's listener. 
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
                _ = HandleRequestAsync(_cts.Token);
            }
            catch (HttpListenerException ex)
            {
                _logger.LogError(ex, "Failed to bind HTTP listener #{Id} to the network interface.", Id);
                throw new InvalidOperationException("Cannot bind listener, the host/port combination is already registered.", ex);
            }
        }
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Queues the agent HTTP request for processing on the threadpool
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task HandleRequestAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && HttpListener.IsListening)
            {
                try
                {
                    HttpListenerContext context = await HttpListener.GetContextAsync();
                    _ = Task.Run(() => ProcessRequest(context), token);
                }
                catch (Exception) when (token.IsCancellationRequested)
                {
                    _logger.LogInformation("HTTP listener #{id} stopped via token cancellation", Id);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,"Critical error in HTTP listener #{Id}", Id);
                }
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
                Console.WriteLine($"Http module HandleRequestAsync current threadId: {Environment.CurrentManagedThreadId}");
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
                _logger.LogError(ex, "Failed to process agent request on HTTP listener #{Id}.", Id);
            }
            finally
            {
                try
                {
                    context.Response.Close();
                    _logger.LogInformation("HTTP listener #{id} sent response to agent.", Id);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(ex, "Failed to close response context; client likely disconnected prematurely.");
                }
            }
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Updates HTTP module.
        /// </summary>
        /// <param name="hosts"></param>
        /// <param name="headers"></param>
        /// <param name="rotationStrategy"></param>
        /// <param name="c2port"></param>
        /// <param name="bindport"></param>
        /// <returns></returns>
        public async Task Update(List<string> hosts, Dictionary<string,string> headers, string rotationStrategy, int c2port, int bindport)
        {
            // TODO: needs conditionals to first check if restart is required => if there is a bindport that differs from the currently set one.
            await _semaphore.WaitAsync();
            try
            {
                await UpdateHosts(hosts);
                await UpdateHeaders(headers);
                HttpC2port = c2port;
                await Restart(bindport);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        private Task UpdateHosts(List<string> hosts)
        {
            if (hosts.Count <= 0)
            {
                throw new InvalidOperationException("HTTP listener must have hosts.");
            }

            return Task.Run(() =>
            {
                for (int i = 0; i < hosts.Count; i++)
                {
                    Hosts.Clear();
                    Hosts.Add(hosts[i]);
                }
            });
           
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        private Task UpdateHeaders(Dictionary<string, string> headers)
        {
            return Task.Run(() =>
            {
                if (headers.Count > 0)
                {
                    for (int i = 0; i < headers.Count; i++)
                    {
                        Headers.Clear();
                        Headers.AddOrUpdate(headers.ElementAt(i).Key, headers.ElementAt(i).Value, (key, oldvalue) => headers.ElementAt(i).Value);
                    }
                }
                else
                {
                    Headers.Clear();
                }
            });
        }

        private Task Restart(int bindPort)
        {
            if (bindPort <= 0 || bindPort > 65535)
            {
                throw new ListenerCreationException("Bind port numbers must be between 0 and 65535.");
            }

            return Task.Run(() =>
            {
                UriBuilder uriBuilder = new UriBuilder("http", "localhost", bindPort);
                HttpListener.Stop();
                HttpListener.Prefixes.Clear();
                HttpListener.Prefixes.Add(uriBuilder.ToString());
                HttpListener.Start();
                HttpBindport = bindPort;
            });
        }



        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Terminates the Http listener and the processing of ongoing requests
        /// <remarks>
        /// The lock is needed to prevent multiple threads from attempting to dispose
        /// HttpListener. 
        /// </remarks>
        /// </summary>
        public void Stop()
        {
            lock (_threadLock)
            {
                if (_isDisposed)
                {
                    throw new InvalidOperationException($"HTTP listener #{Id} already disposed.");
                }

                _cts.Cancel();
                HttpListener.Stop();
                HttpListener.Close();
                _cts.Dispose();
                _isDisposed = true;
                _logger.LogInformation("HTTP listener #{id} has been stopped.", Id);
            }
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//