//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Trinity.Shared.Models;
using TeamServer.Data;
using Microsoft.EntityFrameworkCore;

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
        /**
            * Inserts a new Campaign into the database.
            *
            * @param campaign The Campaign entity to insert.
            * @return The ID of the newly inserted Campaign.
            */
        public async Task<int> InsertCampaignAsync(Campaign campaign)
        {
            var result = await _context.Campaigns.AddAsync(campaign);
            await _context.SaveChangesAsync();
            return result.Entity.ID;
        }
        /**
            * Updates an existing Campaign in the database.
            *
            * @param campaign The Campaign entity with updated information.
            * @return True if the update was successful, false if the Campaign was not found.
            */
        public async Task<bool> UpdateCampaignAsync(Campaign campaign)
        {
            var existingCampaign = await _context.Campaigns.AnyAsync(c => c.ID.Equals(campaign.ID));

            if (!existingCampaign)
            {
                return false; // Campaign not found
            }

            _context.Campaigns.Update(campaign);

            await _context.SaveChangesAsync();
            return true; // Update successful
        }
        /**
            * Deletes a Campaign from the database by its ID.
            *
            * @param campaignID The ID of the Campaign to delete.
            * @return True if the deletion was successful, false if the Campaign was not found.
            */
        public async Task<bool> DeleteCampaignAsync(int campaignID)
        {
            int result = await _context.Campaigns
                .Where(c => c.ID.Equals(campaignID))
                .ExecuteDeleteAsync();
            return result > 0;
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//