//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Trinity.Shared.Models;
using Trinity.Shared.Results;

namespace Trinity.Shared.Interfaces
{
    public interface IOperatorService
    {
        Task<Result<List<Operator>>> GetOperatorsAsync();
        Task<Result<Operator>> GetOperatorByIdAsync(int operatorID);
        Task<Result> CreateOperatorAsync(Operator newOperator);
        Task<Result> UpdateOperatorAsync(Operator updatedOperator);
        Task<Result> DeleteOperatorAsync(int operatorID);
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
