#pragma once
#include <string>
#include "CommManager.h"

class AgentModule {
public:
	AgentModule();


private:
	CommManager commManager;
	std::string host;
	std::string callbackUUID;
};