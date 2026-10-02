// ========================================== { START OF FILE } ========================================== //
using Microsoft.EntityFrameworkCore;
using TeamServer.Data;
using Trinity.Shared.Models;

namespace TeamServer.Repositories
{
    public class OperatorRepository : RepositoryBase<Operator>
    {
        public OperatorRepository(Context context) : base(context)
        {
        }

        public async Task<Operator?> GetOperatorByIdAsync(int operatorID)
        {
            Operator? result = await _context.Operators.FirstOrDefaultAsync(o => o.ID.Equals(operatorID));
            return result;
        }
        /**
            * Retrieves all Operators from the database.
            *
            * @return A list of all Operators.
            */
        public async Task<List<Operator>> GetOperatorsAsync()
        {
            List<Operator> result = await _context.Operators.ToListAsync();
            return result;
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
        /**
            * Inserts a new Operator into the database.
            *
            * @param operatorEntity The Operator entity to insert.
            * @return The ID of the newly inserted Operator.
            */
        public async Task<int> InsertOperatorAsync(Operator operatorEntity)
        {
            var result = await _context.Operators.AddAsync(operatorEntity);
            await _context.SaveChangesAsync();
            return result.Entity.ID;
        }
        /**
            * Updates an existing Operator in the database.
            *
            * @param operatorEntity The Operator entity with updated information.
            * @return True if the update was successful, false if the Operator was not found.
            */
        public async Task<bool> UpdateOperatorAsync(Operator operatorEntity)
        {
            var existingOperator = await _context.Operators.AnyAsync(o => o.ID.Equals(operatorEntity.ID));

            if (!existingOperator)
            {
                return false; // Operator not found
            }

            _context.Operators.Update(operatorEntity);

            await _context.SaveChangesAsync();
            return true; // Update successful
        }
        /**
            * Deletes an Operator from the database by their ID.
            *
            * @param operatorID The ID of the Operator to delete.
            * @return True if the deletion was successful, false if the Operator was not found.
            */
        public async Task<bool> DeleteOperatorAsync(int operatorID)
        {
            int result = await _context.Operators
                .Where(o => o.ID.Equals(operatorID))
                .ExecuteDeleteAsync();
            return result > 0;
        }
    }
}
// ========================================== { END OF FILE } ========================================== //
