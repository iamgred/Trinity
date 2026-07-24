//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
using System.Net;
using TeamServer.DTOs.Listeners;
using TeamServer.Modules;

namespace TeamServer.Services
{
    public class ListenerService 
    {
        private HttpListener _httpListener;

        // TODO: Create user-defined type that contains the listener 
        // Specifies => Allowed request type (e.g. POST, GET ect...)
        // Conditional that checks the request type to determine whether it should respond or redirect 

        public void CreateListener()
        {
            try
            {
                UriBuilder uriBuilder = new UriBuilder("http", "localhost", 8212);
                HttpCommModule module = new HttpCommModule("test", uriBuilder.ToString(), "testAgent");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public HttpListenerDto StartHttpListener(HttpListenerDto httpListenerDto)
        {
            try
            {
                UriBuilder uriBuilder = new UriBuilder("http", httpListenerDto.Host, httpListenerDto.Port);
                HttpCommModule module = new HttpCommModule("test", uriBuilder.ToString(), "testAgent");
                module.HttpListener.Start();

                if (module.HttpListener.IsListening)
                {
                    Console.WriteLine($"Listener has started on port: {httpListenerDto.Port}!");
                }
                return httpListenerDto;
            }
            catch (HttpListenerException e)
            {
                // log the exception here
                Console.WriteLine(e.ToString());
                httpListenerDto.Error = "Port is currently occupied!";
            }
            return httpListenerDto;
            //IAsyncResult result = _httpListener.BeginGetContext(new AsyncCallback(HandleRequest), _httpListener);
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

        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    StartHttpListener();

        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        HttpListenerContext context = await _httpListener.GetContextAsync();
        //        Console.WriteLine("Background task has started!");
        //    }
        //}
    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //