//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Sockets;

namespace TeamServer.Controllers
{
    [Route(Routes.Server)]
    [ApiController]
    public class ServerController : ControllerBase
    {
        private IPAddress? _IPv4;
        private IPAddress? _IPv6;
        private ILogger<ServerController> _logger;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Default constructor 
        /// </summary>
        /// <param name="logger"></param>
        public ServerController(ILogger<ServerController> logger)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            _IPv4 = hostEntry.AddressList.FirstOrDefault(h => h.AddressFamily.Equals(AddressFamily.InterNetwork));
            _IPv6 = hostEntry.AddressList.FirstOrDefault(h => h.AddressFamily.Equals(AddressFamily.InterNetworkV6));
            _logger = logger;
        }
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("status")]
        public IActionResult GetServerStatus()
        {
            return Ok();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpGet("teamserverIp")]
        public IActionResult GetServerIP()
        {
            if (_IPv4 != null)
            {
                _logger.LogInformation($"Returned teamserver IPv4: {_IPv4}");
                return Ok(_IPv4.ToString());
            }
            _logger.LogInformation("Teamserver IPv4 is null");
            return BadRequest();
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//