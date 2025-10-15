using WEB.GATEWAY.Models;

namespace WEB.GATEWAY.Interfaces;

public interface IOutputCacheService
{
    OutputCachePolicy? GetPolicy(string name);
}