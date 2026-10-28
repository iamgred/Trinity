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

        [Post("/api/v1/commands/{agentID}/spawn/runas")]
        Task<CommandCreationResponse> QueueRunAsCommandAsync(int agentID, [Body] RunAsDTO dto);

        [Post("/api/v1/commands/{agentID}/spawn/run")]
        Task<CommandCreationResponse> QueueRunCommandAsync(int agentID, [Body] RunDTO dto);

        [Post("/api/v1/commands/{agentID}/spawn/runu")]
        Task<CommandCreationResponse> QueueRunUCommandAsync(int agentID, [Body] RunUDTO dto);

        [Post("/api/v1/commands/{agentID}/execute/killprocess")]
        Task<CommandCreationResponse> QueueRunUCommandAsync(int agentID, [Body] KillProcessDTO dto);

        [Post("/api/v1/commands/{agentID}/execute/dotnetassembly")]
        Task<CommandCreationResponse> QueueDotNetAsmCommandAsync(int agentID, [Body] DotNetAsmDTO dto);

        [Post("/api/v1/commands/{agentID}/execute/filedownload")]
        Task<CommandCreationResponse> QueueDownloadCommandAsync(int agentID, [Body] DownloadDTO dto);

        [Post("/api/v1/commands/{agentID}/execute/cancelfiledownload")]
        Task<CommandCreationResponse> QueueCancelDownloadCommandAsync(int agentID, [Body] CancelDownloadDTO dto);

        [Post("/api/v1/commands/{agentID}/execute/upload")]
        Task<CommandCreationResponse> QueueUploadCommandAsync(int agentID, [Body] UploadDTO dto);

        [Post("/api/v1/commands/{agentID}/set/spawnto")]
        Task<CommandCreationResponse> QueueSpawnToCommandAsync(int agentID, [Body] SpawnToDTO dto);

        [Post("/api/v1/commands/{agentID}/set/updatehosts")]
        Task<CommandCreationResponse> QueueUpdateHostsCommandAsync(int agentID, [Body] UpdateHostsDTO dto);

        [Post("/api/v1/commands/{agentID}/set/sleep")]
        Task<CommandCreationResponse> QueueSetSleepCommandAsync(int agentID, [Body] SetSleepDTO dto);

        [Post("/api/v1/commands/{agentID}/execute/kill")]
        Task<CommandCreationResponse> QueueKillAgentCommandAsync(int agentID);

        [Post("/api/v1/commands/{agentID}/execute/getuid")]
        Task<CommandCreationResponse> QueueGetUIDCommandAsync(int agentID);
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//