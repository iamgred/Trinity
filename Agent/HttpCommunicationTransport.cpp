// ================================================================== { START OF FILE } =================================================================== //
#include "HttpCommunicationTransport.h"

void HttpCommunicationTransport::Configure(const CommunicationConfig &newConfig)
{
    this->config = newConfig;
}

bool HttpCommunicationTransport::Connect()
{
    if (isConnected)
    {
        return true; // Already connected
    }

    // Open a WinHTTP session
    hInternet = WinHttpOpen(L"Trinity HTTP Client", WINHTTP_ACCESS_TYPE_DEFAULT_PROXY, WINHTTP_NO_PROXY_NAME, WINHTTP_NO_PROXY_BYPASS, 0);

    if (hInternet == nullptr)
    {
        return false; // Failed to open WinHTTP session
    }

    // Convert the endpoint string to a wide string for WinHTTP
    std::wstring endpointW(config.endpoint.begin(), config.endpoint.end());

    // Establish a connection to the server
    hConnect = WinHttpConnect(hInternet, endpointW.c_str(), config.port, 0);

    if (hConnect == nullptr)
    {
        WinHttpCloseHandle(hInternet);
        hInternet = nullptr;
        return false; // Failed to connect to the server
    }

    isConnected = true;
    return true;
}

bool HttpCommunicationTransport::Send(const std::vector<unsigned char> &data)
{
    if (!isConnected || hConnect == nullptr)
    {
        return false; // Not connected
    }

    HINTERNET hRequest = WinHttpOpenRequest(hConnect, L"POST", L"/", nullptr, WINHTTP_NO_REFERER, WINHTTP_DEFAULT_ACCEPT_TYPES, 0);

    if (hRequest == nullptr)
    {
        return false; // Failed to open HTTP request
    }

    // Send the HTTP request with the data
    const wchar_t *headers = L"Content-Type: application/octet-stream\r\n";

    // Send the request with the data
    BOOL result = WinHttpSendRequest(
        hRequest,
        headers,
        static_cast<DWORD>(-1L),
        data.empty() ? nullptr : const_cast<unsigned char *>(data.data()),
        static_cast<DWORD>(data.size()),
        static_cast<DWORD>(data.size()),
        0);

    if (!result)
    {
        WinHttpCloseHandle(hRequest);
        return false; // Failed to send the request
    }

    result = WinHttpReceiveResponse(
        hRequest,
        nullptr);

    if (!result)
    {
        WinHttpCloseHandle(hRequest);
        return false;
    }

    responseData.clear();

    DWORD availableSize = 0;

    while (WinHttpQueryDataAvailable(hRequest, &availableSize) && availableSize > 0)
    {
        std::vector<unsigned char> buffer(availableSize);
        DWORD bytesRead = 0;

        if (!WinHttpReadData(
                hRequest,
                buffer.data(),
                availableSize,
                &bytesRead))
        {
            WinHttpCloseHandle(hRequest);
            return false;
        }

        responseData.insert(
            responseData.end(),
            buffer.begin(),
            buffer.begin() + bytesRead);

        if (bytesRead == 0)
        {
            break;
        }
    }

    WinHttpCloseHandle(hRequest);

    return true;
}

std::vector<unsigned char> HttpCommunicationTransport::Receive()
{
    if (!isConnected)
    {
        return {}; // Return empty vector if not connected
    }

    std::vector<unsigned char> receivedData = std::move(responseData);
    responseData.clear(); // Clear the response data after moving it

    return receivedData;
}

void HttpCommunicationTransport::Disconnect()
{
    /// Close the WinHTTP connection handle
    if (hConnect != nullptr)
    {
        WinHttpCloseHandle(hConnect);
        hConnect = nullptr;
    }
    // Close the WinHTTP session handle
    if (hInternet != nullptr)
    {
        WinHttpCloseHandle(hInternet);
        hInternet = nullptr;
    }
    isConnected = false;
}
// ================================================================== { END OF FILE } =================================================================== //