
using FluentValidation;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.Validation
{
    public class HelperDtoValidator : AbstractValidator<HelperDto>
    {
        public HelperDtoValidator()
        {
            RuleFor(x => x.AssignedDriverID)
                .NotEmpty().WithMessage("Assigned driver is required.");
        }
    }
}
