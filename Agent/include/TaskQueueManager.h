#pragma once
#include <string>
#include <queue>
#include <variant>
#include "CheckinModels.h"
#include "Command.h"
#include "Task.h"

class TaskQueueManager {
public:
	TaskQueueManager();
	bool HasPendingResult();
	bool HasPendingTask();
	void StoreTaskResult(Response& response);
	std::string GetTaskResults();
	void QueueTask(std::string);
	Request GetNextTask();
	Request ParseTaskRequest(const std::string& rawJson);
private:
	std::deque<Response> taskResults;
	std::queue<std::string> taskQueue;
};