//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //    
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            HttpListenerDto result = _listenerService.StartHttpListener(httpListenerDto);

            if (!String.IsNullOrEmpty(result.Error))
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("Tcp")]
        public IActionResult StartTcpListener()
        {
            return Ok();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public IActionResult DeleteListener()
        {
            return Ok();
        }
    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //