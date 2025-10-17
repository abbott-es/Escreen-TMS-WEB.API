using FluentValidation;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.Validation
{
    public class DriverDtoValidator : AbstractValidator<DriverDto>
    {
        public DriverDtoValidator()
        {
            RuleFor(x => x.LicenseNumber)
                .NotEmpty().WithMessage("License number is required.");
        }
    }
}
