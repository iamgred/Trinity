//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
#include "Agent.h"

void Agent::Exit()
{
    // Exit agent process
    ExitProcess(0);
}

std::string Agent::GetUID()
{
    char username[256];
    DWORD username_len = sizeof(username);
    // Get the username of the current user
    if (GetUserNameA(username, &username_len))
    {
        return std::string(username);
    }

    return std::string();
}

std::string Agent::ExecuteCommand(int commandType)
{
    switch (commandType)
    {
    case 0x0017:
        return GetUID();
    default:
        return std::string();
    }
}

void Agent::Init()
{
    // Initialize agent
    user = GetUID();
}

std::string Agent::GetUser() const
{
    return user;
}

int main(int argc, char *argv[])
{
    MessageBoxA(NULL, "Trinity C2 agent", ";)", MB_OK);

    Agent agent;
    agent.Init();

    return 0;
}

//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
