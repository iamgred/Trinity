// ===================================================== { START OF FILE } ===================================================== //
using Microsoft.AspNetCore.Mvc;
using TeamServer.Services;
using Trinity.Shared.Models;

namespace TeamServer.Controllers
{
    [ApiController]
    [Route(Routes.Admins)]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;

        public AdminController(AdminService adminService)
        {
            _adminService = adminService;
        }
        /// <summary>
        /// Retrieves a list of all Admin entities from the database.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAdminsAsync()
        {
            var result = await _adminService.GetAdminsAsync();
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }
            return Ok(result.Response);
        }
        /// <summary>
        /// Retrieves a specific Admin entity by its ID from the database.
        /// </summary>
        /// <param name="adminID"></param>
        /// <returns></returns>
        [HttpGet("{adminID}")]
        public async Task<IActionResult> GetAdminByIdAsync(int adminID)
        {
            var result = await _adminService.GetAdminByIdAsync(adminID);
            if (!result.IsSuccess)
            {
                return NotFound();
            }
            return Ok(result.Response);
        }
        /// <summary>
        /// Creates a new Admin entity in the database.
        /// </summary>
        /// <param name="admin"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateAdminAsync([FromBody] Admin admin)
        {
            var result = await _adminService.CreateAdminAsync(admin);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }
            return Ok();
        }
        /// <summary>
        /// Updates an existing Admin entity in the database by its ID.
        /// </summary>
        /// <param name="adminID"></param>
        /// <param name="admin"></param>
        /// <returns></returns>
        [HttpPut("{adminID}")]
        public async Task<IActionResult> UpdateAdminAsync(int adminID, [FromBody] Admin admin)
        {
            admin.ID = adminID; // Ensure the ID is set correctly
            var result = await _adminService.UpdateAdminAsync(admin);
            if (!result.IsSuccess)
            {
                return NotFound(result.Error);
            }
            return Ok();
        }
        /// <summary>
        /// Deletes an existing Admin entity from the database by its ID.
        /// </summary>
        /// <param name="adminID"></param>
        /// <returns></returns>
        [HttpDelete("{adminID}")]
        public async Task<IActionResult> DeleteAdminAsync(int adminID)
        {
            var result = await _adminService.DeleteAdminAsync(adminID);
            if (!result.IsSuccess)
            {
                return NotFound(result.Error);
            }
            return Ok();
        }
    }
}
// ===================================================== { END OF FILE } ===================================================== //