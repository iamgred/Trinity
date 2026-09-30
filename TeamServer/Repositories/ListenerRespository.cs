//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TeamServer.Data;
using Trinity.Shared.DTOs.Listener;
using Trinity.Shared.DTOs.Listener.Http;
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
        public async Task<Result> IsListenerHttp(int ID)
        {
            var listener = await _dbSet
                .Include(l => l.Protocol)
                .SingleOrDefaultAsync(l => l.ID.Equals(ID) && l.Protocol.Name.Equals("HTTP"));

            if (listener == null)
            {
                return ListenerError.NotFound(ID);
            }

            return Result.Success();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Adds the specified listener to the database and returns information about the created listener.
        /// </summary>
        /// <remarks>Persists the listener via the DbContext, saves changes, and loads the Protocol
        /// navigation property before creating the response.</remarks>
        /// <param name="listener">The listener entity to add to the database.</param>
        /// <returns>A task that resolves to a Result containing a CreateHttpListenerResponse with the created listener's ID,
        /// name, and protocol name.</returns>
        public async Task<Result> AddListenerAsync(Listener listener)
        {
            var result = await _dbSet.AddAsync(listener);
            await _context.SaveChangesAsync();
            await _context.Entry(listener).Reference(l => l.Protocol).LoadAsync();

            return Result.Success();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Asynchronously retrieves all listeners projected to ListenerResponse, including each listener's ID, name,
        /// and protocol name.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a List<ListenerResponse>
        /// representing the listeners.</returns>
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
        /// Gets the details for the listener with the specified identifier.
        /// </summary>
        /// <remarks>Performs an asynchronous lookup against the underlying data store and returns a
        /// not-found failure result if no matching listener is found.</remarks>
        /// <param name="ID">The identifier of the listener to retrieve.</param>
        /// <returns>A Result containing a GetListenerDetailsResponse when found; a failure Result if no listener with the
        /// specified identifier exists.</returns>
        public async Task<Result<GetListenerDetailsResponse>> GetListenerAsync(int ID)
        {
            var listener = await _dbSet.FindAsync(ID);

            if (listener == null)
            {
                return ListenerError.NotFound(ID);
            }

            return new GetListenerDetailsResponse(listener.ID, listener.Name, listener.Config);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Updates the configuration for the listener with the specified ID and saves the change asynchronously.
        /// </summary>
        /// <remarks>Saves changes to the database context asynchronously. Returns a NotFound error result
        /// if no listener exists with the specified ID.</remarks>
        /// <param name="ID">The identifier of the listener to update.</param>
        /// <param name="config">A JsonDocument containing the new listener configuration.</param>
        /// <returns>A Result representing success, or an error result when the listener is not found.</returns>
        public async Task<Result> UpdateListenerAsync(int ID, JsonDocument config)
        {
            var listener = await _dbSet.FindAsync(ID);

            if (listener == null)
            {
                return ListenerError.NotFound(ID);
            }

            listener.Config = config;
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Deletes the listener with the specified identifier from the database.
        /// </summary>
        /// <remarks>Performs the delete operation asynchronously and saves changes to the underlying
        /// context.</remarks>
        /// <param name="ID">The identifier of the listener to delete.</param>
        /// <returns>A Result indicating success, or an error result if the listener was not found.</returns>
        public async Task<Result> DeleteListenerAsync(int ID)
        {
            var listener = await _dbSet.FindAsync(ID);

            if (listener == null)
            {
                return ListenerError.NotFound(ID);
            }

            _dbSet.Remove(listener);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//