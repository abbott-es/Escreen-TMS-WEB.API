using FluentValidation;
using WEB.DOMAIN.Entity;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.DTO;

public class UserDtoValidator : AbstractValidator<UserDto>
{
    private readonly IRepository<UserInfo> _userRepository;
    private readonly IRepository<Auth> _authRepository;

    public UserDtoValidator(IRepository<UserInfo> userRepository, IRepository<Auth> authRepository)
    {
        _userRepository = userRepository;
        _authRepository = authRepository;

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MustAsync(BeUniqueUsername).WithMessage("Username must be unique.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MustAsync(BeUniqueEmail).WithMessage("Email must be unique.");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken ct)
    {
        var existing = (await _userRepository.GetAllAsync(ct))
                       .Any(u => u.Email == email);
        return !existing;
    }
    private async Task<bool> BeUniqueUsername(string username, CancellationToken ct)
    {
        var existing = (await _authRepository.GetAllAsync(ct))
                       .Any(u => u.Username == username);
        return !existing;
    }
}
