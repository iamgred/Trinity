// =============================================================== { START OF FILE } =============================================================== //
using TeamServer.Repositories;
using Trinity.Shared.Models;

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
        public async Task<Admin?> GetAdminByIdAsync(int adminID)
        {
            return await _db.GetAdminByIdAsync(adminID);
        }
        public async Task<List<Admin>> GetAdminsAsync()
        {
            return await _db.GetAdminsAsync();
        }
        public async Task<int> CreateAdminAsync(Admin admin)
        {
            return await _db.InsertAdminAsync(admin);
        }
        public async Task<bool> UpdateAdminAsync(Admin admin)
        {
            return await _db.UpdateAdminAsync(admin);
        }
        public async Task<bool> DeleteAdminAsync(int adminID)
        {
            return await _db.DeleteAdminAsync(adminID);
        }
    }
}
// ===================================================================== { END OF FILE } ===================================================================== //