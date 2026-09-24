//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Trinity.Shared.DTOs.TeamServer;
using Trinity.Shared.Exceptions;

namespace TeamServer.Controllers
{
    [Route(Routes.Server)]
    [ApiController]
    public class ServerController : ControllerBase
    {
        private readonly IPAddress _ipv4;
        private readonly IPAddress _ipv6;
        private ILogger<ServerController> _logger;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Default constructor.
        /// Fails immediately if host cannot be resolved.
        /// </summary>
        /// <exception cref="IpResolutionException"></exception>
        /// <param name="logger"></param>
        public ServerController(ILogger<ServerController> logger)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName()) ?? throw new IpResolutionException("DNS resolution falied: Unable to resolve teamserver's host name.");
            this._ipv4 = hostEntry.AddressList.FirstOrDefault(h => h.AddressFamily.Equals(AddressFamily.InterNetwork)) ?? throw new IpResolutionException("DNS resolution falied: Unable to resolve teamserver's IPv4 address.");
            this._ipv6 = hostEntry.AddressList.FirstOrDefault(h => h.AddressFamily.Equals(AddressFamily.InterNetwork)) ?? throw new IpResolutionException("DNS resolution falied: Unable to resolve teamserver's IPv6 address.");
            this._logger = logger;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Retrieves the server's current operational status for health checks.
        /// </summary>
        /// <remarks>Exposed at GET /status. Suitable for simple liveness and readiness checks.</remarks>
        /// <returns>An IActionResult that produces an HTTP 200 (OK) response.</returns>
        [HttpGet("status")]
        public IActionResult GetServerStatus()
        {
            return Ok();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Gets the server's IPv4 and IPv6 addresses as a TeamServerIpResponse.
        /// </summary>
        /// <returns>An IActionResult that returns 200 OK with a TeamServerIpResponse containing the IPv4 and IPv6 address
        /// strings.</returns>
        [HttpGet("teamserverip")]
        public IActionResult GetServerIP()
        {
            TeamServerIpResponse response = new TeamServerIpResponse(_ipv4.ToString(), _ipv6.ToString());
            return Ok(response);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//