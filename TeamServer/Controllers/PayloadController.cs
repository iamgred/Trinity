using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using TeamServer.Services;
using Trinity.Shared.DTOs.Payload;

namespace TeamServer.Controllers
{
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

            if (String.IsNullOrEmpty(result.Error))
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

            return Ok(result);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpGet("download")]
        public IActionResult GetPayloadFile([FromQuery] int payloadID)
        {
            string fileContent = "This is a dummy file...";
            byte[] fileBytes = Encoding.UTF8.GetBytes(fileContent);
            string contentType = "text/plain";
            string downloadName = "dummypayload";

            return File(fileBytes, contentType, downloadName);
        }
    }
}
