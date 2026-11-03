#include "Download.h"
#include <fstream>

#include "base64.hpp"

namespace Download
{
    bool WriteChunk(
        const std::string &outputPath,
        const std::string &encodedData,
        std::int64_t offset)
    {
        if (offset < 0)
            return false;

        /*
         * Base64
         *     ↓
         * raw bytes
         */
        const std::string bytes =
            base64::from_base64(encodedData);

        std::fstream output(
            outputPath,
            std::ios::binary |
                std::ios::in |
                std::ios::out);

        /*
         * The file does not exist yet.
         * Create it if this is the first chunk.
         */
        if (!output)
        {
            output.clear();

            output.open(
                outputPath,
                std::ios::binary |
                    std::ios::out |
                    std::ios::trunc);

            if (!output)
                return false;
        }

        /*
         * Put the write position at the chunk's
         * original byte offset.
         */
        output.seekp(
            offset,
            std::ios::beg);

        if (!bytes.empty())
        {
            output.write(
                bytes.data(),
                static_cast<std::streamsize>(bytes.size()));
        }

        return static_cast<bool>(output);
    }
}