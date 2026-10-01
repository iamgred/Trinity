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
            return Ok(result);
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
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
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
            return CreatedAtAction(nameof(GetCampaignByIdAsync), new { campaignID = result }, result);
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
            if (!result)
            {
                return NotFound();
            }
            return Ok(result);
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
            if (!result)
            {
                return NotFound();
            }
            return Ok(result);
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
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        /// <summary>
        /// Assign an operator to a specific campaign by ID
        /// </summary>
        /// <param name="campaignID"></param>
        /// <param name="operatorID"></param>
        /// <returns></returns>
        [HttpPost("{campaignID}/operators/{operatorID}")]
        public async Task<IActionResult> AssignOperatorToCampaignAsync(int campaignID, int operatorID)
        {
            var result = await _campaignService.AssignOperatorToCampaignAsync(campaignID, operatorID);
            if (!result)
            {
                return NotFound();
            }
            return Ok(result);
        }
        /// <summary>
        /// Unassign an operator from a specific campaign by ID
        /// </summary>
        /// <param name="campaignID"></param>
        /// <param name="operatorID"></param>
        /// <returns></returns>
        [HttpDelete("{campaignID}/operators/{operatorID}")]
        public async Task<IActionResult> UnassignOperatorFromCampaignAsync(int campaignID, int operatorID)
        {
            var result = await _campaignService.RemoveOperatorFromCampaignAsync(campaignID, operatorID);
            if (!result)
            {
                return NotFound();
            }
            return Ok(result);
        }
    }
}
// ==================================================================== { END OF FILE } ===================================================================== //