//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.EntityFrameworkCore;
using TeamServer.Data;
using Trinity.Shared.DTOs.Listener;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Models;

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
        public async Task<CreateHttpListenerResponse> AddListenerAsync(Listener listener)
        {
            var result = await _dbSet.AddAsync(listener);
            await _context.SaveChangesAsync();

            return new CreateHttpListenerResponse(result.Entity.ID);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
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
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//