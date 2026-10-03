// ================================================================== { START OF FILE } =================================================================== //
#pragma once
#include "CommunicationTransport.h"
#include "CommunicationConfig.h"
#include <vector>
#include <winhttp.h>

class HttpCommunicationTransport : public CommunicationTransport
{
public:
    HttpCommunicationTransport() = default;
    ~HttpCommunicationTransport() override = default;

    void Configure(const CommunicationConfig &config) override;
    bool Connect() override;
    bool Send(const std::vector<unsigned char> &data) override;
    std::vector<unsigned char> Receive() override;
    void Disconnect() override;

private:
    CommunicationConfig config;
    bool isConnected = false;

    HINTERNET hInternet = nullptr;
    HINTERNET hConnect = nullptr;
    std::vector<unsigned char> responseData;
};
// ================================================================== { END OF FILE } =================================================================== //