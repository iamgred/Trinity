using System;
using System.Collections.Generic;
using System.Text;

namespace Trinity.Shared.DTOs.Checkin
{
    public record IntialCheckInRequest
    {
        public string internalIP { get; init; } = string.Empty;
        public string externalIP { get; init; } = string.Empty;
        public string OS { get; init; } = string.Empty;
        public string user { get; init; } = string.Empty;
        public string processName { get; init; } = string.Empty;
        public int PID { get; init; }
        public string Integrity { get; init; } = string.Empty;
        public string macAddress { get; init; } = string.Empty;
        public string motherboard { get; init; } = string.Empty;
        public int RAM { get; init; }
        public double DiskSize { get; init; }
        public double FreeDisk { get; init; }
        public int CPUCount { get; init; }

        public IntialCheckInRequest()
        {
        }

        public IntialCheckInRequest(string internalIP, string externalIP, string OS, string user,
            string processName, int PID, string Integrity, string macAddress, string motherboard, int RAM,
            double DiskSize, double FreeDisk, int CPUCount)
        {
            this.internalIP = internalIP;
            this.externalIP = externalIP;
            this.OS = OS;
            this.user = user;
            this.processName = processName;
            this.PID = PID;
            this.Integrity = Integrity;
            this.macAddress = macAddress;
            this.motherboard = motherboard;
            this.RAM = RAM;
            this.DiskSize = DiskSize;
            this.FreeDisk = FreeDisk;
            this.CPUCount = CPUCount;
        }
    }
}
