#pragma once

#include <cstddef>
#include <cstdint>
#include <string>

namespace FileTransfer
{
    struct FileChunk
    {
        std::string fileName;
        std::string data;
        std::int64_t offset;
        std::int64_t totalSize;
        bool final;
    };

    // Reads a chunk from a local file and Base64 encodes the bytes.
    bool ReadChunk(
        const std::string &filePath,
        std::int64_t offset,
        std::size_t chunkSize,
        FileChunk &chunk);

    // Decodes a Base64 chunk and writes it to the destination file.
    bool WriteChunk(
        const std::string &outputPath,
        const std::string &encodedData,
        std::int64_t offset);

    // Rebuilds a file from numbered chunk files.
    bool RebuildFile(
        const std::string &transferDirectory,
        const std::string &outputPath);
}