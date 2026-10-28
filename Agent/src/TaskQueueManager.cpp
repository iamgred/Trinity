#include "TaskQueueManager.h"
#include "rapidjson/stringbuffer.h"
#include "rapidjson/document.h"
#include "rapidjson/writer.h"
#include "Task.h"

#define SUCCESSFUL "Successful"
#define FAILURE "Failure"

Request TaskQueueManager::ParseTaskRequest(const std::string &rawJson)
{
	rapidjson::Document doc;
	doc.Parse(rawJson.c_str());

	// Validate JSON
	if (doc.HasParseError() || !doc.IsObject())
		return TaskRequest<bool>{};

	// Support both uppercase and lowercase field names.
	const auto &taskIdValue =
		doc.HasMember("TaskID") ? doc["TaskID"] : doc.HasMember("taskID") ? doc["taskID"]
																		  : doc["taskID"];

	const auto &typeValue =
		doc.HasMember("TaskType") ? doc["TaskType"] : doc.HasMember("taskType") ? doc["taskType"]
												  : doc.HasMember("type")		? doc["type"]
																				: doc["type"];

	const auto &codeValue =
		doc.HasMember("TaskCode") ? doc["TaskCode"] : doc.HasMember("taskCode") ? doc["taskCode"]
																				: doc["code"];

	const auto &statusValue =
		doc.HasMember("Status") ? doc["Status"] : doc["status"];

	// Validate required fields before reading them.
	if (!taskIdValue.IsInt() ||
		!typeValue.IsInt() ||
		!codeValue.IsInt() ||
		!statusValue.IsInt())
		int taskID = doc["TaskID"].GetInt();
	int type = doc["TaskType"].GetInt();
	int code = doc["TaskCode"].GetInt();
	int status = doc["Status"].GetInt();
	const rapidjson::Value &node = doc["Command"];

	switch (type)
	{
		return TaskRequest<bool>{};
	}

	const int taskID = taskIdValue.GetInt();
	const int type = typeValue.GetInt();
	const int code = codeValue.GetInt();
	const int status = statusValue.GetInt();

	// Support "data", "Command", and "command" structures.
	const rapidjson::Value *command = nullptr;

	if (doc.HasMember("data") && doc["data"].IsObject())
		command = &doc["data"];
	else if (doc.HasMember("Command") && doc["Command"].IsObject())
		command = &doc["Command"];
	else if (doc.HasMember("command") && doc["command"].IsObject())
		command = &doc["command"];

	// No command object: return a valid envelope with no parsed payload.
	if (command == nullptr)
		return TaskRequest<bool>{taskID, type, code, status, false};

	const auto &node = *command;

	// --------------------------------------------------
	// Upload / Download path
	// --------------------------------------------------
	const char *pathKey =
		node.HasMember("Path") ? "Path" : "path";

	if (type == UPLOAD &&
		node.HasMember(pathKey) &&
		node[pathKey].IsString())
	{
		return TaskRequest<CommandUploadFile>{
			taskID, type, code, status,
			CommandUploadFile{node[pathKey].GetString()}};
	}

	if (type == DOWNLOAD_2 &&
		code == DOWNLOAD &&
		node.HasMember(pathKey) &&
		node[pathKey].IsString())
	{
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

		return TaskRequest<CommandDownloadFile>{
			taskID, type, code, status,
			CommandDownloadFile{
				node[pathKey].GetString(),
				data,
				offset,
				totalBytes,
				final}};
	}

	return TaskRequest<bool>{taskID, type, code, status, false};
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
