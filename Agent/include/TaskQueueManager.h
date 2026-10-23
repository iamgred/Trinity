#pragma once
#include <string>
#include <queue>
#include "CheckinModels.h"

class TaskQueueManager {
public:
	TaskQueueManager();
	bool HasPendingResult();
	bool HasPendingTask();
	void StoreTaskResult(int taskID, bool isSuccessful, std::string output);
	std::string GetTaskResult();
	void QueueTask(std::string);
	CheckinResponse GetNextTask();
private:
	std::queue<ResultRequest> taskResults;
	std::queue<std::string> taskQueue;
};