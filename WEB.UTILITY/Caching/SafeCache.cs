using LanguageExt;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.UTILITY.Caching
{
    public class SafeCache : ICache
    {
        private readonly ICache _cache;
        private readonly ILogger<SafeCache> _logger;

        public SafeCache(ICache cache, ILogger<SafeCache> logger)
        {
            _cache = cache;
            _logger = logger;
        }
        public async Task Remove(string key)
        {
            try
            {
                await _cache.Remove(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear cache. Key: {key}.", key);
            }
        }

        public async Task RemoveByPrefix(string keyPrefix)
        {
            try
            {
                await _cache.RemoveByPrefix(keyPrefix);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear cache by key prefix. Key prefix: {keyPrefix}.", keyPrefix);
            }
        }

        public async Task<Option<T>> Get<T>(string key)
        {
            try
            {
                return await _cache.Get<T>(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get by cache. Key: {key}.", key);
                return default;
            }
        }

        public async Task Set<T>(string key, T value)
        {
            try
            {
                await _cache.Set(key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set cache. Key: {key}.", key);
            }
        }

        public async Task Set<T>(string key, T value, TimeSpan expiresIn)
        {
            try
            {
                await _cache.Set(key, value, expiresIn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set cache. Key: {key}.", key);
            }
        }
    }
}
