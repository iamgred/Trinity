#pragma once

#include <cstddef>
#include <cstdint>
#include <string>
#include <rapidjson/document.h>

namespace Upload
{
    struct Chunk
    {
        std::string fileName;
        std::string data;
        std::int64_t offset;
        std::int64_t totalSize;
        bool final;

        rapidjson::Value ToJsonValue(
            rapidjson::Document::AllocatorType &allocator) const
        {
            rapidjson::Value obj(rapidjson::kObjectType);

            rapidjson::Value fileNameVal;
            fileNameVal.SetString(
                fileName.c_str(),
                static_cast<rapidjson::SizeType>(fileName.length()),
                allocator);

            rapidjson::Value dataVal;
            dataVal.SetString(
                data.c_str(),
                static_cast<rapidjson::SizeType>(data.length()),
                allocator);

            obj.AddMember("FileName", fileNameVal, allocator);
            obj.AddMember("Data", dataVal, allocator);
            obj.AddMember("Offset", offset, allocator);
            obj.AddMember("TotalSize", totalSize, allocator);
            obj.AddMember("Final", final, allocator);

            return obj;
        }
    };

    // Reads a portion of a local file and Base64 encodes the bytes.
    bool ReadChunk(
        const std::string &filePath,
        std::int64_t offset,
        std::size_t chunkSize,
        Chunk &chunk);
}