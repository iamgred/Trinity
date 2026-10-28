#include "TaskQueueManager.h"
#include "rapidjson/stringbuffer.h"
#include "rapidjson/document.h"
#include "rapidjson/writer.h" 
#include "Task.h"

#define SUCCESSFUL "Successful"
#define FAILURE "Failure"

Request TaskQueueManager::ParseTaskRequest(const std::string& rawJson)
{

	rapidjson::Document doc = {};
	doc.Parse(rawJson.c_str());

	int taskID = doc["taskID"].GetInt();
	int type = doc["type"].GetInt();
	int code = doc["code"].GetInt();
	int status = doc["status"].GetInt();
	const rapidjson::Value& node = doc["data"];

	switch (type)
	{
		case POWERSHELL:
			auto cmd = node["Commandlet"].GetString();
			auto arg = node["Arguements"].GetString();
			return CreateTaskRequest<CommandPowershell>(taskID, type, code, status, CommandPowershell { cmd, arg });
			break;
	}
	TaskRequest<bool> test;
	return test;
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
void TaskQueueManager::StoreTaskResult(Response& response)
{
	std::string json = std::visit([](const auto& responseEnvelope) {
		return responseEnvelope.StructToJson();
		}, response);

	taskResults.push(json);
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
std::string TaskQueueManager::GetTaskResult()
{
	std::string response = taskResults.front();
	taskResults.pop();
	return response;
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
void TaskQueueManager::QueueTask(std::string blob)
{
	if (blob.empty()) return;
	taskQueue.push(blob);
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
Request TaskQueueManager::GetNextTask()
{
	std::string taskBlob = taskQueue.front();
	taskQueue.pop();
	return ParseTaskRequest(taskBlob);
}
