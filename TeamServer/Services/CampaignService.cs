//========================================================= {START OF FILE} =========================================================//
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
        private readonly DatabaseService _db;

        public CampaignService(DatabaseService db)
        {
            _db = db;
        }

        public async Task<List<Campaign>> GetCampaignsAsync()
        {
            return await _db.GetCampaignsAsync();
        }

        public async Task<Campaign?> GetCampaignByIdAsync(int campaignID)
        {
            return await _db.GetCampaignByIdAsync(campaignID);
        }

        public async Task<int> CreateCampaignAsync(Campaign newCampaign)
        {
            return await _db.InsertCampaignAsync(newCampaign);
        }

        public async Task<bool> UpdateCampaignAsync(Campaign updatedCampaign)
        {
            return await _db.UpdateCampaignAsync(updatedCampaign);
        }

        public async Task<bool> DeleteCampaignAsync(int campaignID)
        {
            return await _db.DeleteCampaignAsync(campaignID);
        }

        public async Task<List<Operator>> GetOperatorsByCampaignIdAsync(int campaignID)
        {
            return await _db.GetOperatorsByCampaignIdAsync(campaignID);
        }
        public async Task<bool> AssignOperatorToCampaignAsync(int campaignID, int operatorID)
        {
            return await _db.AssignOperatorToCampaignAsync(campaignID, operatorID);
        }
        public async Task<bool> RemoveOperatorFromCampaignAsync(int campaignID, int operatorID)
        {
            return await _db.RemoveOperatorFromCampaignAsync(campaignID, operatorID);
        }
    }
}
//========================================================= {END OF FILE} =========================================================//