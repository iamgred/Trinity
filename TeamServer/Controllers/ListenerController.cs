//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //    
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TeamServer.DTOs.Listeners;
using TeamServer.Modules;
using TeamServer.Services;

namespace TeamServer.Controllers
{
    [Route(Routes.Listeners)]
    [ApiController]
    public class ListenerController : ControllerBase
    {
        private ListenerService _listenerService;

        public ListenerController(ListenerService listenerService)
        {
            _listenerService = listenerService;
        }
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Listeners() 
        {
            IEnumerable<HttpCommModule> result = _listenerService.GetListeners();
            return Ok(result);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("http")]
        public IActionResult StartHttpListener([FromBody] HttpListenerDto httpListenerDto)
        {
            Stopwatch watch = Stopwatch.StartNew();
            Console.WriteLine($"StartHttpListener current threadId: {Environment.CurrentManagedThreadId}");
            HttpListenerDto result = _listenerService.StartHttpListener(httpListenerDto);
            watch.Stop();

            Console.WriteLine($"Total time: {watch.ElapsedMilliseconds}ms");

            if (!String.IsNullOrEmpty(result.Error))
            {
                return BadRequest(result);
            }
            Console.WriteLine($"StartHttpListener current threadId: {Environment.CurrentManagedThreadId}");
            return Ok(result);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPut("http")]
        public async Task<IActionResult> UpdateHttpListener([FromBody] HttpListenerDto httpListenerDto)
        {
            HttpListenerDto result = await _listenerService.UpdateHttpListener(httpListenerDto);

            if (!String.IsNullOrEmpty(result.Error))
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpDelete("http")]
        public IActionResult StopHttpListener([FromQuery] string moduleId)
        {
            bool result = _listenerService.StopHttpListener(moduleId);

            if (!result)
            {
                return BadRequest();
            }

            return Ok();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("tcp")]
        public IActionResult StartTcpListener()
        {
            return Ok();
        }
    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //