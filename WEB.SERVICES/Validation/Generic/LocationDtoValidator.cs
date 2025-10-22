using FluentValidation;
using WEB.SERVICES.DTO.Generic;

namespace WEB.SERVICES.Validation.Generic
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
