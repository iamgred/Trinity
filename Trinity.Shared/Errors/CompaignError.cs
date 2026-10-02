using Trinity.Shared;
using Trinity.Shared.Enums;

namespace Trinity.Shared.Errors
{
    public static class CampaignError
    {
        public static Error NotFound(int campaignId)
        {
            return new Error(
                "Campaign Not Found",
                $"The campaign with ID {campaignId} was not found.",
                ErrorType.NotFound
            );
        }

        public static Error AlreadyExists(int campaignId)
        {
            return new Error(
                "Campaign Already Exists",
                $"A campaign with ID {campaignId} already exists.",
                ErrorType.Conflict
            );
        }

        public static Error OperatorAssignmentFailed(int operatorID, int campaignID)
        {
            return new Error(
                "Operator Assignment Failed",
                $"Failed to assign operator with ID {operatorID} to campaign with ID {campaignID}.",
                ErrorType.Validation
            );
        }

        public static Error OperatorAssignmentNotFound(int operatorID, int campaignID)
        {
            return new Error(
                "Operator Assignment Not Found",
                $"No assignment found for operator with ID {operatorID} in campaign with ID {campaignID}.",
                ErrorType.NotFound
            );
        }
    }
}