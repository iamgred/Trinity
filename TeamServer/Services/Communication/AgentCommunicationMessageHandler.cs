// =========================================== { START OF FILE } =========================================== //
using System;
using System.Text;
using Trinity.Shared.Interfaces;

namespace TeamServer.Services.Communication
{
    public class AgentCommunicationMessageHandler : ICommunicationMessageHandler
    {
        private readonly AgentService _agentService;

        public AgentCommunicationMessageHandler(AgentService agentService)
        {
            _agentService = agentService;
        }

        public async Task<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> message, CancellationToken cancellationToken = default)
        {
            var blob = Encoding.UTF8.GetString(message.Span);
            var result = await _agentService.Checkin(blob);
            return Encoding.UTF8.GetBytes(result.Response ?? string.Empty);
        }
    }
}
// =========================================== { END OF FILE } =========================================== //