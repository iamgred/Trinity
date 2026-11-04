// ============================================== { START OF FILE } ============================================== //
using Microsoft.AspNetCore.Mvc;
using TeamServer.Interface;
using TeamServer.Services;
using Trinity.Shared.Enums;

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
        // [HttpPost("upload")]
        // public async Task<IActionResult> UploadFileAsync([FromBody] IFormFile file)
        // {
        //     var result = await _fileStorageService.SaveFileAsync(file);

        //     if (!result.IsSuccess)
        //     {
        //         return BadRequest(result.Error);
        //     }

        //     return Ok(result.Response);
        // }

        /// <summary>
        /// Downloads a file from the uploads or loot directory.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFileAsync(
            string fileName,
            [FromQuery] FileFolders folderName)
        {
            var result = await _fileStorageService.GetFileAsync(fileName, folderName);

            if (!result.IsSuccess)
            {
                return NotFound(result.Error);
            }

            return File(result.Response!, "application/octet-stream", fileName);
        }

        /// <summary>
        /// Lists all files in the uploads or loot directory.
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ListFiles([FromQuery] FileFolders folderName)
        {
            var result = _fileStorageService.ListFiles(folderName);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Response);
        }
        /// <summary>
        /// Uploads a file to the uploads directory.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            var result = await _fileStorageService.SaveFileAsync(file);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Response);
        }
    }
}
// ============================================== { END OF FILE } ============================================== //