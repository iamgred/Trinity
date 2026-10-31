//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
#pragma once
#include <string>
#include <variant>
#include "rapidjson/stringbuffer.h"
#include "rapidjson/document.h"
#include "rapidjson/writer.h"

// Task Codes 
#define NO_CODE				0
#define SLEEP				1
#define SPAWN_TO			2
#define UPDATE_HOSTS		3
#define RUN					4
#define RUN_AS				5
#define RUN_U 				6
#define KILL_PROCESS		7
#define DOWNLOAD_CHUNK 		8
#define CANCEL_DOWNLOAD 	9

// Task Types 
#define NOP					0
#define INTIAL_CHECKIN		1
#define KILL				2
#define POWERSHELL			3
#define SET					4
#define PROCESS				5
#define DOWNLOAD			6
#define UPLOAD				7
#define SHELL				8
#define ESCALATE			9
#define NET_ASM				10
#define OBJECT_FILE			11
#define GET_UID				12
#define WIN_TASK			13

//  Statuses 
#define NO_FLAG 			0
#define TASK_ERROR			1
#define SUCCESS				2
#define DISPATCHED			3
#define QUEUED				4
#define RUNNING				5
#define MORE_TO_FOLLOW		6
#define NO_MORE_TO_FOLLOW	7

template <typename T>
struct TaskResponse
{
	int taskID;
	int type;
	int code;
	int status;
	T   data;

	std::string StructToJson() const
	{
		rapidjson::Document doc;
		doc.SetObject();

		rapidjson::Document::AllocatorType& allocator = doc.GetAllocator();

		doc.AddMember("TaskID", taskID, allocator);
		doc.AddMember("TaskType", type, allocator);
		doc.AddMember("taskCode", code, allocator);
		doc.AddMember("Status", status, allocator);
		doc.AddMember("Data", data.ToJsonValue(allocator), allocator);

		rapidjson::StringBuffer buffer;
		rapidjson::Writer<rapidjson::StringBuffer> writer(buffer);
		doc.Accept(writer);

		return buffer.GetString();
	}

	rapidjson::Value ToJsonValue(rapidjson::Document::AllocatorType& allocator) const {

		rapidjson::Value objValue;
		objValue.SetObject();

		objValue.AddMember("TaskID", taskID, allocator);
		objValue.AddMember("TaskType", type, allocator);
		objValue.AddMember("TaskCode", code, allocator);
		objValue.AddMember("Status", status, allocator);
		objValue.AddMember("Data", data.ToJsonValue(allocator),allocator);

		return objValue;
	}
};

template <typename T>
TaskResponse<T> CreateTaskResponse(int taskID, int type, int code, int status, T data)
{
	return TaskResponse<T> { taskID, type, code, status, data };
};

template <typename T>
struct TaskRequest
{
	int taskID;
	int type;
	int code;
	int status;
	T   data;

	std::string StructToJson() const
	{
		rapidjson::Document doc;
		doc.SetObject();

		rapidjson::Document::AllocatorType& allocator = doc.GetAllocator();

		doc.AddMember("TaskID", taskID, allocator);
		doc.AddMember("TaskType", type, allocator);
		doc.AddMember("TaskCode", code, allocator);
		doc.AddMember("Status", status, allocator);
		doc.AddMember("Command", data.ToJsonValue(allocator), allocator);

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
	return TaskRequest<T> { taskID, type, code, status, data };
};

struct TaskSuccess
{
	std::string output;

	rapidjson::Value ToJsonValue(rapidjson::Document::AllocatorType& allocator) const {
		rapidjson::Value obj(rapidjson::kObjectType);

		rapidjson::Value outputVal;
		outputVal.SetString(output.c_str(), static_cast<rapidjson::SizeType>(output.length()), allocator);
		obj.AddMember("Output", outputVal, allocator);

		return obj;
	}
};

struct TaskError
{
	int code;
	std::string message;

	rapidjson::Value ToJsonValue(rapidjson::Document::AllocatorType& allocator) const {
		rapidjson::Value obj(rapidjson::kObjectType);

		obj.AddMember("Code", code, allocator);
		rapidjson::Value msgVal;
		msgVal.SetString(message.c_str(), static_cast<rapidjson::SizeType>(message.length()), allocator);
		obj.AddMember("Message", msgVal, allocator);

		return obj;
	}
};

using Request = std::variant<TaskRequest<CommandPowershell>, TaskRequest<CommandWinTask>, TaskRequest<bool>>;
using Response = std::variant<TaskResponse<TaskSuccess>, TaskResponse<TaskError>>;

inline std::string CreateResponseTaskValue(const std::deque<Response>& tasks) {
	rapidjson::Document jsonDoc;
	jsonDoc.SetArray();
	rapidjson::Document::AllocatorType& allocator = jsonDoc.GetAllocator();

	for (const auto& taskVariant : tasks) {
		rapidjson::Value taskVal = std::visit([&allocator](const auto& concreteTask) -> rapidjson::Value {
			return concreteTask.ToJsonValue(allocator);
			}, taskVariant);

		jsonDoc.PushBack(taskVal, allocator);
	}

	rapidjson::StringBuffer strbuf;
	rapidjson::Writer<rapidjson::StringBuffer> writer(strbuf);
	jsonDoc.Accept(writer);

	return strbuf.GetString();
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//