//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
using TeamServer.Data;
using Trinity.Shared.Models;
namespace TeamServer.Services
{
    public class DatabaseService
    {
        private readonly Context _context;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initialises a new instance of the DatabaseService class.
        /// </summary>
        public DatabaseService(Context context)
        {
            this._context = context;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Inserts an Agent task.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public async Task<bool> InsertAgentTask(Trinity.Shared.Models.Task task)
        {
            await _context.Tasks.AddAsync(task);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }
    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //