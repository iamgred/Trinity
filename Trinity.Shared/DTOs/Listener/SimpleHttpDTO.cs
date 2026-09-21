using System;
using System.Collections.Generic;
using System.Text;

namespace Trinity.Shared.DTOs.Listener
{
    public class SimpleHttpDTO
    {
        public string Name { get; set; } = String.Empty;
        public string Host { get; set; } = String.Empty;
        public string Type { get; set;  } = String.Empty;
    }
}
