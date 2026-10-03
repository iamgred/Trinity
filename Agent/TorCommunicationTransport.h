// ================================================================== { START OF FILE } =================================================================== //
#pragma once

#include "CommunicationTransport.h"
#include "CommunicationConfig.h"

#include <cstddef>
#include <cstdint>
#include <vector>
#include <string>

#ifdef _WIN32
#include <winsock2.h>
using SocketHandle = SOCKET;
#else
using SocketHandle = int;
#endif

class TorCommunicationTransport : public CommunicationTransport
{
public:
    TorCommunicationTransport();
    ~TorCommunicationTransport() override;

    void Configure(const CommunicationConfig &config) override;

    bool Connect() override;

    bool Send(const std::vector<unsigned char> &data) override;

    std::vector<unsigned char> Receive() override;

    void Disconnect() override;

private:
    bool ConnectToProxy();

    bool PerformSocks5Handshake();

    bool SendAll(
        const unsigned char *data,
        std::size_t size);

    bool ReceiveExact(
        unsigned char *data,
        std::size_t size);

    void CloseSocket();

    static bool ParsePort(
        const std::string &value,
        unsigned short &port);

private:
    CommunicationConfig config;

    SocketHandle socket;

    bool isConnected = false;
};

// ================================================================== { END OF FILE } =================================================================== //