//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using TeamServer.Data;

namespace TeamServer.Services
{
    public static class StatusCacheService
    {
        private static readonly ConcurrentDictionary<int, string> _statuses = new();

        public static void Intialise(Context context) 
        {
            var dbStatuses = context.TaskStatus.AsNoTracking().ToDictionary(s => s.ID, s => s.Name);
            foreach(var kvp in dbStatuses)
            {
                _statuses[kvp.Key] = kvp.Value;
            }
        }
        public static string GetStatusName(int id) => _statuses.TryGetValue(id, out var name) ? name : "Not Found";
        public static int GetStatusID(string status) => _statuses.FirstOrDefault(kvp => kvp.Value.Equals(status)).Key;
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//