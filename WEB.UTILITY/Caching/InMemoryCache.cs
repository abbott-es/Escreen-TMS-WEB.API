using LanguageExt;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace WEB.UTILITY.Caching
{

    public class InMemoryCache : ICache
    {
        private readonly IMemoryCache _memoryCache;

        private readonly ILogger<InMemoryCache> _logger;
        public InMemoryCache(IMemoryCache memoryCache, ILogger<InMemoryCache> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public Task Set<T>(string key, T value, TimeSpan expiresIn)
        {
            _memoryCache.Set(key, value, expiresIn);
            _logger.LogDebug($"Memory Cache added key: {key} expire:{expiresIn.TotalMinutes} minutes");
            return Task.CompletedTask;
        }

        public Task Set<T>(string key, T value)
        {
            _memoryCache.Set(key, value);
            _logger.LogDebug($"Memory Cache added key: {key}");
            return Task.CompletedTask;
        }

        public Task<Option<T>> Get<T>(string key)
        {
            if (_memoryCache.TryGetValue(key, out T value))
            {
                _logger.LogDebug($"Memory Cache fetch key: {key}");
                return Task.FromResult(Option<T>.Some(value));
            }
            return Task.FromResult(Option<T>.None);
        }

        public Task Remove(string key)
        {
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }

        /// <summary>
        /// No custom prefix for InMemoryCache
        /// </summary>
        /// <param name="keyPrefix"></param>
        /// <returns></returns>
        public Task RemoveByPrefix(string keyPrefix)
        {
            return Task.CompletedTask;
        }
    }
}
