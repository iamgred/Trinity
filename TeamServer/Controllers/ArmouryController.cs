//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.Mvc;
using TeamServer.Services;
using Trinity.Shared.DTOs.Tool;

namespace TeamServer.Controllers
{
    [Route(Routes.Armoury)]
    [ApiController]
    public class ArmouryController : ControllerBase
    {
        private readonly ToolService _toolService;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="toolService"></param>
        public ArmouryController(ToolService toolService)
        {
            _toolService = toolService;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpGet("tools")]
        public async Task<IActionResult> GetToolsAsync([FromQuery] int pagination, [FromQuery] int rows)
        {
            var result = await _toolService.GetToolsAsync(pagination, rows);
            return result.IsSuccess ? Ok(result.Response) : BadRequest(result.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("upload")]
        public async Task<IActionResult> UploadToolAsync([FromForm] ToolCreationRequest request)
        {
            var result = await _toolService.UploadAsync(request);
            return result.IsSuccess ? CreatedAtAction(nameof(UploadToolAsync), result) : BadRequest(result.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpDelete("remove/{toolID}")]
        public async Task<IActionResult> RemoveToolAsync([FromRoute] int toolID)
        {
            var result = await _toolService.RemoveToolAsync(toolID);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//