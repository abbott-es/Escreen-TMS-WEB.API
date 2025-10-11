using FluentValidation;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.Validation
{
    public class LogoutDtoValidator : AbstractValidator<LogoutDto>
    {
        public LogoutDtoValidator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage("AccessToken is required.");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("RefreshToken is required.");
        }
    }
}