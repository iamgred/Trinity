// ================================================================== { START OF FILE } =================================================================== //
#pragma once
#include "CommunicationTransport.h"
#include "CommunicationConfig.h"
#include <memory>
#include <vector>

class CommunicationManager
{
public:
    explicit CommunicationManager(std::unique_ptr<CommunicationTransport> transport);

    void Configure(const CommunicationConfig &config);

    bool Connect();
    bool Send(const std::vector<unsigned char> &data);
    std::vector<unsigned char> Receive();
    void Disconnect();

private:
    std::unique_ptr<CommunicationTransport> transport;
};
// ================================================================== { END OF FILE } =================================================================== //