using System;
using System.Collections.Generic;
using System.Text;

namespace Trinity.Shared.Models
{
    public abstract class ListenerBase
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public int ListenerID { get; set; }
        public Listener Listener { get; set; } = null!;
    }
}
