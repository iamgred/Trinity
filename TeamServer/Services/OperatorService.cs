//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
using TeamServer.Repositories;
using Trinity.Shared.Errors;
using Trinity.Shared.Models;
using Trinity.Shared.Results;

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
        public async Task<Result<List<Operator>>> GetOperatorsAsync()
        {
            return await _db.GetOperatorsAsync();
        }
        public async Task<Result<Operator>> GetOperatorByIdAsync(int operatorID)
        {
            var operatorEntity = await _db.GetOperatorByIdAsync(operatorID);

            return operatorEntity == null
                ? OperatorError.NotFound(operatorID)
                : operatorEntity;
        }
        public async Task<Result> CreateOperatorAsync(Operator newOperator)
        {
            await _db.InsertOperatorAsync(newOperator);
            return Result.Success();
        }
        public async Task<Result> UpdateOperatorAsync(Operator updatedOperator)
        {
            bool isUpdated = await _db.UpdateOperatorAsync(updatedOperator);
            return isUpdated ? Result.Success() : OperatorError.NotFound(updatedOperator.ID);
        }
        public async Task<Result> DeleteOperatorAsync(int operatorID)
        {
            bool isDeleted = await _db.DeleteOperatorAsync(operatorID);
            return isDeleted ? Result.Success() : OperatorError.NotFound(operatorID);
        }
    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //