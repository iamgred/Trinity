// ========================================== { START OF FILE } ========================================== //
using Microsoft.EntityFrameworkCore;
using TeamServer.Data;
using Trinity.Shared.Models;
using Task = System.Threading.Tasks.Task;

namespace TeamServer.Repositories
{
    public class OperatorRepository : RepositoryBase<Operator>
    {
        public OperatorRepository(Context context) : base(context)
        {
        }

        /// <summary>
        /// Retrieves an Operator by their ID.
        /// </summary>
        /// <param name="operatorID"></param>
        /// <returns>Operator with the specified ID, or null if not found.</returns>
        public async Task<Operator?> GetOperatorByIdAsync(int operatorID)
        {
            Operator? result = await _context.Operators.FirstOrDefaultAsync(o => o.ID.Equals(operatorID));
            return result;
        }
        /// <summary>
        /// Retrieves all Operators from the database.
        /// </summary>
        /// <returns>A list of all Operators.</returns>
        public async Task<List<Operator>> GetOperatorsAsync()
        {
            List<Operator> result = await _context.Operators.ToListAsync();
            return result;
        }
        /// <summary>
        /// Inserts a new Operator into the database.
        /// </summary>
        /// <param name="operatorEntity"></param>
        /// <returns>Represents the success or failure of the operation.</returns>
        public async Task InsertOperatorAsync(Operator operatorEntity)
        {
            await _context.Operators.AddAsync(operatorEntity);
        }
        /// <summary>
        /// Updates an existing Operator in the database. If the Operator does not exist, it returns false; otherwise, it updates the Operator and returns true.
        /// </summary>
        /// <param name="operatorEntity"></param>
        /// <returns>True if the update was successful, false if the Operator was not found.</returns>
        public async Task<bool> UpdateOperatorAsync(Operator operatorEntity)
        {
            Operator? existingOperator = await _context.Operators.FirstOrDefaultAsync(o => o.ID.Equals(operatorEntity.ID));

            if (existingOperator == null)
            {
                return false; // Operator not found
            }

            _context.Entry(existingOperator).CurrentValues.SetValues(operatorEntity);
            return true; // Update successful
        }
        /// <summary>
        /// Deletes an Operator from the database by their ID. If the Operator does not exist, it returns false; otherwise, it deletes the Operator and returns true.
        /// </summary>
        /// <param name="operatorID"></param>
        /// <returns>True if the deletion was successful, false if the Operator was not found.</returns>
        public async Task<bool> DeleteOperatorAsync(int operatorID)
        {
            Operator? result = await _context.Operators.FirstOrDefaultAsync(o => o.ID.Equals(operatorID));
            if (result == null)
            {
                return false; // Operator not found
            }
            _context.Operators.Remove(result);
            return true; // Delete successful;
        }
    }
}
// ========================================== { END OF FILE } ========================================== //