//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Refit;
using AdminModel = Trinity.Shared.Models.Admin;

namespace Trinity.Shared.Interfaces
{
    public interface IAdminAPI
    {
        [Get("/api/v1/admins")]
        Task<List<AdminModel>> GetAdminsAsync();

        [Get("/api/v1/admins/{id}")]
        Task<AdminModel> GetByIdAsync(int id);

        [Post("/api/v1/admins")]
        Task CreateAsync([Body] AdminModel admin);

        [Delete("/api/v1/admins/{id}")]
        Task DeleteAsync(int id);
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
