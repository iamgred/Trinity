//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System;
using System.Collections.Generic;
using System.Text;
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
        public required Architectures Architecture { get; set; }
        public required string RetryStrategy { get; set; }
        public required PayloadTypes PayloadType { get; set; }
        public required string AES256KEY { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^