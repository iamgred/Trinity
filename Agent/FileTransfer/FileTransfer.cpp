// ==================================== { START OF FILE } ==================================== //
////// IGNORE IT DOESNT WORK YET ITS FOR LATER
#include "FileTransfer.h"

#include <cstdint>
#include <fstream>
#include <vector>

namespace
{
    constexpr char base64_chars[] =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        "abcdefghijklmnopqrstuvwxyz"
        "0123456789+/";

    std::string Base64Encode(
        const std::vector<std::uint8_t> &data)
    {
        std::string result;

        result.reserve(((data.size() + 2) / 3) * 4);

        std::size_t i = 0;

        while (i + 2 < data.size())
        {
            const std::uint32_t value =
                (static_cast<std::uint32_t>(data[i]) << 16) |
                (static_cast<std::uint32_t>(data[i + 1]) << 8) |
                static_cast<std::uint32_t>(data[i + 2]);

            result += base64_chars[(value >> 18) & 0x3F];
            result += base64_chars[(value >> 12) & 0x3F];
            result += base64_chars[(value >> 6) & 0x3F];
            result += base64_chars[value & 0x3F];

            i += 3;
        }

        const std::size_t remaining = data.size() - i;

        if (remaining == 1)
        {
            const std::uint32_t value =
                static_cast<std::uint32_t>(data[i]) << 16;

            result += base64_chars[(value >> 18) & 0x3F];
            result += base64_chars[(value >> 12) & 0x3F];
            result += '=';
            result += '=';
        }
        else if (remaining == 2)
        {
            const std::uint32_t value =
                (static_cast<std::uint32_t>(data[i]) << 16) |
                (static_cast<std::uint32_t>(data[i + 1]) << 8);

            result += base64_chars[(value >> 18) & 0x3F];
            result += base64_chars[(value >> 12) & 0x3F];
            result += base64_chars[(value >> 6) & 0x3F];
            result += '=';
        }

        return result;
    }
}

bool FileTransfer::ReadFile(
    const std::string &path,
    std::string &data)
{
    data.clear();

    std::ifstream file(
        path,
        std::ios::binary | std::ios::ate);

    if (!file.is_open())
        return false;

    const std::streamsize size = file.tellg();

    if (size < 0)
        return false;

    file.seekg(0, std::ios::beg);

    std::vector<std::uint8_t> bytes(
        static_cast<std::size_t>(size));

    if (size > 0)
    {
        file.read(
            reinterpret_cast<char *>(bytes.data()),
            size);

        if (!file)
            return false;
    }

    data = Base64Encode(bytes);

    return true;
}
// ==================================== { END OF FILE } ==================================== //
