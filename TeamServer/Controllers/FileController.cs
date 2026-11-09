// ============================================== { START OF FILE } ============================================== //
using Microsoft.AspNetCore.Mvc;
using Trinity.Shared.Interfaces;

namespace TeamServer.Controllers
{
    [ApiController]
    [Route(Routes.Files)]
    public class FilesController : ControllerBase
    {
        private readonly IReceivedFileService _files;

        public FilesController(IReceivedFileService files)
        {
            _files = files;
        }

        [HttpGet("{campaignId:int}")]
        public async Task<IActionResult> ListFilesAsync(int campaignId)
        {
            var files = await _files.ListFilesAsync(campaignId);
            return Ok(files);
        }

        [HttpGet("{campaignId:int}/download/{fileName}")]
        public async Task<IActionResult> DownloadFileAsync(int campaignId, string fileName)
        {
            var stream = await _files.OpenFileAsync(campaignId, fileName);
            if (stream is null)
                return NotFound();

            return File(stream, "application/octet-stream", fileName);
        }
    }
}
// ============================================== { END OF FILE } ============================================== //