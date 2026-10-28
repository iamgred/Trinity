//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using FluentValidation;
using Trinity.Shared.DTOs.Listener.Http;


namespace TeamServer.Validators.Listeners.Http
{
    public class CreateHttpListenerRequestValidator : AbstractValidator<CreateHttpListenerRequest>
    {
        public CreateHttpListenerRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().Length(1, 250);
            RuleFor(x => x.Config).NotNull();

            RuleFor(x => x.Config.BindPort).InclusiveBetween(1, 65535);
            RuleFor(x => x.Config.C2Port).InclusiveBetween(1, 65535);
            RuleFor(x => x.Config.Header).NotEmpty();
            RuleFor(x => x.Config.RotationStrategy).NotEmpty();
            RuleFor(x => x.Config.MaxRetryStrategy).NotEmpty();
            RuleFor(x => x.Config.Hosts).NotEmpty();

        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//