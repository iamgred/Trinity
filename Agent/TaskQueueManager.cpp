#include "TaskQueueManager.h"
#include "CheckinModels.h"
#include <string>
#include <queue>

#define SUCCESSFUL "Successful"
#define FAILURE "Failure"

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

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
// Default constructor
TaskQueueManager::TaskQueueManager()
{
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
bool TaskQueueManager::HasPendingResult()
{
	return taskResults.size() > 0;
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
bool TaskQueueManager::HasPendingTask()
{
	return taskQueue.size() > 0;
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
std::string GetTaskStatusMessage(bool isSuccessful)
{
	switch (isSuccessful)
	{
	case true:
		return SUCCESSFUL;
		break;
	default:
		return FAILURE;
	}
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
void TaskQueueManager::StoreTaskResult(int taskID, bool isSuccessful, std::string output)
{
	ResultRequest result = { taskID, GetTaskStatusMessage(isSuccessful), output};
	taskResults.push(result);
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
std::string TaskQueueManager::GetTaskResult()
{
	std::string result = taskResults.front().StructToJson();
	taskResults.pop();
	return result;
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
void TaskQueueManager::QueueTask(std::string blob)
{
	if (blob.empty()) return;
	taskQueue.push(blob);
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
CheckinResponse TaskQueueManager::GetNextTask()
{
	std::string taskBlob = taskQueue.front();
	taskQueue.pop();
	return ParseCommand(taskBlob);
}
