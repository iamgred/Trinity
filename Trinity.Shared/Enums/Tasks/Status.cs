//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System;
using System.Collections.Generic;
using System.Text;

namespace Trinity.Shared.Enums.Tasks
{
    public enum Status : int
    {
        NO_FLAG             = 0,
        ERROR               = 1,
        SUCCESS             = 2,
        DISPATCHED          = 3,
        QUEUED              = 4,
        RUNNING             = 5,
        MORE_TO_FOLLOW      = 6,
        NO_MORE_TO_FOLLOW   = 7,
        UPLOAD_CHUNK        = 8,
        UPLOAD_FINAL        = 9,
        DOWNLOAD_CHUNK      = 10,
        DOWNLOAD_FINAL      = 11,
        KEYLOGGER           = 12
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//