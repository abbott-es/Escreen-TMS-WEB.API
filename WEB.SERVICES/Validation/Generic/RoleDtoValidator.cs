using FluentValidation;
using WEB.SERVICES.DTO.Generic;

namespace WEB.SERVICES.Validation.Generic
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
