// ================================================================== { START OF FILE } =================================================================== //
using Microsoft.EntityFrameworkCore;
using TeamServer.Data;
using Trinity.Shared.Models;

namespace TeamServer.Repositories
{
    public class CampaignBridgeRepository : RepositoryBase<CampaignBridge>
    {
        public CampaignBridgeRepository(Context context) : base(context)
        {
        }

        /**
            * Retrieves all Operators associated with a specific Campaign ID.
            *
            * @param campaignID The ID of the Campaign to retrieve Operators for.
            * @return A list of Operators associated with the specified Campaign ID.
            */
        public async Task<List<Operator>> GetOperatorsByCampaignIdAsync(int campaignID)
        {
            List<Operator> result = await _context.CampaignBridges
                .Where(c => c.CampaignID.Equals(campaignID))
                .Join(_context.Operators, c => c.OperatorID, o => o.ID, (c, o) => o)
                .ToListAsync();
            return result;
        }
        /**
            * Assigns an Operator to a Campaign by creating a new CampaignBridge entry.
            *
            * @param operatorID The ID of the Operator to assign.
            * @param campaignID The ID of the Campaign to assign the Operator to.
            * @return True if the assignment was successful, false if the Operator or Campaign does not exist or if the assignment already exists.
            */
        public async Task<bool> AssignOperatorToCampaignAsync(int operatorID, int campaignID)
        {
            // Check if the operator exists
            bool operatorExists = await _context.Operators.AnyAsync(o => o.ID.Equals(operatorID));
            if (!operatorExists)
            {
                return false; // Operator does not exist
            }

            // Check if the campaign exists
            bool campaignExists = await _context.Campaigns.AnyAsync(c => c.ID.Equals(campaignID));
            if (!campaignExists)
            {
                return false; // Campaign does not exist
            }

            bool assignmentExists = await _context.CampaignBridges.AnyAsync(cb => cb.OperatorID.Equals(operatorID) && cb.CampaignID.Equals(campaignID));
            if (assignmentExists)
            {
                return false; // Assignment already exists
            }

            // Create a new CampaignBridge entry
            CampaignBridge campaignBridge = new CampaignBridge
            {
                OperatorID = operatorID,
                CampaignID = campaignID,
                AssignedDate = DateTime.UtcNow
            };

            await _context.CampaignBridges.AddAsync(campaignBridge);
            await _context.SaveChangesAsync();

            return true;
        }
        /**
            * Removes an Operator from a Campaign by deleting the corresponding CampaignBridge entry.
            *
            * @param operatorID The ID of the Operator to remove.
            * @param campaignID The ID of the Campaign to remove the Operator from.
            * @return True if the removal was successful, false if the assignment does not exist.
            */
        public async Task<bool> RemoveOperatorFromCampaignAsync(int operatorID, int campaignID)
        {
            int result = await _context.CampaignBridges
                .Where(cb => cb.OperatorID.Equals(operatorID) && cb.CampaignID.Equals(campaignID))
                .ExecuteDeleteAsync();
            return result > 0;
        }
    }
}
// ================================================================== { END OF FILE } =================================================================== //