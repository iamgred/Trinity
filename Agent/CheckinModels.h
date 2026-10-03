#pragma once
#include <string>

struct IntitalCheckinRequest
{
	std::string internalIP:
	std::string externalIP;
	std::string OS;
	std::string user;
	std::string processName;
	int PID;
	std::string integrity;
	std::string macAddress;
	std::string motherboard;
	int RAM;
	double diskSize;
	double FreeDisk;
	int CPUCount;
};

struct FullCheckinRequest
{
	int taskID;
	std::string status;
	std::string output;
};