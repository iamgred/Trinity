// ==================================================== { BEGINNING OF FILE } ==================================================== //
namespace Trinity.Shared.Models
{
    public class CampaignBridge
    {
        public int ID { get; set; }
        public required int CampaignID { get; set; }
        public required int OperatorID { get; set; }
        public required DateTime AssignedDate { get; set; }
    }
}
// ==================================================== { END OF FILE } ==================================================== //