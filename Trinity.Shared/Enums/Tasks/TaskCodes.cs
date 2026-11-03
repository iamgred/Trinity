//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System;
using System.Collections.Generic;
using System.Text;

namespace Trinity.Shared.Enums.Tasks
{
    public enum TaskCode : int
    {
        NO_CODE = 0,
        SLEEP = 1,
        SPAWN_TO = 2,
        UPDATE_HOSTS = 3,
        RUN = 4,
        RUN_AS = 5,
        RUN_U = 6,
        KILL_PROCESS = 7,
        DOWNLOAD_CHUNK = 8,
        START_KEYLOGGER = 9,
        STOP_KEYLOGGER = 10,
        DOWNLOAD_FINAL = 11
    }

}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//