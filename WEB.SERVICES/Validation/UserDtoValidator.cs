using FluentValidation;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.Validation
{
    public class UserDtoValidator : AbstractValidator<UserDto>
    {
        public UserDtoValidator()
        {
            RuleFor(x => x.RoleID)
                .NotEmpty().WithMessage("Role ID is required.");
        }
    }
}
