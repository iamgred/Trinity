// =============================================================== { START OF FILE } =============================================================== //
using TeamServer.Repositories;
using Trinity.Shared.Errors;
using Trinity.Shared.Models;
using Trinity.Shared.Results;

namespace TeamServer.Services
{
    /// <summary>
    /// Provides services for managing Admin entities in the database, including CRUD operations.
    /// </summary>
    public class AdminService
    {
        private readonly AdminRepository _db;

        public AdminService(AdminRepository db)
        {
            _db = db;
        }
        public async Task<Result<Admin>> GetAdminByIdAsync(int adminID)
        {
            var admin = await _db.GetAdminByIdAsync(adminID);

            return admin == null ? AdminError.NotFound(adminID) : admin;
        }
        public async Task<Result<List<Admin>>> GetAdminsAsync()
        {
            return await _db.GetAdminsAsync();
        }
        public async Task<Result> CreateAdminAsync(Admin admin)
        {
            await _db.InsertAdminAsync(admin);
            return Result.Success();
        }
        public async Task<Result> UpdateAdminAsync(Admin admin)
        {
            bool updated = await _db.UpdateAdminAsync(admin);
            return updated ? Result.Success() : AdminError.NotFound(admin.ID);
        }
        public async Task<Result> DeleteAdminAsync(int adminID)
        {
            bool deleted = await _db.DeleteAdminAsync(adminID);
            return deleted ? Result.Success() : AdminError.NotFound(adminID);
        }
    }
}
// ===================================================================== { END OF FILE } ===================================================================== //