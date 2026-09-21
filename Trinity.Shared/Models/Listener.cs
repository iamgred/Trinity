using System;
using System.Collections.Generic;
using System.Text;
using Trinity.Shared.Enums;
using Trinity.Shared.Interfaces;

namespace Trinity.Shared.Models
{
    public class Listener
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public required ListenerTypes Type { get; set; }
        public required DateTime CreatedAt { get; set; }
        public ListenerBase Listeners { get; set; }
    }
}
