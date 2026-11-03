#pragma once

#include <cstdint>
#include <string>

namespace Download
{
    // Decodes a Base64 chunk and writes it to the destination file.
    bool WriteChunk(
        const std::string &outputPath,
        const std::string &encodedData,
        std::int64_t offset);
}