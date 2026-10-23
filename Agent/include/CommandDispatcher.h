// ========================================== { START OF FILE } ========================================== //
#pragma once

#include <string>

class Agent;

class CommandDispatcher
{
public:
    explicit CommandDispatcher(Agent &agent);
    std::string Dispatch(int commandType, const std::string &command);

private:
    Agent &agent;
};
// ========================================== { END OF FILE } ========================================== //