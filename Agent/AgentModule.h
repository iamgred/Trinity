#pragma once
#include <string>
#include "CommManager.h"
#include "TaskQueueManager.h"
#include <windows.h>

class AgentModule {
public:
	AgentModule();
	BOOL GetUsername(OUT LPWSTR* szWhoamiStr);


private:
	CommManager commManager;
	TaskQueueManager taskManager;
};