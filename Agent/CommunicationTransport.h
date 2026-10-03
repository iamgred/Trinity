// ================================================================== { START OF FILE } =================================================================== //
#pragma once
#include <vector>
#include "CommunicationConfig.h"

class CommunicationTransport
{
public:
    virtual ~CommunicationTransport() = default;
    virtual void Configure(const CommunicationConfig &config) = 0;
    virtual bool Connect() = 0;
    virtual bool Send(const std::vector<unsigned char> &data) = 0;
    virtual std::vector<unsigned char> Receive() = 0;
    virtual void Disconnect() = 0;
};
// ================================================================== { END OF FILE } =================================================================== //