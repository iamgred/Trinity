// ================================================================== { START OF FILE } =================================================================== //
using Microsoft.EntityFrameworkCore;
using TeamServer.Data;
using Trinity.Shared.Models;
using Task = System.Threading.Tasks.Task;

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
        /// <summary>
        /// Inserts a new Admin into the database.
        /// </summary>
        /// <param name="adminEntity"></param>
        /// <returns></returns>
        public async Task InsertAdminAsync(Admin adminEntity)
        {
            await _context.Admins.AddAsync(adminEntity);
        }
        /// <summary>
        /// Updates an existing Admin in the database. If the Admin does not exist, it returns false; otherwise, it updates the Admin and returns true.
        /// </summary>
        /// <param name="adminEntity"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAdminAsync(Admin adminEntity)
        {
            Admin? existingAdmin = await _context.Admins.FirstOrDefaultAsync(a => a.ID.Equals(adminEntity.ID));

            if (existingAdmin == null)
            {
                return false; // Admin not found
            }

            _context.Entry(existingAdmin).CurrentValues.SetValues(adminEntity);

            return true; // Update successful
        }
        /// <summary>
        /// Deletes an Admin from the database by their ID. If the Admin does not exist, it returns false; otherwise, it deletes the Admin and returns true.
        /// </summary>
        /// <param name="adminID"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAdminAsync(int adminID)
        {
            Admin? admin = await _context.Admins.FirstOrDefaultAsync(a => a.ID.Equals(adminID));
            if (admin == null)
            {
                return false; // Admin not found
            }

            _context.Admins.Remove(admin);
            return true; // Delete successful
        }
    }
}
// ================================================================== { END OF FILE } =================================================================== //