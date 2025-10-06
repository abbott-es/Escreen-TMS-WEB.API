using FluentValidation;
using WEB.DOMAIN.Entity;
using WEB.DOMAIN.Interface; // for IRepository<User>
using WEB.SERVICES.DTO;

public class AuthDTOValidator : AbstractValidator<UserDto>
{
    private readonly IRepository<Auth> _authRepository;

    public AuthDTOValidator(IRepository<Auth> authRepository)
    {
        _authRepository = authRepository;

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MustAsync(BeUniqueUsername).WithMessage("Username must be unique.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }

    private async Task<bool> BeUniqueUsername(string username, CancellationToken ct)
    {
        var existing = (await _authRepository.GetAllAsync(ct))
                       .Any(u => u.Username == username);
        return !existing;
    }
}
