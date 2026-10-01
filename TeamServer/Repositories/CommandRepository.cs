//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Trinity.Shared.Models;
using TeamServer.Data;

namespace TeamServer.Repositories
{
    public class CommandRepository : RepositoryBase<Trinity.Shared.Models.Task>
    {
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initializes a new instance of CommandRepository with the specified Context.
        /// </summary>
        /// <remarks>Forwards the provided context to the base class constructor.</remarks>
        /// <param name="context">The Context used for repository data access.</param>
        public CommandRepository(Context context) : base(context)
        {
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Adds the specified task entity to the underlying DbSet for insertion when changes are saved.
        /// </summary>
        /// <remarks>Does not persist changes to the database; call SaveChangesAsync to commit.</remarks>
        /// <param name="Task">The task entity to add to the context.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async System.Threading.Tasks.Task QueueAsync(Trinity.Shared.Models.Task Task)
        {
            await _dbSet.AddAsync(Task);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//