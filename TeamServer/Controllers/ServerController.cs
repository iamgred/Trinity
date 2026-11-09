//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.Mvc;
using Trinity.Shared.Configurations;
using Trinity.Shared.DTOs.TeamServer;

namespace TeamServer.Controllers
{
    [Route(Routes.Server)]
    [ApiController]
    public class ServerController : ControllerBase
    {
        private readonly string _ipV4;
        private readonly string _ipV6;
        private ILogger<ServerController> _logger;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="settings"></param>
        public ServerController(ILogger<ServerController> logger, ServerNetworkSettings settings)
        {
            _logger = logger;
            _ipV4 = settings.GetIpV4Address();
            _ipV6 = settings.GetIpV6Address();
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
            TeamServerIpResponse response = new TeamServerIpResponse(_ipV4, _ipV6);
            return Ok(response);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//