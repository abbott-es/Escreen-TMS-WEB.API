using FluentValidation;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.Validation
{
    public class RoleDtoValidator : AbstractValidator<RoleDto>
    {
        public RoleDtoValidator()
        {
            RuleFor(x => x.RoleName)
                .NotEmpty().WithMessage("Role name is required.");
        }
    }
}
