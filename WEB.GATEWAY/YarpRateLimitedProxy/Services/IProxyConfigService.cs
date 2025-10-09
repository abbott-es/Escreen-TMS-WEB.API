using YarpRateLimitedProxy.Models;

namespace YarpRateLimitedProxy.Services
{
    public interface IProxyConfigService
    {
        ProxyConfigData Current { get; }
    }
}
