//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
#include "Agent.h"
#include "CheckinModels.h"
#include "CommManager.h"
#include "TaskQueueManager.h"

int main(int argc, char* argv[]) {
    CommManager commManager = {};
    TaskQueueManager taskManager = {};
    Agent agent = {commManager, taskManager};
    agent.Start();
    return 0;
}

//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
