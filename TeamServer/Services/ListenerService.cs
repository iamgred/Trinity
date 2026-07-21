//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
using System.Net;

namespace TeamServer.Services
{
    public class ListenerService : BackgroundService
    {
        private HttpListener _httpListener;

        // TODO: Create user-defined type that contains the listener 
        // Specifies => Allowed request type (e.g. POST, GET ect...)
        // Conditional that checks the request type to determine whether it should respond or redirect 

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public void StartHttpListener()
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add("http://localhost:8212/");
            _httpListener.Start();

            if (_httpListener.IsListening)
            {
                Console.WriteLine("Listening on port 8888");
            }

            IAsyncResult result = _httpListener.BeginGetContext(new AsyncCallback(HandleRequest), _httpListener);
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

            HttpListenerContext context = _httpListener.EndGetContext(result);
            HttpListenerRequest request = context.Request;

            if (!request.HttpMethod.Equals(HttpMethod.Get))
            {

            }
            
            HttpListenerResponse response = context.Response;

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
        public void GetListeners() { }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public void Deletelistener() { }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartHttpListener();

            while (!stoppingToken.IsCancellationRequested)
            {
                HttpListenerContext context = await _httpListener.GetContextAsync();
                Console.WriteLine("Background task has started!");
            }
        }
    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //