//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Trinity.Shared.Interfaces;
using TeamServer.Data;
using Microsoft.EntityFrameworkCore;

namespace TeamServer.Repositories
{
    public class RepositoryBase<T> : IRepository<T> where T : class
    {
        protected readonly Context _context;
        protected readonly DbSet<T> _dbSet;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initializes a new instance of the Repository<T> class using the specified Context and resolves the DbSet for
        /// the entity type.
        /// </summary>
        /// <remarks>The context must not be null. The caller is responsible for managing and disposing
        /// the context's lifetime as appropriate.</remarks>
        /// <param name="context">Context used to access the database and to obtain the DbSet for the entity type.</param>
        public RepositoryBase(Context context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<T> GetByIdAsync(int id)
        {
            var result = await _dbSet.FindAsync(id) ?? throw new InvalidOperationException();
            return result;
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//