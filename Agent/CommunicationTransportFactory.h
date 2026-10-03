// ================================================================== { START OF FILE } =================================================================== //
#pragma once
#include "CommunicationTransport.h"
#include "CommunicationConfig.h"
#include <memory>

class CommunicationTransportFactory
{
public:
    static std::unique_ptr<CommunicationTransport> Create(const CommunicationConfig &config);
};
// ================================================================== { END OF FILE } =================================================================== //