using System.Text.Json;
using Trinity.Shared.Enums;

namespace Trinity.Shared.Models
{
    public class Listener
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public required ListenerType Type { get; set; }
        public required JsonDocument Config { get; set; }
    }
}
