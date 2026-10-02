using System;
using System.Collections.Generic;
using System.Text;

namespace Trinity.Shared.Models
{
    public class Operator
    {
        public int ID { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
