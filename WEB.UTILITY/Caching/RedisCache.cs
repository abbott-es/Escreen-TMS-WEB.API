using LanguageExt;
using Microsoft.Extensions.Logging;
using StackExchange.Redis.Extensions.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.UTILITY.Caching
{
    public class RedisCache : ICache
    {
        private readonly IRedisDatabase _db;
        private readonly IRedisClient _client;
        private readonly ILogger<RedisCache> _logger;

        public RedisCache(
            IRedisDatabase db,
            IRedisClient client,
            ILogger<RedisCache> logger)
        {
            _db = db;
            _client = client;
            _logger = logger;
        }
        public async Task<Option<T>> Get<T>(string key)
        {
            return await _db.GetAsync<T>(key);
        }

        public async Task Set<T>(string key, T value)
        {
            await _db.AddAsync<T>(key, value);
        }

        public async Task Set<T>(string key, T value, TimeSpan expiresIn)
        {
            await _db.AddAsync<T>(key, value, expiresIn);
        }

        public async Task Remove(string key)
        {
            await _db.RemoveAsync(key);
        }

        public async Task RemoveByPrefix(string keyPrefix)
        {
            var allKeys = (await _db.SearchKeysAsync($"{keyPrefix}*")).ToArray();
            if (allKeys.Count() > 0)
            {
                var group = allKeys.GroupBy(k => _client.ConnectionPoolManager.GetConnection().GetHashSlot(k));
                foreach (var keyGroup in group)
                {
                    _logger.LogInformation("Removing keys from slot {Slot}", keyGroup.Key);
                    await _db.RemoveAllAsync(keyGroup.ToArray());
                    _logger.LogInformation("Removed keys {keys}", string.Join(";", keyGroup));
                }
            }
        }
    }
}
