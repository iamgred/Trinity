//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Trinity.Shared.Enums;

namespace Trinity.Shared.Models
{
    public class Payload
    {
        public int ID { get; set; }
        public required string PayloadUUID { get; set; }
        public required int ListenerID { get; set; }
        public required int CampaignID { get; set; }
        public required int CreatedByOperatorID { get; set; }
        public required string FileName { get; set; }
        public required Arch Arch { get; set; }
        public required int Sleep { get; set; }
        public required int Jitter { get; set; }
        public required RotationStrategy RotationStrategy { get; set; }
        public required PayloadType Type { get; set; }
        public required string AES256KEY { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^