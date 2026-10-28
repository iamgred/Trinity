using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using TeamServer.Services;
using Trinity.Shared.DTOs.Payload;

namespace TeamServer.Controllers
{
    /// <summary>
    /// Controller responsible for handling payload-related operations, including generating new payloads, retrieving existing payloads, and fetching specific payload files based on their IDs.
    /// https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.physicalfile?view=aspnetcore-10.0
    /// </summary>
    [Route(Routes.Payloads)]
    [ApiController]
    public class PayloadController : ControllerBase
    {
        private PayloadService _payloadService;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public PayloadController(PayloadService payloadService)
        {
            this._payloadService = payloadService;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("generate")]
        public async Task<IActionResult> GeneratePayloadAsync([FromBody] PayloadCreationDTO payloadCreationDTO)
        {
            var result = await _payloadService.GeneratePayloadAsync(payloadCreationDTO);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpGet]
        public async Task<IActionResult> GetPayloadsAsync()
        {
            var result = await _payloadService.GetPayloadsAsync();

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result.Response);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Retrieves the file path of a specific payload based on its ID. It fetches the payload from the database and constructs the full file path using the payload's file name and type.
        /// </summary>
        /// <param name="payloadID"></param>
        /// <returns>The file path of the payload, or an error result if the payload is not found.</returns>
        [HttpGet("{payloadID}")]
        public async Task<IActionResult> GetPayloadAsync(int payloadID)
        {
            var result = await _payloadService.GetPayloadFilePathAsync(payloadID);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            string filePath = result.Response!;

            string contentType = "application/octet-stream";

            return PhysicalFile(filePath, contentType, Path.GetFileName(filePath));
        }
    }
}
