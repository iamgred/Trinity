//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Trinity.Shared.Models;
using TeamServer.Data;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace TeamServer.Repositories
{
    public class CampaignRepository : RepositoryBase<Campaign>
    {
        public CampaignRepository(Context context) : base(context)
        {
        }
        public async Task<List<Campaign>> GetCampaignsAsync()
        {
            List<Campaign> result = await _context.Campaigns.ToListAsync();
            return result;
        }
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /**
            * Retrieves a Campaign by its ID.
            *
            * @param campaignID The ID of the Campaign to retrieve.
            * @return The Campaign with the specified ID, or null if not found.
            */
        public async Task<Campaign?> GetCampaignByIdAsync(int campaignID)
        {
            Campaign? result = await _context.Campaigns.FirstOrDefaultAsync(c => c.ID.Equals(campaignID));
            return result;
        }
        /// <summary>
        /// Inserts a new Campaign into the database.
        /// </summary>
        /// <param name="campaign"></param>
        /// <returns></returns>
        public async Task InsertCampaignAsync(Campaign campaign)
        {
            await _context.Campaigns.AddAsync(campaign);
        }
        /**
            * Updates an existing Campaign in the database.
            *
            * @param campaign The Campaign entity with updated information.
            * @return True if the update was successful, false if the Campaign was not found.
            */
        public async Task<bool> UpdateCampaignAsync(Campaign campaign)
        {
            Campaign? existingCampaign = await _context.Campaigns.FirstOrDefaultAsync(c => c.ID.Equals(campaign.ID));

            if (existingCampaign == null)
            {
                return false; // Campaign not found
            }

            _context.Entry(existingCampaign).CurrentValues.SetValues(campaign);

            return true; // Update successful
        }
        /// <summary>
        /// Deletes a Campaign from the database by its ID. If the Campaign does not exist, it returns false; otherwise, it deletes the Campaign and returns true.
        /// </summary>
        /// <param name="campaignID"></param>
        /// <returns>True if the deletion was successful, false if the Campaign was not found.</returns>
        public async Task<bool> DeleteCampaignAsync(int campaignID)
        {
            Campaign? campaign = await _context.Campaigns.FirstOrDefaultAsync(c => c.ID.Equals(campaignID));
            if (campaign == null)
            {
                return false; // Campaign not found
            }
            _context.Campaigns.Remove(campaign);
            return true; // Delete successful
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//