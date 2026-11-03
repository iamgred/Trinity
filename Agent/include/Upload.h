#pragma once

#include <cstddef>
#include <cstdint>
#include <string>

namespace Upload
{
    struct Chunk
    {
        std::string fileName;
        std::string data;
        std::int64_t offset;
        std::int64_t totalSize;
        bool final;
    };

    // Reads a portion of a local file and Base64 encodes the bytes.
    bool ReadChunk(
        const std::string &filePath,
        std::int64_t offset,
        std::size_t chunkSize,
        Chunk &chunk);
}