//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System;
using System.Collections.Generic;
using System.Text;

namespace Trinity.Shared.Models
{
    public class Host
    {
        public int ID { get; set; }
        public int AgentID { get; set; }
        public required string Hostname { get; set; }
        public required string Domain { get; set; }
        public required string OS { get; set; }
        public required string Motherboard { get; set; }
        public required int RAM { get; set; }
        public required double DiskSize { get; set; }
        public required double FreeDisk { get; set; }
        public required string GPU { get; set; }
        public required string CPU { get; set; }
        public required int CPUCount { get; set; }
        public required string MACAddress { get; set; } // Can normalise the schema here (Not read very much)
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//