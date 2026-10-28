// ========================================================================== { START OF FILE } ========================================================================== //
using Trinity.Shared.DTOs.Agent;
using Trinity.Shared.Results;

namespace TeamServer.Interface
{
    public interface IAgentService
    {
        Task<Result<List<AgentResponse>>> GetAgentsAsync();
        Task<Result<string>> Checkin(string blob);
    }
}
// ========================================================================== { END OF FILE } ========================================================================== //