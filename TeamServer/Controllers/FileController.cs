// ============================================== { START OF FILE } ============================================== //
using System.Security.Claims;
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

        [HttpGet]
        public async Task<IActionResult> ListFilesAsync()
        {
            if (!TryGetCampaignId(out var campaignId))
                return Forbid();

            var files = await _files.ListFilesAsync(campaignId);
            return Ok(files);
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFileAsync(string fileName)
        {
            if (!TryGetCampaignId(out var campaignId))
                return Forbid();

            var stream = await _files.OpenFileAsync(campaignId, fileName);
            if (stream is null)
                return NotFound();

            return File(stream, "application/octet-stream", fileName);
        }

        private bool TryGetCampaignId(out int campaignId)
        {
            var raw = User.FindFirstValue("campaignId");
            return int.TryParse(raw, out campaignId);
        }
    }
}
// ============================================== { END OF FILE } ============================================== //