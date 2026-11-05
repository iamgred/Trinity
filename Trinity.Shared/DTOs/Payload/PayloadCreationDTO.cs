//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Trinity.Shared.Enums;

namespace Trinity.Shared.DTOs.Payload
{
    public class PayloadCreationDTO
    {
        public string Name { get; set; } = String.Empty;
        public int OperatorID { get; set; }
        public int Sleep { get; set; }
        public int Jitter { get; set; }
        public int ListenerID { get; set; }
        public int CampaignID { get; set; }
        public Arch Architecture { get; set; }
        public PayloadType Type { get; set; }
        public RotationStrategy RotationStrategy { get; set; }
        public bool isStageless { get; set; } = default;
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//