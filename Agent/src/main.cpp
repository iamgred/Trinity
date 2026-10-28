//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
#include "Agent.h"
#include "CommManager.h"
#include "CheckinModels.h"
#include "TaskQueueManager.h"
#include "Task.h"
#include "Command.h"
#include <vector>

int main(int argc, char *argv[])
{
    CommManager commManager = {};
    TaskQueueManager taskManager = {};
    Agent agent = { commManager, taskManager };

    auto taskResponse = CreateTaskResponse(1, 1, 2, 2, TaskSuccess { "test_success_output" });
    std::cout << taskResponse.StructToJson() << std::endl;

    std::deque<Response> taskList;
    taskList.push_back(taskResponse);
    auto output = CreateResponseTaskValue(taskList);
    std::cout << output << std::endl;

    agent.Start();
    return 0;
}

//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
