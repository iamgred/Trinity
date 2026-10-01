//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TeamServer.Services;

namespace TeamServer.Controllers
{
    [Route(Routes.Agents)]
    [ApiController]
    public class AgentController : ControllerBase
    {
        private AgentService _agentService;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initializes a new instance of the AgentController class with the specified AgentService.
        /// </summary>
        /// <param name="agentService">The service that provides agent-related operations.</param>
        public AgentController(AgentService agentService)
        {
            this._agentService = agentService;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Retrieves all agents and returns an HTTP response containing the agents on success or a bad request on
        /// failure.
        /// </summary>
        /// <remarks>Performs an asynchronous call to the agent service to obtain the agents.</remarks>
        /// <returns>An OkObjectResult containing the agents when the operation succeeds; otherwise a BadRequestResult.</returns>
        [HttpGet()]
        public async Task<IActionResult> GetAgents()
        {
            var result = await _agentService.GetAgentsAsync();
            return result.IsSuccess ? Ok(result.Response) : BadRequest();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpGet("{agentID}")]
        public async Task<IActionResult> GetAgentDashboard([FromRoute] int agentID)
        {
            //var response = await _agentService.GetAgentDashboard(agentID);
            return Ok();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("checkin")]
        public async Task<IActionResult> Checkin([FromBody] string blob)
        {
             _agentService.Checkin(blob);
            return Ok();
        }

    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//