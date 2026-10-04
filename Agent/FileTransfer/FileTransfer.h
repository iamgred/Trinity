// ==================================== { START OF FILE } ==================================== //
#pragma once

#include <cstdint>
#include <string>
#include <vector>

class FileTransfer
{
public:
    static bool ReadFile(
        const std::string &path,
        std::vector<std::uint8_t> &data);

    static bool WriteBase64ToFile(
        const std::string &path,
        const std::string &data);
};
// ==================================== { END OF FILE } ==================================== //