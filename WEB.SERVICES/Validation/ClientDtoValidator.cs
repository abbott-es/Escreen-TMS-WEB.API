using FluentValidation;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.Validation
{
    public class ClientDtoValidator : AbstractValidator<ClientDto>
    {
        public ClientDtoValidator()
        {
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Client name is required.");
        }
    }
}
