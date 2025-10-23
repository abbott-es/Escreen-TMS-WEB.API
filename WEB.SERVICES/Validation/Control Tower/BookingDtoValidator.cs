using FluentValidation;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.Validation.Control_Tower
{
    public class BookingDtoValidator : AbstractValidator<BookingDto>
    {
        public BookingDtoValidator()
        {
            RuleFor(x => x.VehicleID)
                .NotEmpty().WithMessage("Vehicle is required.");
            RuleFor(x => x.DriverUserID)
                .NotEmpty().WithMessage("Driver is required.");
            RuleFor(x => x.StartRoute)
                .NotEmpty().WithMessage("Start Route is required.");
            RuleFor(x => x.EndRoute)
                .NotEmpty().WithMessage("End Route is required.");
        }
    }
}
