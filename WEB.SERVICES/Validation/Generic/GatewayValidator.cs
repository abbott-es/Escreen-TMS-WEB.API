using FluentValidation;
using WEB.DOMAIN.Entity.Generic;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.DTO.Generic;

namespace WEB.SERVICES.Validation.Generic
{
    public class GatewayValidator : AbstractValidator<GatewayDto>
    {
        private readonly IRepository<Gateway> _gatewayRepository;
        public GatewayValidator(IRepository<Gateway> gatewayRepository)
        {
            _gatewayRepository = gatewayRepository;
            RuleFor(x => x.Method)
                .IsInEnum().WithMessage("Invalid method. Must be one of: 1-Get, 2-Post, 3-Put, 4-Delete.");
            RuleFor(x => x.GatewayUrl)
                .NotEmpty().WithMessage("Gateway URL is required.")
                .MustAsync(BeUniqueGatewayUrl).WithMessage("Gateway URL must be unique");
            RuleFor(x => x.KeyName)
                .NotEmpty().WithMessage("Key name is required.")
                .MustAsync(BeUniqueKeyName).WithMessage("Key name must be unique");
        }
        private async Task<bool> BeUniqueGatewayUrl(string url, CancellationToken ct)
        {
            var existing = (await _gatewayRepository.GetAllAsync(u => u.GatewayUrl == url, ct)).Any();
            return !existing;
        }
        private async Task<bool> BeUniqueKeyName(string keyName, CancellationToken ct)
        {
            var existing = (await _gatewayRepository.GetAllAsync(u => u.KeyName == keyName, ct)).Any();
            return !existing;
        }
    }
}
