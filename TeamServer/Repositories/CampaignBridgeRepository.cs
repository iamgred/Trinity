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
        /// <summary>
        /// Gets all Operators associated with a specific Campaign ID.
        /// </summary>
        /// <param name="campaignID"></param>
        /// <returns>A list of Operators associated with the specified Campaign ID.</returns>
        public async Task<List<Operator>> GetOperatorsByCampaignIdAsync(int campaignID)
        {
            List<Operator> result = await _context.CampaignBridges
                .Where(c => c.CampaignID.Equals(campaignID))
                .Join(_context.Operators, c => c.OperatorID, o => o.ID, (c, o) => o)
                .ToListAsync();
            return result;
        }
        /// <summary>
        /// Assigns an Operator to a Campaign by creating a new CampaignBridge entry. If the Operator or Campaign does not exist, or if the assignment already exists, it returns false; otherwise, it creates the assignment and returns true.
        /// </summary>
        /// <param name="operatorID"></param>
        /// <param name="campaignID"></param>
        /// <returns> Returns true if the assignment was successful, false if the Operator or Campaign does not exist or if the assignment already exists.</returns>
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

            return true;
        }
        /// <summary>
        /// Removes an Operator from a Campaign by deleting the corresponding CampaignBridge entry. If the assignment does not exist, it returns false; otherwise, it deletes the assignment and returns true.
        /// </summary>
        /// <param name="operatorID"></param>
        /// <param name="campaignID"></param>
        /// <returns>True if the removal was successful, false if the assignment does not exist.</returns>
        public async Task<bool> RemoveOperatorFromCampaignAsync(int operatorID, int campaignID)
        {
            CampaignBridge? campaignBridge = await _context.CampaignBridges.FirstOrDefaultAsync(cb => cb.OperatorID.Equals(operatorID) && cb.CampaignID.Equals(campaignID));
            if (campaignBridge == null)
            {
                return false; // Assignment does not exist
            }

            _context.CampaignBridges.Remove(campaignBridge);
            return true;
        }
    }
}
// ================================================================== { END OF FILE } =================================================================== //