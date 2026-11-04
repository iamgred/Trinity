#include "Upload.h"

#include <algorithm>
#include <filesystem>
#include <fstream>
#include <vector>

#include "base64.hpp"

namespace fs = std::filesystem;

namespace Upload
{
    bool ReadChunk(
        const std::string &filePath,
        std::int64_t offset,
        std::size_t chunkSize,
        Chunk &chunk)
    {
        if (chunkSize == 0)
            return false;

        std::ifstream input(
            filePath,
            std::ios::binary | std::ios::ate);

        if (!input)
            return false;

        const auto fileSize = input.tellg();

        if (fileSize < 0)
            return false;

        if (offset < 0 ||
            offset > static_cast<std::int64_t>(fileSize))
        {
            return false;
        }

        input.seekg(offset, std::ios::beg);

        const auto remaining =
            static_cast<std::int64_t>(fileSize) - offset;

        const auto bytesToRead =
            static_cast<std::size_t>(
                std::min<std::int64_t>(
                    static_cast<std::int64_t>(chunkSize),
                    remaining));

        std::vector<char> bytes(bytesToRead);

        if (bytesToRead > 0)
        {
            input.read(
                bytes.data(),
                static_cast<std::streamsize>(bytesToRead));

            if (!input)
                return false;
        }

        /*
         * Match the .NET ASM approach:
         *
         * raw bytes
         *     ↓
         * std::string
         *     ↓
         * Base64
         */
        const std::string rawBytes(
            bytes.begin(),
            bytes.end());

        chunk.fileName =
            fs::path(filePath).filename().string();

        chunk.data =
            base64::to_base64(rawBytes);

        chunk.offset = offset;

        chunk.totalSize =
            static_cast<std::int64_t>(fileSize);

        chunk.final =
            offset +
                static_cast<std::int64_t>(bytesToRead) >=
            static_cast<std::int64_t>(fileSize);

        return true;
    }
}