// Agent.h : Include file for standard system include files,
// or project specific include files.

#pragma once

#include <Windows.h>
#include <string>

class Agent
{
public:
    void Exit();
    std::string GetUID();
    std::string GetUser() const;
    void Init();
    std::string ExecuteCommand(int commandType);
    std::string ExecutePowerShell(const std::string &command);

private:
    std::string user;
};
// TODO: Reference additional headers your program requires here.