//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
#pragma once
#include <cstdint>
#include <string>
#include "rapidjson/stringbuffer.h"
#include "rapidjson/document.h"
#include "rapidjson/writer.h"

// Task Codes
#define NO_CODE 0
#define SLEEP 1
#define SPAWN_TO 2
#define UPDATE_HOSTS 3
#define RUN 4
#define RUN_AS 5
#define RUN_U 6
#define KILL_PROCESS 7
#define DOWNLOAD 8
#define CANCEL_DOWNLOAD 9

// Task Types
#define NOP 0
#define INTIAL_CHECKIN 1
#define KILL 2
#define SET 4
#define PROCESS 5
#define DOWNLOAD_2 6
#define UPLOAD 7
#define SHELL 8
#define ESCALATE 9
#define NET_ASM 10
#define OBJECT_FILE 11
#define GET_UID 12

//  Statuses
#define NO_FLAG 0
#define TASK_ERROR 1
#define SUCCESS 2
#define DISPATCHED 3
#define QUEUED 4
#define RUNNING 5
#define MORE_TO_FOLLOW 6
#define NO_MORE_TO_FOLLOW 7

template <typename T>
struct TaskResponse
{
	int taskID;
	int type;
	int code;
	int status;
	T data;

	std::string StructToJson() const
	{
		rapidjson::Document doc;
		doc.SetObject();

		rapidjson::Document::AllocatorType &allocator = doc.GetAllocator();

		doc.AddMember("TaskID", taskID, allocator);
		doc.AddMember("TaskType", type, allocator);
		doc.AddMember("TaskCode", code, allocator);
		doc.AddMember("Status", status, allocator);
		doc.AddMember("Data", data.ToJsonValue(allocator), allocator);

		rapidjson::StringBuffer buffer;
		rapidjson::Writer<rapidjson::StringBuffer> writer(buffer);
		doc.Accept(writer);

		return buffer.GetString();
	}
};

template <typename T>
TaskResponse<T> CreateTaskResponse(int taskID, int type, int code, int status, T data)
{
	return TaskResponse<T>{taskID, type, code, status, data};
};

template <typename T>
struct TaskRequest
{
	int taskID;
	int type;
	int code;
	int status;
	T data;

	std::string StructToJson() const
	{
		rapidjson::Document doc;
		doc.SetObject();

		rapidjson::Document::AllocatorType &allocator = doc.GetAllocator();

		doc.AddMember("taskID", taskID, allocator);
		doc.AddMember("type", type, allocator);
		doc.AddMember("code", code, allocator);
		doc.AddMember("status", status, allocator);
		doc.AddMember("data", data.ToJsonValue(allocator), allocator);

		rapidjson::StringBuffer buffer;
		rapidjson::Writer<rapidjson::StringBuffer> writer(buffer);
		doc.Accept(writer);

		return buffer.GetString();
	}

	int GetType()
	{
		return type;
	}
};

template <typename T>
TaskRequest<T> CreateTaskRequest(int taskID, int type, int code, int status, T data)
{
	return TaskRequest<T>{taskID, type, code, status, data};
};

struct TaskSuccess
{
	std::string output;

	rapidjson::Value ToJsonValue(rapidjson::Document::AllocatorType &allocator) const
	{
		rapidjson::Value outputValue;
		outputValue.SetString(output.c_str(), static_cast<rapidjson::SizeType>(output.length()), allocator);
		return outputValue;
	}
};

struct TaskError
{
	int code;
	std::string message;

	rapidjson::Value ToJsonValue(rapidjson::Document::AllocatorType &allocator) const
	{
		rapidjson::Value obj(rapidjson::kObjectType);

		obj.AddMember("code", code, allocator);
		rapidjson::Value msgVal;
		msgVal.SetString(message.c_str(), static_cast<rapidjson::SizeType>(message.length()), allocator);
		obj.AddMember("message", msgVal, allocator);

		return obj;
	}
};

struct CommandUploadFile
{
	std::string path;

	rapidjson::Value ToJsonValue(rapidjson::Document::AllocatorType &allocator) const
	{
		rapidjson::Value value(rapidjson::kObjectType);
		value.AddMember("Path", rapidjson::Value(path.c_str(), allocator), allocator);
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