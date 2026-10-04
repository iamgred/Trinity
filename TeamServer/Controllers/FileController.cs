// ============================================== { START OF FILE } ============================================== //
using Microsoft.AspNetCore.Mvc;
using TeamServer.Interface;
using TeamServer.Services;

namespace TeamServer.Controllers
{
    [ApiController]
    [Route(Routes.Files)]
    public class FilesController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;
        public FilesController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        /// <summary>
        /// Uploads a file to the uploads directory.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFileAsync([FromBody] IFormFile file)
        {
            var result = await _fileStorageService.SaveFileAsync(file);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Response);
        }

        /// <summary>
        /// Downloads a file from the uploads directory.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFileAsync(string fileName)
        {
            var result = await _fileStorageService.GetFileAsync(fileName);

            if (!result.IsSuccess)
            {
                return NotFound(result.Error);
            }
            return File(result.Response!, "application/octet-stream", fileName);
        }

        /// <summary>
        /// Lists all files in the uploads directory.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ListFiles()
        {
            var result = _fileStorageService.ListFiles();

            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Response);
        }
    }
}
// ============================================== { END OF FILE } ============================================== //