//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Refit;
using System.Threading.Tasks;
using AgentTask = Trinity.Shared.Models.Task;
using Tool = Trinity.Shared.Models.Tool;

namespace Trinity.Shared.Interfaces
{
    public interface IArmouryAPI
    {
        [Get("/api/v1/armoury/tools")]
        Task<List<Tool>> GetToolsAsync([Query] int pagination, [Query] int rows);

        [Delete("/api/v1/armoury/remove/{toolID}")]
        Task RemoveToolAsync(int toolID);
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
