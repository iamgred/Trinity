//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Refit;
using Trinity.Shared.DTOs.Command;
namespace Trinity.Shared.Interfaces
{
    public interface ICommandAPI
    {
        [Post("/api/v1/commands/{agentID}/spawn/powershell")]
        Task<CommandCreationResponse> QueuePowerShellCommandAsync(int agentID, [Body] PowerShellDTO dto);

        [Post("/api/v1/commands/{agentID}/spawn/shell")]
        Task<CommandCreationResponse> QueueShellCommandAsync(int agentID, [Body] ShellDTO dto);

        [Post("/api/v1/commands/{agentID}/spawn/runAs")]
        Task<CommandCreationResponse> QueueRunAsCommandAsync(int agentID, [Body] RunAsDTO dto);

        [Post("/api/v1/commands/{agentID}/spawn/run")]
        Task<CommandCreationResponse> QueueRunCommandAsync(int agentID, [Body] RunDTO dto);

        [Post("/api/v1/commands/{agentID}/spawn/runU")]
        Task<CommandCreationResponse> QueueRunUCommandAsync(int agentID, [Body] RunUDTO dto);

        [Post("/api/v1/commands/{agentID}/spawn/killprocess")]
        Task<CommandCreationResponse> QueueRunUCommandAsync(int agentID, [Body] KillProcessDTO dto);

        [Post("/api/v1/commands/{agentID}/spawn/dotnet")]
        Task<CommandCreationResponse> QueueDotNetAsmCommandAsync(int agentID, [Body] DotNetAsmDTO dto);
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//