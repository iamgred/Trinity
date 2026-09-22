//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Trinity.Shared.Enums
{
    [DataContract]
    public enum CommandTypes
    {
        [EnumMember(Value = "PowerShell")]
        Powershell,
        [EnumMember(Value = "Shell")]
        Shell,
        [EnumMember(Value = "Download")]
        Download,
        [EnumMember(Value = "Cancel Download")]
        CancelDownload,
        [EnumMember(Value = "Upload")]
        Upload,
        [EnumMember(Value = "Kill Process")]
        Kill,
        [EnumMember(Value = "Execute Assembly")]
        ExecuteAssembly,
        [EnumMember(Value = "Execute Beacon Object File (BOF)")]
        BOF,
        [EnumMember(Value = "Execute")]
        Execute,
        [EnumMember(Value = "Run")]
        Run,
        [EnumMember(Value = "RunAs")]
        RunAs,
        [EnumMember(Value = "RunU")]
        RunU,
        [EnumMember(Value = "Escalate")]
        Escalate,
        [EnumMember(Value = "SpawnTo")]
        SpawnTo,
        [EnumMember(Value = "Update Hosts")]
        UpdateHosts,
        [EnumMember(Value = "Set Sleep")]
        SetSleep,
        [EnumMember(Value = "Kill Agent")]
        KillAgent

    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//