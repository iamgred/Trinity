//========================================================= {START OF FILE} =========================================================//
using TeamServer.Repositories;
using Trinity.Shared.Models;

namespace TeamServer.Services
{
    /**
     * Service class for managing Campaign entities in the database.
     * Provides methods to perform CRUD operations on Campaign records.
     * Also provides methods for managing the relationship between Campaigns and Operators.
     */
    public class CampaignService
    {
        private readonly CampaignBridgeRepository _campaignBridgeRepository;
        private readonly CampaignRepository _campaignRepository;

        public CampaignService(CampaignBridgeRepository campaignBridgeRepository, CampaignRepository campaignRepository)
        {
            _campaignBridgeRepository = campaignBridgeRepository;
            _campaignRepository = campaignRepository;
        }

        public async Task<List<Campaign>> GetCampaignsAsync()
        {
            return await _campaignRepository.GetCampaignsAsync();
        }

        public async Task<Campaign?> GetCampaignByIdAsync(int campaignID)
        {
            return await _campaignRepository.GetCampaignByIdAsync(campaignID);
        }

        public async Task<int> CreateCampaignAsync(Campaign newCampaign)
        {
            return await _campaignRepository.InsertCampaignAsync(newCampaign);
        }

        public async Task<bool> UpdateCampaignAsync(Campaign updatedCampaign)
        {
            return await _campaignRepository.UpdateCampaignAsync(updatedCampaign);
        }

        public async Task<bool> DeleteCampaignAsync(int campaignID)
        {
            return await _campaignRepository.DeleteCampaignAsync(campaignID);
        }

        public async Task<List<Operator>> GetOperatorsByCampaignIdAsync(int campaignID)
        {
            return await _campaignBridgeRepository.GetOperatorsByCampaignIdAsync(campaignID);
        }
        public async Task<bool> AssignOperatorToCampaignAsync(int operatorID, int campaignID)
        {
            return await _campaignBridgeRepository.AssignOperatorToCampaignAsync(operatorID, campaignID);
        }
        public async Task<bool> RemoveOperatorFromCampaignAsync(int operatorID, int campaignID)
        {
            return await _campaignBridgeRepository.RemoveOperatorFromCampaignAsync(operatorID, campaignID);
        }
    }
}
//========================================================= {END OF FILE} =========================================================//