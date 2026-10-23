// ================================== { START OF FILE } ================================== //
using FluentValidation;
using Trinity.Shared.Models;

namespace TeamServer.Validators
{
    public class OperatorValidator : AbstractValidator<Operator>
    {
        public OperatorValidator()
        {
            RuleFor(OperatorEntity => OperatorEntity.Username)
                .NotEmpty()
                .WithMessage("Username is required.");
            RuleFor(OperatorEntity => OperatorEntity.Password)
                .NotEmpty()
                .WithMessage("Password is required.");
        }
    }
}
// ================================== { END OF FILE } ================================== //