// ================================================================== { START OF FILE } =================================================================== //
#pragma once
#include <string>
#include <unordered_map>

enum class CommunicationType
{
    TCP,
    HTTP,
    SMB,
    TOR
};

struct CommunicationConfig
{
    CommunicationType type;
    std::string endpoint;
    unsigned short port = 0;

    // Transport-specific configuration.
    // HTTP/Tor/etc. can interpret these without
    // CommunicationManager knowing transport details.
    std::unordered_map<std::string, std::string> options;
};
// ================================================================== { END OF FILE } =================================================================== //