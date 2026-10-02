//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Buffers;
using System.Text.Json;
using TeamServer.Services;
using Trinity.Shared.DTOs.Checkin;

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
            var result = await _agentService.Checkin(blob);
            return result.IsSuccess ? Ok(result.Response) : BadRequest();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("base64/initial/checkin")]
        public IActionResult GetIntialBase64([FromBody] IntialCheckInRequest request, [FromQuery] string uuid)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            Span<byte> uuidSpan = bufferWriter.GetSpan(uuid.Length);
            int bytesWritten = System.Text.Encoding.UTF8.GetBytes(uuid, uuidSpan);
            bufferWriter.Advance(bytesWritten);

            using (var jsonWriter = new Utf8JsonWriter(bufferWriter))
            {
                JsonSerializer.Serialize(jsonWriter, request);
            }

            return Ok(Convert.ToBase64String(bufferWriter.WrittenSpan));
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("base64/full/checkin")]
        public IActionResult GetFullBase64([FromBody] ResultRequest request, [FromQuery] string uuid)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            Span<byte> uuidSpan = bufferWriter.GetSpan(uuid.Length);
            int bytesWritten = System.Text.Encoding.UTF8.GetBytes(uuid, uuidSpan);
            bufferWriter.Advance(bytesWritten);

            using (var jsonWriter = new Utf8JsonWriter(bufferWriter))
            {
                JsonSerializer.Serialize(jsonWriter, request);
            }

            return Ok(Convert.ToBase64String(bufferWriter.WrittenSpan));
        }

    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//