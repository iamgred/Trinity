//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Refit;
using OperatorModel = Trinity.Shared.Models.Operator;

namespace Trinity.Shared.Interfaces
{
    public interface IOperatorAPI
    {
        [Get("/api/v1/operators")]
        Task<List<OperatorModel>> GetOperatorsAsync();

        [Get("/api/v1/operators/{id}")]
        Task<OperatorModel> GetByIdAsync(int id);

        [Post("/api/v1/operators")]
        Task CreateAsync([Body] object op);

        [Delete("/api/v1/operators/{id}")]
        Task DeleteAsync(int id);
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
