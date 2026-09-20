//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Trinity.Shared.DTOs.TeamServer;

namespace TeamServer.Controllers
{
    [Route(Routes.Server)]
    [ApiController]
    public class ServerController : ControllerBase
    {
        private IPAddress _IPv4;
        private IPAddress _IPv6;
        private ILogger<ServerController> _logger;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Default constructor 
        /// </summary>
        /// <param name="logger"></param>
        public ServerController(ILogger<ServerController> logger)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress? ipV4 = hostEntry.AddressList.FirstOrDefault(h => h.AddressFamily.Equals(AddressFamily.InterNetwork));
            IPAddress? ipV6 = hostEntry.AddressList.FirstOrDefault(h => h.AddressFamily.Equals(AddressFamily.InterNetworkV6));

            if (ipV4 == null || ipV6 == null)
            {
                throw new InvalidOperationException("Could not determine TeamServer IP addresses!");
            }
            this._IPv4 = ipV4;
            this._IPv6 = ipV6;
            this._logger = logger;
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
        [HttpGet("teamserverip")]
        public IActionResult GetServerIP()
        {
            TeamServerIpDTO response = new TeamServerIpDTO { IpV4 = _IPv4.ToString(), IpV6 =  _IPv6.ToString() };
            return Ok(response);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//