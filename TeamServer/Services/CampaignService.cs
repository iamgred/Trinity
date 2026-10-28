//========================================================= {START OF FILE} =========================================================//
using TeamServer.Repositories;
using Trinity.Shared.Errors;
using Trinity.Shared.Models;
using Trinity.Shared.Results;

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

        public async Task<Result<List<Campaign>>> GetCampaignsAsync()
        {
            return await _campaignRepository.GetCampaignsAsync();
        }

        public async Task<Result<Campaign>> GetCampaignByIdAsync(int campaignID)
        {
            var campaign = await _campaignRepository.GetCampaignByIdAsync(campaignID);
            return campaign == null
                ? CampaignError.NotFound(campaignID)
                : campaign;
        }

        public async Task<Result> CreateCampaignAsync(Campaign newCampaign)
        {
            await _campaignRepository.InsertCampaignAsync(newCampaign);
            await _campaignRepository.CommitAsync();
            return Result.Success();
        }

        public async Task<Result> UpdateCampaignAsync(Campaign updatedCampaign)
        {
            bool updateResult = await _campaignRepository.UpdateCampaignAsync(updatedCampaign);
            if (!updateResult)
            {
                return CampaignError.NotFound(updatedCampaign.ID);
            }
            await _campaignRepository.CommitAsync();
            return Result.Success();
        }

        public async Task<Result> DeleteCampaignAsync(int campaignID)
        {
            bool deleteResult = await _campaignRepository.DeleteCampaignAsync(campaignID);
            if (!deleteResult)
            {
                return CampaignError.NotFound(campaignID);
            }
            await _campaignRepository.CommitAsync();
            return Result.Success();
        }

        public async Task<Result<List<Operator>>> GetOperatorsByCampaignIdAsync(int campaignID)
        {
            var campaign = await _campaignRepository.GetCampaignByIdAsync(campaignID);

            if (campaign == null)
            {
                return CampaignError.NotFound(campaignID);
            }

            return await _campaignBridgeRepository.GetOperatorsByCampaignIdAsync(campaignID);
        }
        public async Task<Result> AssignOperatorToCampaignAsync(int operatorID, int campaignID)
        {
            bool assigned = await _campaignBridgeRepository.AssignOperatorToCampaignAsync(operatorID, campaignID);
            if (!assigned)
            {
                return CampaignError.NotFound(campaignID);
            }
            await _campaignBridgeRepository.CommitAsync();
            return Result.Success();
        }
        public async Task<Result> RemoveOperatorFromCampaignAsync(int operatorID, int campaignID)
        {
            bool removed = await _campaignBridgeRepository.RemoveOperatorFromCampaignAsync(operatorID, campaignID);
            if (!removed)
            {
                return CampaignError.NotFound(campaignID);
            }
            await _campaignBridgeRepository.CommitAsync();
            return Result.Success();
        }
    }
}
//========================================================= {END OF FILE} =========================================================//