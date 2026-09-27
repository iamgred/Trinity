using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Trinity.Shared.Enums;
using Trinity.Shared.Interfaces;

namespace Trinity.Shared.Models
{
    public class Listener
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public required int ProtocolID { get; set; }
        public Protocol? Protocol { get; set; }
        public JsonDocument? Config { get; set; }
    }
}
