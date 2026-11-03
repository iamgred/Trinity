//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Trinity.Shared.Enums;

namespace Trinity.Shared.Models
{
    public class Tool
    {
        public int ID { get; set; }
        public required string Name { get; set; } = String.Empty;
        public required Arch Arch { get; set; } = Arch.x64;
        public required ToolType Type { get; set; } = ToolType.NET_ASM;
        public required double Size { get; set; } = default;
        public required string Path { get; set; } = String.Empty;
        public required int OperatorID { get; set; }
        public Operator? Operator { get; set; }
        public DateTime AddedDateTime { get; set; } = DateTime.UtcNow;
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//