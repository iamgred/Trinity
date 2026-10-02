// ==================================================================== { START OF FILE } ===================================================================== //
using Microsoft.AspNetCore.Mvc;
using TeamServer.Services;
using Trinity.Shared.Models;

namespace TeamServer.Controllers
{
    [ApiController]
    [Route(Routes.Campaigns)]
    public class CampaignController : ControllerBase
    {
        private readonly CampaignService _campaignService;

        public CampaignController(CampaignService campaignService)
        {
            _campaignService = campaignService;
        }

        /// <summary>
        /// Get all campaigns
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetCampaignsAsync()
        {
            var result = await _campaignService.GetCampaignsAsync();

            if (!result.IsSuccess)
            {
                return NotFound(result.Error);
            }
            return Ok(result.Response);
        }
        /// <summary>
        /// Get a campaign by ID
        /// </summary>
        /// <param name="campaignID"></param>
        /// <returns></returns>
        [HttpGet("{campaignID}")]
        public async Task<IActionResult> GetCampaignByIdAsync(int campaignID)
        {
            var result = await _campaignService.GetCampaignByIdAsync(campaignID);
            if (!result.IsSuccess)
            {
                return NotFound(result.Error);
            }
            return Ok(result.Response);
        }
        /// <summary>
        /// Create a new campaign
        /// </summary>
        /// <param name="campaign"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateCampaignAsync([FromBody] Campaign campaign)
        {
            var result = await _campaignService.CreateCampaignAsync(campaign);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }
            return Ok();
        }
        /// <summary>
        /// Update an existing campaign
        /// </summary>
        /// <param name="campaignID"></param>
        /// <param name="campaign"></param>
        /// <returns></returns>
        [HttpPut("{campaignID}")]
        public async Task<IActionResult> UpdateCampaignAsync(int campaignID, [FromBody] Campaign campaign)
        {
            campaign.ID = campaignID; // Ensure the campaign ID is set correctly
            var result = await _campaignService.UpdateCampaignAsync(campaign);
            if (!result.IsSuccess)
            {
                return NotFound(result.Error);
            }
            return Ok();
        }
        /// <summary>
        /// Delete a campaign by ID
        /// </summary>
        /// <param name="campaignID"></param>
        /// <returns></returns>
        [HttpDelete("{campaignID}")]
        public async Task<IActionResult> DeleteCampaignAsync(int campaignID)
        {
            var result = await _campaignService.DeleteCampaignAsync(campaignID);
            if (!result.IsSuccess)
            {
                return NotFound(result.Error);
            }
            return Ok();
        }
        /// <summary>
        /// Get operators associated with a specific campaign by ID
        /// </summary>
        /// <param name="campaignID"></param>
        /// <returns></returns>
        [HttpGet("{campaignID}/operators")]
        public async Task<IActionResult> GetOperatorsByCampaignIdAsync(int campaignID)
        {
            var result = await _campaignService.GetOperatorsByCampaignIdAsync(campaignID);
            if (!result.IsSuccess)
            {
                return NotFound(result.Error);
            }
            return Ok(result.Response);
        }
        /// <summary>
        /// Assign an operator to a specific campaign by ID
        /// </summary>
        /// <param name="campaignID"></param>
        /// <param name="operatorID"></param>
        /// <returns></returns>
        [HttpPost("{campaignID}/operators/{operatorID}")]
        public async Task<IActionResult> AssignOperatorToCampaignAsync(int operatorID, int campaignID)
        {
            var result = await _campaignService.AssignOperatorToCampaignAsync(operatorID, campaignID);
            if (!result.IsSuccess)
            {
                return NotFound(result.Error);
            }
            return Ok();
        }
        /// <summary>
        /// Unassign an operator from a specific campaign by ID
        /// </summary>
        /// <param name="campaignID"></param>
        /// <param name="operatorID"></param>
        /// <returns></returns>
        [HttpDelete("{campaignID}/operators/{operatorID}")]
        public async Task<IActionResult> UnassignOperatorFromCampaignAsync(int operatorID, int campaignID)
        {
            var result = await _campaignService.RemoveOperatorFromCampaignAsync(operatorID, campaignID);
            if (!result.IsSuccess)
            {
                return NotFound(result.Error);
            }
            return Ok();
        }
    }
}
// ==================================================================== { END OF FILE } ===================================================================== //