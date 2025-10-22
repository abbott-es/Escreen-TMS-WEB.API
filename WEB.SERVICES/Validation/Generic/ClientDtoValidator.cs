using FluentValidation;
using WEB.SERVICES.DTO.Generic;

namespace WEB.SERVICES.Validation.Generic
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
