using System;
using System.Collections.Generic;
using System.Text;

namespace Trinity.Shared.DTOs.Checkin
{
    public record IntialCheckInRequest(string internalIP, string externalIP, string OS, string user,
        string processName, int PID, string Integrity, string macAddress, string motherboard, int RAM,
        double DiskSize, double FreeDisk, int CPUCount);
}
