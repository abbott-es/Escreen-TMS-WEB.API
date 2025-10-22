using FluentValidation;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.Validation
{
    public class LocationDtoValidator : AbstractValidator<LocationDto>
    {
        public LocationDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.");
        }
    }
}
