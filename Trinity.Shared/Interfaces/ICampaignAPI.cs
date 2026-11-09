//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Refit;
using CampaignModel = Trinity.Shared.Models.Campaign;

namespace Trinity.Shared.Interfaces
{
    public interface ICampaignAPI
    {
        [Get("/api/v1/campaigns")]
        Task<List<CampaignModel>> GetCampaignsAsync();

        [Get("/api/v1/campaigns/{id}")]
        Task<CampaignModel> GetByIdAsync(int id);

        [Post("/api/v1/campaigns")]
        Task CreateAsync([Body] object campaign);

        [Delete("/api/v1/campaigns/{id}")]
        Task DeleteAsync(int id);
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
