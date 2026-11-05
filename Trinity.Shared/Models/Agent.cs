//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
namespace Trinity.Shared.Models
{
    public class Agent
    {
        public int ID { get; set; }
        public required string CheckInUUID { get; set; }
        public required int CampaignID { get; set; }
        public Campaign Campaign { get; init; }
        public required int ListenerID { get; set; }
        public Listener Listener { get; init; }
        public required int PayloadID { get; set; }
        public Payload Payload { get; init; }
        public required string Username { get; set; }
        public required string ProcesseName { get; set; }
        public required string Architecure { get; set; }
        public required int ProcessPID { get; set; }
        public required string Integrity { get; set; }
        public required DateTime LastCheckIn { get; set; }
        public required int Sleep { get; set; }
        public required int Jitter { get; set; }
        public required string ExternalIP { get; set; }
        public required string InternalIP { get; set; }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//