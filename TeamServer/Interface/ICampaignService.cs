// ========================================================= { START OF FILE } ========================================================= //
using Trinity.Shared.Models;
using Trinity.Shared.Results;

namespace TeamServer.Interface
{
    public interface ICampaignService
    {
        Task<Result<List<Campaign>>> GetCampaignsAsync();

        Task<Result<Campaign>> GetCampaignByIdAsync(int campaignID);

        Task<Result> CreateCampaignAsync(Campaign newCampaign);

        Task<Result> UpdateCampaignAsync(Campaign updatedCampaign);

        Task<Result> DeleteCampaignAsync(int campaignID);

        Task<Result<List<Operator>>> GetOperatorsByCampaignIdAsync(int campaignID);

        Task<Result> AssignOperatorToCampaignAsync(int operatorID, int campaignID);

        Task<Result> RemoveOperatorFromCampaignAsync(int operatorID, int campaignID);
    }
}
// ========================================================= { END OF FILE } ========================================================= //