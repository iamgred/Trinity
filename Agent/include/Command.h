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

    rapidjson::Value ToJsonValue(rapidjson::Document::AllocatorType& allocator) const {
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

struct CommandWinTask
{
    std::string path;
    std::string username;
    std::string taskName;

    rapidjson::Value ToJsonValue(rapidjson::Document::AllocatorType& allocator) const {
        rapidjson::Value obj(rapidjson::kObjectType);

        rapidjson::Value pathVal;
        pathVal.SetString(path.c_str(), static_cast<rapidjson::SizeType>(path.length()), allocator);

        rapidjson::Value usernameVal;
        usernameVal.SetString(username.c_str(), static_cast<rapidjson::SizeType>(username.length()), allocator);

        rapidjson::Value taskNameVal;
        taskNameVal.SetString(taskName.c_str(), static_cast<rapidjson::SizeType>(taskName.length()), allocator);

        obj.AddMember("Path", pathVal, allocator);
        obj.AddMember("Username", usernameVal, allocator);
        obj.AddMember("TaskName", taskNameVal, allocator);

        return obj;
    }
};
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//