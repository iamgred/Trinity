// ========================================== { START OF FILE } ========================================== //
#include "Agent.h"
#include "CommandDispatcher.h"

CommandDispatcher::CommandDispatcher(Agent &agent) : agent(agent) {}

std::string CommandDispatcher::Dispatch(int commandType, const std::string& command)
{
    switch (commandType)
    {
    case 0x0001:
        return agent.ExecutePowerShell(command);

    case 0x0002:
        return agent.ExecuteShell(command);

    default:
        return std::string();
    }
}
    // ========================================== { END OF FILE } ========================================== //