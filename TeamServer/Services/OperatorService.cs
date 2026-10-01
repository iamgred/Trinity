//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
using TeamServer.Repositories;
using Trinity.Shared.Models;

namespace TeamServer.Services
{
    /**
     * Service class for managing Operator entities in the database.
     * Provides methods to perform CRUD operations on Operator records.
     */
    public class OperatorService
    {
        private readonly OperatorRepository _db;

        public OperatorService(OperatorRepository db)
        {
            _db = db;
        }
        public async Task<List<Operator>> GetOperatorsAsync()
        {
            return await _db.GetOperatorsAsync();
        }
        public async Task<Operator?> GetOperatorByIdAsync(int operatorID)
        {
            return await _db.GetOperatorByIdAsync(operatorID);
        }
        public async Task<int> CreateOperatorAsync(Operator newOperator)
        {
            return await _db.InsertOperatorAsync(newOperator);
        }
        public async Task<bool> UpdateOperatorAsync(Operator updatedOperator)
        {
            return await _db.UpdateOperatorAsync(updatedOperator);
        }
        public async Task<bool> DeleteOperatorAsync(int operatorID)
        {
            return await _db.DeleteOperatorAsync(operatorID);
        }
    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //