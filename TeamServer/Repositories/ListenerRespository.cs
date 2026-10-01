//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TeamServer.Data;
using Trinity.Shared.DTOs.Listener;
using Trinity.Shared.Errors;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Models;
using Trinity.Shared.Results;

namespace TeamServer.Repositories
{
    public class ListenerRespository : RepositoryBase<Listener>, IListenerRepository
    {
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="context"></param>
        public ListenerRespository(Context context) : base(context)
        {
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Adds a Listener to the context asynchronously and returns its generated ID.
        /// </summary>
        /// <remarks>The entity is added to the DbContext; changes are not persisted until
        /// SaveChangesAsync is called.</remarks>
        /// <param name="listener">Listener to add.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the added Listener.</returns>
        public async System.Threading.Tasks.Task AddListenerAsync(Listener listener)
        {
            await _dbSet.AddAsync(listener);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Retrieves all listeners and projects them into ListenerResponse instances containing the listener ID, name,
        /// and protocol name.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of ListenerResponse
        /// objects.</returns>
        public async Task<List<ListenerResponse>> GetListenersAsync()
        {
            var listeners = await _dbSet
                .Include(l => l.Protocol)
                .Select(l => new ListenerResponse(
                    l.ID,
                    l.Name,
                    l.Protocol.Name))
                .ToListAsync();
            return listeners;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Asynchronously retrieves the Listener with the specified identifier.
        /// </summary>
        /// <param name="ID">The identifier of the Listener to retrieve.</param>
        /// <returns>A Task whose result is the Listener with the specified identifier, or null if no matching entity is found.</returns>
        public async Task<Listener> GetListenerAsync(int ID)
        {
            var listener = await _dbSet.FindAsync(ID);
            return listener;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Removes the specified listener from the underlying DbSet.
        /// </summary>
        /// <remarks>Marks the entity for deletion in the context; the removal is applied when SaveChanges
        /// is called.</remarks>
        /// <param name="listener">The listener to remove.</param>
        public void DeleteListener(Listener listener)
        {
            _dbSet.Remove(listener);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//