// Agent.h : Include file for standard system include files,
// or project specific include files.

#pragma once

#include <Windows.h>
#include <string>
#include "CommunicationConfig.h"
#include "CommunicationManager.h"
#include <memory>

class Agent
{
public:
    void Exit();
    std::string GetUID();
    std::string GetUser() const;
    void Init();
    std::string ExecuteCommand(int commandType);
    std::string ExecutePowerShell(const std::string &command);
    void ConfigureCommunication(const CommunicationConfig &config);

private:
    std::string user;
    std::unique_ptr<CommunicationManager> communicationManager;
};
// TODO: Reference additional headers your program requires here.