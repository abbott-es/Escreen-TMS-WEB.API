using FluentValidation;
using WEB.DOMAIN.Entity;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.DTO;

public class AuthDTOValidator : AbstractValidator<UserDto>
{
    private readonly IRepository<Auth> _authRepository;
    private readonly IRepository<UserInfo> _userInfoRepository;

    public AuthDTOValidator(IRepository<Auth> authRepository, IRepository<UserInfo> userInfoRepository)
    {
        _authRepository = authRepository;
        _userInfoRepository = userInfoRepository;

        RuleFor(x => x.Auth.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MustAsync(BeUniqueUsername).WithMessage("Username must be unique.");

        RuleFor(x => x.Auth.Password)
            .NotEmpty().WithMessage("Password is required.");

        RuleFor(x => x.UserInfo.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MustAsync(BeUniqueEmail).WithMessage("Email must be unique.");
    }

    private async Task<bool> BeUniqueUsername(string username, CancellationToken ct)
    {
        var existing = (await _authRepository.GetAllAsync(u => u.Username == username, ct)).Any();
        return !existing;
    }
    private async Task<bool> BeUniqueEmail(string email, CancellationToken ct)
    {
        var existing = (await _userInfoRepository.GetAllAsync(u => u.Email == email, ct)).Any();
        return !existing;
    }
}
