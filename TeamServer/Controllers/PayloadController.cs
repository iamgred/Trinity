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
    }
}
