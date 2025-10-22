using FluentValidation;
using WEB.SERVICES.DTO.Generic;

namespace WEB.SERVICES.Validation.Generic
{
    public class VehicleDtoValidator : AbstractValidator<VehicleDto>
    {
        public VehicleDtoValidator()
        {
            RuleFor(x => x.Model)
                .NotEmpty().WithMessage("Model is required.");
            RuleFor(x => x.PlateNumber)
                .NotEmpty().WithMessage("Plate Number is required.");
        }
    }
}
