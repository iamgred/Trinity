//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Trinity.Shared.Enums
{
    [DataContract]
    public enum CommandTypes : byte
    {
        [EnumMember(Value = "PowerShell")]
        Powershell = 0x0001,
        [EnumMember(Value = "Shell")]
        Shell = 0x0002,
        [EnumMember(Value = "Download")]
        Download = 0x0003,
        [EnumMember(Value = "Cancel Download")]
        CancelDownload = 0x0004,
        [EnumMember(Value = "Upload")]
        Upload = 0x0005,
        [EnumMember(Value = "Kill Process")]
        Kill = 0x0006,
        [EnumMember(Value = "Execute Assembly")]
        ExecuteAssembly = 0x0006,
        [EnumMember(Value = "Execute Beacon Object File (BOF)")]
        BOF = 0x0007,
        [EnumMember(Value = "Execute")]
        Execute = 0x0008,
        [EnumMember(Value = "Run")]
        Run = 0x0009,
        [EnumMember(Value = "RunAs")]
        RunAs = 0x0010,
        [EnumMember(Value = "RunU")]
        RunU = 0x0011,
        [EnumMember(Value = "Escalate")]
        Escalate = 0x0012,
        [EnumMember(Value = "SpawnTo")]
        SpawnTo = 0x0013,
        [EnumMember(Value = "Update Hosts")]
        UpdateHosts = 0x0014,
        [EnumMember(Value = "Set Sleep")]
        SetSleep = 0x0015,
        [EnumMember(Value = "Kill Agent")]
        KillAgent = 0x0016,
        [EnumMember(Value = "Get UID")]
        GetUID = 0x0017

    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^