// ================================== { START OF FILE } ================================== //
using FluentValidation;
using Trinity.Shared.Models;

namespace TeamServer.Validators
{
    public class CampaignValidator : AbstractValidator<Campaign>
    {
        public CampaignValidator()
        {
            RuleFor(campaign => campaign.Name)
                .NotEmpty()
                .WithMessage("Campaign name is required.");
        }
    }
}
// ================================== { END OF FILE } ================================== //