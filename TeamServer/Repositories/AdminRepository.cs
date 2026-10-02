// ================================================================== { START OF FILE } =================================================================== //
using Microsoft.EntityFrameworkCore;
using TeamServer.Data;
using Trinity.Shared.Models;

namespace TeamServer.Repositories
{
    public class AdminRepository : RepositoryBase<Admin>
    {
        public AdminRepository(Context context) : base(context)
        {
        }

        /**
            * Retrieves an Admin by their ID.
            *
            * @param adminID The ID of the Admin to retrieve.
            * @return The Admin with the specified ID, or null if not found.
            */
        public async Task<Admin?> GetAdminByIdAsync(int adminID)
        {
            Admin? result = await _context.Admins.FirstOrDefaultAsync(a => a.ID.Equals(adminID));
            return result;
        }
        /**
            * Retrieves all Admins from the database.
            *
            * @return A list of all Admins.
            */
        public async Task<List<Admin>> GetAdminsAsync()
        {
            List<Admin> result = await _context.Admins.ToListAsync();
            return result;
        }
        /**
            * Inserts a new Admin into the database.
            *
            * @param adminEntity The Admin entity to insert.
            * @return The ID of the newly inserted Admin.
            */
        public async Task<int> InsertAdminAsync(Admin adminEntity)
        {
            var result = await _context.Admins.AddAsync(adminEntity);
            await _context.SaveChangesAsync();
            return result.Entity.ID;
        }
        /**
            * Updates an existing Admin in the database.
            *
            * @param adminEntity The Admin entity with updated information.
            * @return True if the update was successful, false if the Admin was not found.
            */
        public async Task<bool> UpdateAdminAsync(Admin adminEntity)
        {
            var existingAdmin = await _context.Admins.AnyAsync(a => a.ID.Equals(adminEntity.ID));

            if (!existingAdmin)
            {
                return false; // Admin not found
            }

            _context.Admins.Update(adminEntity);

            await _context.SaveChangesAsync();
            return true; // Update successful
        }
        /// <summary>
        /// Deletes an Admin from the database by their ID.
        /// </summary>
        /// <param name="adminID"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAdminAsync(int adminID)
        {
            int result = await _context.Admins
                .Where(a => a.ID.Equals(adminID))
                .ExecuteDeleteAsync();
            return result > 0;
        }
    }
}
// ================================================================== { END OF FILE } =================================================================== //