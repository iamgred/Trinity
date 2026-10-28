// ============================================== { START OF FILE} ============================================== //
using Trinity.Shared.Models;
using Trinity.Shared.Results;

namespace TeamServer.Interface
{
    public interface IAdminService
    {
        Task<Result<List<Admin>>> GetAdminsAsync();
        Task<Result<Admin>> GetAdminByIdAsync(int adminID);
        Task<Result> CreateAdminAsync(Admin admin);
        Task<Result> UpdateAdminAsync(Admin admin);
        Task<Result> DeleteAdminAsync(int adminID);
    }
}
// ============================================== { END OF FILE} ============================================== //