//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.SignalR;
using TeamServer.NotificationDispatchers;
using TeamServer.Repositories;

namespace TeamServer.Services
{
    public class AgentStatusMonitorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly HashSet<int> _knownOnline = [];

        public AgentStatusMonitorService(IServiceScopeFactory scopeFactory, IHubContext<NotificationHub> hubContext)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5_000, stoppingToken);
                await CheckAgentsAsync();
            }
        }

        private async Task CheckAgentsAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<AgentRepository>();
                var agents = await repo.GetAgentsAsync();

                var nowOnline = new HashSet<int>();
                bool anyWentOffline = false;

                foreach (var agent in agents)
                {
                    bool online = agent.lastCheckIn != default &&
                        (DateTime.UtcNow - agent.lastCheckIn).TotalMilliseconds < (agent.sleep * 1.5);

                    if (online)
                        nowOnline.Add(agent.agentID);
                    else if (_knownOnline.Contains(agent.agentID))
                        anyWentOffline = true;
                }

                if (anyWentOffline)
                    await _hubContext.Clients.All.SendAsync("AgentPulse");

                _knownOnline.Clear();
                foreach (var id in nowOnline) _knownOnline.Add(id);
            }
            catch { }
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
