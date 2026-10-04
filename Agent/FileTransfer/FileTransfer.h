// ==================================== { START OF FILE } ==================================== //
#pragma once

#include <cstdint>
#include <string>
#include <vector>

class FileTransfer
{
public:
    static bool CopyFile(
        const std::string &source,
        const std::string &destination);

    static bool GetFileSize(
        const std::string &path,
        std::uintmax_t &size);

    static bool FileExists(
        const std::string &path);

    static bool ReadFile(
        const std::string &path,
        std::vector<std::uint8_t> &data);
};
// ==================================== { END OF FILE } ==================================== //